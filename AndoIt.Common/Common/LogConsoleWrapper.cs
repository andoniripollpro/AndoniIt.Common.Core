using AndoIt.Common.Common;
using AndoIt.Common.Interface;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Reflection;

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
		public void Debug(string message, StackTrace stackTrace = null, params object[] paramValues)
		{
            if (stackTrace != null) message = $"{stackTrace.ToStringClassMethod()}: {message}";
            if (paramValues != null && paramValues.Length > 0)
                message += $"{Environment.NewLine}Params: {ParamsToString(stackTrace.GetFrame(0).GetMethod(), paramValues)}";
            if (this.logLevel <= LogLevel.Debug)
				Console.WriteLine($"Debug: {stackTrace.ToStringClassMethod()}: {message}");
		}

        private string ParamsToString(MethodBase method, params object[] values)
        {
            ParameterInfo[] parms = method.GetParameters();
            object[] namevalues = new object[2 * parms.Length];

            string msg = "(";
            for (int i = 0, j = 0; i < parms.Length; i++, j += 2)
            {
                msg += "{" + j + "}={" + (j + 1) + "}, ";
                namevalues[j] = parms[i].Name;
                if (i < values.Length) namevalues[j + 1] = values[i];
            }
            msg += ")";
            return string.Format(msg, namevalues);
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
