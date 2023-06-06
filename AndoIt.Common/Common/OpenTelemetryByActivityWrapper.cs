using AndoIt.Common.Common;
using AndoIt.Common.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace AndoIt.Common
{
    public class OpenTelemetryByActivityWrapper : ILog, IDisposable
    {
        private readonly Activity? wrappedLog;
        private readonly ILog incidenceEscalator;
        public readonly List<string> forbiddenWords = new List<string>();

        public const string APP_NAME = "ServicioLiveToFile";

        public string FORBIDDEN_WORD_CHARACTERS = "XXXXXXXXXXXXXXXXXXXX";

        public OpenTelemetryByActivityWrapper(Activity? wrappedLog, ILog incidenceEscalator = null, List<string> forbiddenWords = null)
        {
            //using var loggerFactory = LoggerFactory.Create(builder =>
            //{
            //    builder.AddOpenTelemetry(options =>
            //    {
            //        options.AddConsoleExporter();
            //    });
            //});

            //var logger = loggerFactory.CreateLogger<object>();

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
            this.wrappedLog.SetStatus(ActivityStatusCode.Error, $"FATAL: {message}{Environment.NewLine}Exception: {exception}");//, paramValues); // Sin parámetros
        }
        public void Error(string message, Exception exception = null, StackTrace stackTrace = null, params object[] paramValues)
        {
            this.incidenceEscalator?.Error(message, exception, stackTrace);
            if (stackTrace != null) message = $"{stackTrace.ToStringClassMethod()}: {message}{Environment.NewLine}"
                    + $"Params: {ParamsToString(stackTrace.GetFrame(0).GetMethod(), paramValues)}{Environment.NewLine}{stackTrace.ToString()}";
            this.wrappedLog.SetStatus(ActivityStatusCode.Error, $"ERROR: {message}{Environment.NewLine}Exception: {exception}");//, paramValues); // Sin parámetros
        }
        public void Warn(string message, Exception exception = null, StackTrace stackTrace = null)
        {
            this.incidenceEscalator?.Warn(message, exception, stackTrace);
            if (stackTrace != null) message = $"{stackTrace.ToStringClassMethod()}: {message}{Environment.NewLine}{stackTrace.ToString()}";
            this.wrappedLog.SetStatus(ActivityStatusCode.Ok, $"WARN: {message}{Environment.NewLine}Exception: {exception}"); 
        }
        public void Info(string message, StackTrace stackTrace = null)
        {
            this.incidenceEscalator?.Info(message, stackTrace);
            if (stackTrace != null) message = $"{stackTrace.ToStringClassMethod()}: {message}";
            this.wrappedLog.SetStatus(ActivityStatusCode.Ok, $"INFO: {message}");
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
            this.wrappedLog.SetStatus(ActivityStatusCode.Ok, $"DEBUG: {message}");
        }
        public void Dispose()
        {
            //  En reoría no hay que hacer nada
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
    }
}
