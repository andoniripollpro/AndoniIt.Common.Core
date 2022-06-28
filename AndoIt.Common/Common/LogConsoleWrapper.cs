using AndoIt.Common.Interface;
using System;
using System.Diagnostics;

namespace AndoIt.Common
{
	public class LogConsoleWrapper : ILog, IDisposable
	{
		private readonly LogLevel logLevel;

		public LogConsoleWrapper(LogLevel logLevel)
		{
			this.logLevel = logLevel;
		}
				
		public void Fatal(string message, Exception exception = null, StackTrace stackTrace = null, params object[] paramValues)
		{
			Console.WriteLine($"FATAL: {message} Exception: {exception?.ToString()}");
		}		
		public void Error(string message, Exception exception = null, StackTrace stackTrace = null, params object[] paramValues)
		{
			if (this.logLevel <= LogLevel.Error)
				Console.WriteLine($"ERROR: {message} Exception: {exception?.ToString()}");
		}				
		public void Warn(string message, Exception exception = null, StackTrace stackTrace = null)
		{
			if (this.logLevel <= LogLevel.Warn)
				Console.WriteLine($"WARN: {message} Exception: {exception?.ToString()}");
		}		
		public void Info(string message, StackTrace stackTrace = null)
		{
			if (this.logLevel <= LogLevel.Info)
				Console.WriteLine($"INFO: {message}");
		}
		public void InfoSafe(string message, StackTrace stackTrace)
		{
			if (this.logLevel <= LogLevel.Info)
				Console.WriteLine($"INFO: {message}");
		}
		public void DebugSafe(string message, StackTrace stackTrace)
		{
			if (this.logLevel <= LogLevel.Debug)
				Console.WriteLine($"Debug: {message}");
		}
		private string SafeCleanForbiddenWords(string message)
		{
			return string.Empty;
		}
		public void Debug(string message, StackTrace stackTrace = null)
		{
			if (this.logLevel <= LogLevel.Debug)
				Console.WriteLine($"Debug: {message}");
		}
		public void Dispose()
		{
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
