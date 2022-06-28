using AndoIt.Common.Service.Interface;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace AndoIt.Common.Service
{
	public partial class WinService : ServiceBase
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private readonly IProcess process;
		private Task task;
		private CancellationTokenSource cancellationTokenSource;
		private int timesToWaitOneSecondTillProcessIsFinished;
		
		public WinService()
		{
			//InitializeComponent();
			this.components = new System.ComponentModel.Container();

			var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetAssembly(typeof(WinService)).Location);

			this.ServiceName = config.AppSettings.Settings["ServiceName"].Value;

			string assemblyName = config.AppSettings.Settings["assemblyName"].Value;
			string namespaceDotClassName = config.AppSettings.Settings["namespaceDotClassName"].Value;
			ToEventLog($"El servicio '{this.ServiceName}' va a intentar instanciar la clase '{namespaceDotClassName}' que está en el ensamblado '{assemblyName}'", EventLogEntryType.Information);
			this.process = CreateInstance<IProcess>(assemblyName, namespaceDotClassName);

			string timesToWaitOneSecondTillProcessIsFinished = config.AppSettings.Settings["timesToWaitOneSecondTillProcessIsFinished"]?.Value ?? "10";
			this.timesToWaitOneSecondTillProcessIsFinished = int.Parse(timesToWaitOneSecondTillProcessIsFinished);			
		}

		public I CreateInstance<I>(string assemblyName, string namespaceDotClassName) where I : class
		{
			string assemblyPath = $"{Assembly.GetExecutingAssembly().AssemblyDirectory()}\\{assemblyName}";
			
			Assembly assembly = Assembly.LoadFrom(assemblyPath);
			Type type = assembly.GetType(namespaceDotClassName);
			return Activator.CreateInstance(type) as I;			
		}

		protected override void OnStart(string[] args)
		{
			this.cancellationTokenSource = new CancellationTokenSource();
			this.task = Task.Factory.StartNew(() =>
			{
				this.process.Do();
			}, cancellationTokenSource.Token);
		}

		private void ToEventLog(string message, EventLogEntryType eventLogEntryType)
		{
			using (EventLog eventLog = new EventLog("Application"))
			{
				eventLog.Source = Assembly.GetExecutingAssembly().GetName().Name;
				eventLog.WriteEntry(message, eventLogEntryType, 33, 1);	
			}
		}

		protected override void OnStop()
		{
			ToEventLog($"El servicio '{this.ServiceName}' va a intentar detenerse. Se le va a dar {this.timesToWaitOneSecondTillProcessIsFinished} segundos para que el IProcess se detenga", EventLogEntryType.Information);
			Task.Factory.StartNew(() => this.process.Dispose() );
			int i = 0;
			while (!this.task.IsCompleted && i < this.timesToWaitOneSecondTillProcessIsFinished)
			{
				i++;
				Thread.Sleep(1000);	//	1 sec.
			}
			if (!this.task.IsCompleted)
			{
				ToEventLog($"Ha sido necesario interrumpir la tarea por no haber acabado todavía", EventLogEntryType.Warning);
				this.cancellationTokenSource.Cancel();
			}
			this.Dispose();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}

