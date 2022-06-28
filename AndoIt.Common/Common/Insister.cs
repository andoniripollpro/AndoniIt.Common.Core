using AndoIt.Common.Interface;
using System;
using System.Threading;

namespace AndoIt.Common.Common
{
	/// <summary>
	/// Reintenta una función n veces antes de lanzar la ultima excepción que provocaba
	/// </summary>
	public class Insister
	{
		private readonly ILog log;

		public Insister(ILog log = null)
		{
			this.log = log;
		}
		public T Insist<T>(Func<T> func, int retrayes, int millisecondsDelay = 0)
		{
			int tryNumber = 1;
			Exception exception = null;
			do
			{
				try
				{
					T result = func.Invoke();
					exception = null;
					return result;
				}
				catch (Exception ex)
				{
					exception = ex;
					this.log?.Error($"Insister.Insist failed at try number {tryNumber}. Func={func.Target.GetType().Name}.{func.Method.Name}. Retry={tryNumber}", ex);
					Thread.Sleep(millisecondsDelay);
				}
				finally
				{
					tryNumber++;
				}
			} while (exception != null && tryNumber <= retrayes);
			throw exception;
		}
		public void Insist(Action action, int retrayes, int millisecondsDelay = 0)
		{
			int tryNumber = 1;
			Exception exception = null;
			do
			{
				try
				{
					action.Invoke();
					exception = null;
					return;
				}
				catch (Exception ex)
				{
					exception = ex;
					this.log?.Error($"Insister.Insist failed at try number {tryNumber}. Func={action.Target.GetType().Name}.{action.Method.Name}. Retry={tryNumber}", ex);
					Thread.Sleep(millisecondsDelay);
				}
				finally
				{
					tryNumber++;
				}
			} while (exception != null && tryNumber <= retrayes);
			throw exception;
		}
	}
}
