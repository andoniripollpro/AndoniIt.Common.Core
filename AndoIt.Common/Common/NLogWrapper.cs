using AndoIt.Common.Common;
using AndoIt.Common.Interface;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace AndoIt.Common
{
	public class NLogWrapper: ILog, IDisposable
	{		
		private readonly Logger wrappedLog;
		private readonly ILog incidenceEscalator;
		public readonly List<string> forbiddenWords = new List<string>();

		public const string APP_NAME = "ServicioLiveToFile";

		public string FORBIDDEN_WORD_CHARACTERS = "XXXXXXXXXXXXXXXXXXXX";

        public Logger WrappedLog => wrappedLog;

        public NLogWrapper(Logger wrappedLog, ILog incidenceEscalator = null, List<string> forbiddenWords = null)
		{
			this.wrappedLog = wrappedLog ?? throw new ArgumentNullException("wrappedLog");			
			this.incidenceEscalator = incidenceEscalator;
			if (forbiddenWords != null)
			{
				this.forbiddenWords.AddRange(forbiddenWords);
			}
		}
				
		public void Fatal(string message, Exception exception = null, StackTrace stackTrace = null, params object[] paramValues)
		{
			this.incidenceEscalator?.Fatal(message, exception, stackTrace);
			if (stackTrace != null) message = $"{stackTrace.ToStringClassMethod()}: {message}{Environment.NewLine}" 
					+ $"Params: {ParamsToString(stackTrace.GetFrame(0).GetMethod(), paramValues)}{Environment.NewLine}{stackTrace.ToString()}";
			this.WrappedLog.Fatal(exception, message);			
		}		
		public void Error(string message, Exception exception = null, StackTrace stackTrace = null, params object[] paramValues)
		{
			this.incidenceEscalator?.Error(message, exception, stackTrace);
			if (stackTrace != null) message = $"{stackTrace.ToStringClassMethod()}: {message}{Environment.NewLine}"
					+ $"Params: {ParamsToString(stackTrace.GetFrame(0).GetMethod(), paramValues)}{Environment.NewLine}{stackTrace.ToString()}";
			this.WrappedLog.Error(exception, message);
		}				
		public void Warn(string message, Exception exception = null, StackTrace stackTrace = null)
		{
			this.incidenceEscalator?.Warn(message, exception, stackTrace);
			if (stackTrace != null) message = $"{stackTrace.ToStringClassMethod()}: {message}{Environment.NewLine}{stackTrace.ToString()}";
			this.WrappedLog.Warn(exception, message);
		}		
		public void Info(string message, StackTrace stackTrace = null)
		{
			this.incidenceEscalator?.Info(message, stackTrace);
			if (stackTrace != null) message = $"{stackTrace.ToStringClassMethod()}: {message}";
			this.WrappedLog.Info(message);			
		}
		public void InfoSafe(string message, StackTrace stackTrace)
		{
			message = SafeCleanForbiddenWords(message);
			this.incidenceEscalator?.InfoSafe(message, stackTrace);
			this.Info(message, stackTrace);
		}
		public void DebugSafe(string message, StackTrace stackTrace)
		{
			message = SafeCleanForbiddenWords(message);
			this.incidenceEscalator?.DebugSafe(message, stackTrace);
			this.Debug(message);
		}
		private string SafeCleanForbiddenWords(string message)
		{
			this.forbiddenWords.ForEach(x => message = message.Replace(x, FORBIDDEN_WORD_CHARACTERS));
			return message;
		}
		public void Debug(string message, StackTrace stackTrace = null)
		{
			this.incidenceEscalator?.Debug(message, stackTrace);
			if (stackTrace != null) message = $"{stackTrace.ToStringClassMethod()}: {message}";
			this.WrappedLog.Debug(message);
		}
		public void Dispose()
		{
			LogManager.Flush();
			LogManager.Shutdown();
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
        public void InfoObject(object objectToTrace)
        {
            this.incidenceEscalator?.InfoObject(objectToTrace);
            this.Warn($"InfoObject no pensado para este objeto log: {JsonConvert.SerializeObject(objectToTrace)}");
        }
    }
}
