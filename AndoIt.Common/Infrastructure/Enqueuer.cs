using AndoIt.Common.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace AndoIt.Common.Infrastructure
{
	/// <summary>
	/// Thread safe
	/// </summary>
	public class Enqueuer : IEnqueuer
	{		
		private readonly ILog log;
		private readonly IEnqueuerClient client;
		private readonly Timer timer;
		private readonly List<IEnqueable> queue;
		private object toLock = new object();

		public bool Continue { get; set; } = false;
		int IEnqueuer.ConfigTimingSecondsMaxTimerLapse { get; set; } = 600000; //Default cada 10 min
		ReadOnlyCollection<IEnqueable> IEnqueuer.Queue => new ReadOnlyCollection<IEnqueable>(this.queue);
		
		public Enqueuer(ILog log, IEnqueuerClient client)
		{			
			this.log = log ?? throw new ApplicationException("El contenedor de objetos no me da ILog");
			this.log.Info("Start", new StackTrace());
			this. client = client ?? throw new ApplicationException("El contenedor de objetos no me da IEnquerClient");

			this.timer = new Timer(0.1);
			this.timer.AutoReset = false;
			this.timer.Elapsed += OnTimerElapsed;

			this.queue = new List<IEnqueable>();

			this.log.Info("End", new StackTrace());
		}
		
		void IEnqueuer.EnqueuePetitionsFromRepository(List<IEnqueable> equeablesFromRepositry)
		{
			equeablesFromRepositry.ForEach(x => this.Enqueue(x));
		}

		void IEnqueuer.InsertTask(object sender, IEnqueable enqueable)
		{
			this.log.Debug("Start", new StackTrace());
			lock (this.toLock)
			{
				this.log.Info("Locked", new StackTrace());
				InsertTaskInsideLock(enqueable);
				this.log.Debug("Unlocking", new StackTrace());
			}
			this.log.Debug("End", new StackTrace());
		}

		void IEnqueuer.ReplyTasksToClient(object sender, IEnqueable toPocess)
		{
			this.log.Debug("Start", new StackTrace());
			lock (this.toLock)
			{
				this.client.ReplyList(((IEnqueuer)this).Queue.Where(x => x.Client == toPocess.Client).ToList(), toPocess.ReplyTo);
			}
			this.log.Debug("End", new StackTrace());
		}

		void IEnqueuer.DeleteTasks(object sender, IEnqueable toPocess)
		{
			this.log.Debug("Start", new StackTrace());
			lock (this.toLock)
			{
				var toDelete = this.queue.Where(x => x.Id == toPocess.Id).FirstOrDefault();
				if (toDelete != null)
				{
					this.queue.Remove(toPocess);
					toDelete.DeletedFromQueue = true;
					this.log.Info($"Task '{toPocess.Id}' eliminado de la cola.");
					this.client.Persist(toDelete);
				}
			}			
			this.log.Debug("End", new StackTrace());
		}

		private void InsertTaskInsideLock(IEnqueable enqueable)
		{
			try
			{
				var enqueableToBeCancelled = this.queue.Where(x => x.Equals(enqueable)).FirstOrDefault();
				if (enqueableToBeCancelled != null)
				{
					this.log.Warn($"La petición programada {enqueable.Id} ya existe en el servicio. Esta petición se cancelará y la nueva se asumirá como la correcta.");
					this.queue.Remove(enqueableToBeCancelled);				
				}

				this.Enqueue(enqueable);
				this.client.Persist(enqueable);
								
				OnTimerElapsedInsideLock();			
			}
			catch (Exception ex)
			{
				this.log.Error($"{enqueable.Id} ha fallado al intentar entrar en el servicio:{Environment.NewLine}{enqueable.ToString()}",
					ex, new StackTrace());
			}
		}
				
		private void Enqueue(IEnqueable enqueable)
		{
			var insertBeforeThis = this.queue.FirstOrDefault(x => x.WhenToHandleNextUtc > enqueable.WhenToHandleNextUtc);
			if (insertBeforeThis == null)
				this.queue.Add(enqueable);
			else
				this.queue.Insert(this.queue.IndexOf(insertBeforeThis), enqueable);

			this.log.InfoSafe($"{enqueable.Id} insertado en la cola:{Environment.NewLine}{enqueable.Id} {enqueable.State.ToString()} a las {enqueable.WhenToHandleNextUtc} UTC{Environment.NewLine}{enqueable.ToString()}", new StackTrace());
		}

		void IEnqueuer.Process()
		{
			this.log.Info("this.timer.Start();", new StackTrace());
			this.Continue = true;
			this.timer.Start();
		}

		public void OnTimerElapsed(object sender, ElapsedEventArgs e)
		{
			lock (this.toLock)
			{
				OnTimerElapsedInsideLock();
			}
		}

		private void OnTimerElapsedInsideLock()
		{
			try
			{
				List<IEnqueable> listToProcess = GetListToHandle();

				this.log.Info($"{listToProcess.Count} peticiones a procesar ahora. {this.queue.Count} en total", new StackTrace());

				listToProcess.ForEach(x => this.Handle(x));

				SetTimerInterval();
			}
			catch (Exception ex)
			{
				this.log.Fatal("Error inesperado", ex, new StackTrace());
			}
			finally
			{
				if (this.Continue)
				{
					this.timer.Enabled = true;
				}
			}
		}

		private List<IEnqueable> GetListToHandle()
		{
			//	Procesando (Handle) las peticiones cuyo momento ha llegado
			var listToProcess = this.queue.Where(x => x.WhenToHandleNextUtc <= DateTime.Now.ToUniversalTime()).ToList();
			return listToProcess;
		}

		private void SetTimerInterval()
		{
			//	Calculando el nuevo intervalo para que el timer nos despierte
			DateTime? nextTime = (this.queue.Count > 0) ? this.queue.Min(x => x.WhenToHandleNextUtc) : (DateTime?) null;
			var now = DateTime.UtcNow;
			int millisecondsMaxTimerLapse = 1000 * ((IEnqueuer)this).ConfigTimingSecondsMaxTimerLapse;
			if (nextTime == null)   //	Cola vacía
				this.timer.Interval = millisecondsMaxTimerLapse;
			else if (now < nextTime)
			{
				this.timer.Interval = Math.Min((nextTime.Value - now).TotalMilliseconds, Int32.MaxValue);   //	Milisegundos para el siguente lapso
				if (this.timer.Interval > millisecondsMaxTimerLapse)
					this.timer.Interval = millisecondsMaxTimerLapse;
			}
			else
				this.timer.Interval = 0.1;  //	Ya ha pasado el siguente instante. Que haga un lapso lo antes posible

			this.log.Info($"Próximo despertar del timer en {this.timer.Interval} milisegundos", new StackTrace());
		}

		private void Handle(IEnqueable enqueable)
		{
			try
			{
				this.queue.Remove(enqueable);

				if (enqueable.State == IEnqueable.EnqueableState.Pending)
				{
					this.log.Info($"Procesando '{enqueable.Id}'", new StackTrace());
					enqueable.Handle();

					this.client.ReplyOk(enqueable);
					this.Enqueue(enqueable);
				}
				else
				{
					this.log.Info($"'{enqueable.Id}' sale de la cola tras haber agotado su tiempo para hacerle seguimiento", new StackTrace());
				}
			}
			catch (Exception ex)
			{
				string erroMessage = $"Error procesando petición '{enqueable.Id}'.{Environment.NewLine}Contenido: {enqueable.ToString()}";
				this.log.Error(erroMessage, ex, new StackTrace());
				this.client.ReplyError(enqueable, ex, erroMessage);
				this.queue.Add(enqueable);
			}
			finally
			{
				this.client.Persist(enqueable);
				this.log.Info($"Petición {enqueable.Id} Handled");
			}
		}

		void IEnqueuer.Dispose()		
		{
			this.log.Info("Stopping", new StackTrace());
			this.Continue = false;
			this.timer.Stop();
			this.timer.Dispose();
		}		
	}

	public interface IEnqueuerClient
	{
		void ReplyOk(IEnqueable enqueable);
		void ReplyError(IEnqueable enqueable, Exception ex, string errorMessage);
		void ReplyList(List<IEnqueable> enqueable, string replyTo);
		void Persist(IEnqueable enqueable);
	}
}
