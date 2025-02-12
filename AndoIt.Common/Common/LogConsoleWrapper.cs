using AndoIt.Common.Common;
using AndoIt.Common.Interface;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace AndoIt.Common
{
	public class LogConsoleWrapper : ILog, IDisposable
	{
		private readonly LogLevel logLevel;
		private readonly ILog incidenceEscalator;

        public LogConsoleWrapper(LogLevel logLevel, ILog incidenceEscalator = null)
		{
			this.logLevel = logLevel;
            this.incidenceEscalator = incidenceEscalator;
        }
				
		public void Fatal(string message, Exception exception = null, StackTrace stackTrace = null, params object[] paramValues)
		{
			Console.WriteLine($"FATAL: {stackTrace.ToStringClassMethod()}: {message} Exception: {exception?.ToString()}");
		}		
		public void Error(string message, Exception exception = null, StackTrace stackTrace = null, params object[] paramValues)
		{
			if (this.logLevel <= LogLevel.Error)
				Console.WriteLine($"ERROR: {stackTrace.ToStringClassMethod()}: {message} Exception: {exception?.ToString()}");
		}				
		public void Warn(string message, Exception exception = null, StackTrace stackTrace = null)
		{
			if (this.logLevel <= LogLevel.Warn)
				Console.WriteLine($"WARN: {stackTrace.ToStringClassMethod()}: {message} Exception: {exception?.ToString()}");
		}		
		public void Info(string message, StackTrace stackTrace = null)
		{
			if (this.logLevel <= LogLevel.Info)
				Console.WriteLine($"INFO: {stackTrace.ToStringClassMethod()}: {message}");
		}
		public void InfoSafe(string message, StackTrace stackTrace)
		{
			if (this.logLevel <= LogLevel.Info)
				Console.WriteLine($"INFO: {stackTrace.ToStringClassMethod()}: {message}");
		}
		public void DebugSafe(string message, StackTrace stackTrace)
		{
			if (this.logLevel <= LogLevel.Debug)
				Console.WriteLine($"Debug: {stackTrace.ToStringClassMethod()}: {message}");
		}
		private string SafeCleanForbiddenWords(string message)
		{
			return string.Empty;
		}
		public void Debug(string message, StackTrace stackTrace = null)
		{
			if (this.logLevel <= LogLevel.Debug)
				Console.WriteLine($"Debug: {stackTrace.ToStringClassMethod()}: {message}");
		}
		public void Dispose()
		{
		}
        public void InfoObject(object objectToTrace)
        {
            this.incidenceEscalator?.InfoObject(objectToTrace);
            this.Warn($"InfoObject no pensado para este objeto log: {JsonConvert.SerializeObject(objectToTrace)}");
        }

        public enum LogLevel
		{ 
			Fatal = 5,
			Error = 4,
			Warn = 3,
			Info = 2,
			Debug = 1
		}
	}
}
