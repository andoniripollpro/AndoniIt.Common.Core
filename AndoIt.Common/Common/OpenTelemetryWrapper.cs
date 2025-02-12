using AndoIt.Common.Common;
using AndoIt.Common.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using OpenTelemetry;

namespace AndoIt.Common
{
    public class OpenTelemetryWrapper : ILog, IDisposable
    {
        private const string SOURCE = "OpenTelemetryWrapper";
        private readonly ILogger<object> wrappedLog;
        private readonly Tracer tracer;
        private readonly ActivitySource activitySource;
        private readonly TracerProvider tracerProvider;
        private readonly Uri collectorUri;
        private readonly ILog incidenceEscalator;
        public readonly List<string> forbiddenWords = new List<string>();

        private static readonly ActivitySource ActivitySource = new("MiAplicacionTracer");

        public string FORBIDDEN_WORD_CHARACTERS = "XXXXXXXXXXXXXXXXXXXX";

        public OpenTelemetryWrapper(Uri collectorUri, ILog incidenceEscalator = null, List<string> forbiddenWords = null)
        {
            if (collectorUri == null) throw new ArgumentNullException("collectorUri no puede ser nulo y debe tener la uri del colector de OTEL-OpenTelemetry");
            this.collectorUri = collectorUri;
            this.incidenceEscalator = incidenceEscalator;
            if (forbiddenWords != null)
            {
                this.forbiddenWords.AddRange(forbiddenWords);
            }

            //  Log
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddOpenTelemetry(options =>
                {
                    options.AddOtlpExporter(exporterOptions =>
                    {
                        exporterOptions.Endpoint = collectorUri;
                        exporterOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
                    options.AddConsoleExporter();
                });
            });
            this.wrappedLog = loggerFactory.CreateLogger<Object>();

            this.activitySource = new ActivitySource(SOURCE);

            this.tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetSampler(new AlwaysOnSampler())
                .AddSource(SOURCE)  // 📌 Necesario para capturar spans
                .AddConsoleExporter()
                .AddOtlpExporter(exporterOptions =>
                {
                    exporterOptions.Endpoint = collectorUri;
                    exporterOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                })
                .Build();
            this.tracer = tracerProvider.GetTracer(SOURCE);
        }

        public void Fatal(string message, Exception exception = null, StackTrace stackTrace = null, params object[] paramValues)
        {
            this.incidenceEscalator?.Fatal(message, exception, stackTrace);
            if (stackTrace != null) message = $"{stackTrace.ToStringClassMethod()}: {message}{Environment.NewLine}"
                    + $"Params: {ParamsToString(stackTrace.GetFrame(0).GetMethod(), paramValues)}{Environment.NewLine}{stackTrace.ToString()}";
            message = $"FATAL-CRITICAL (Exception={exception?.Message}): {message}";
            this.wrappedLog.LogCritical(exception, message, paramValues);
            if (exception?.InnerException != null)
            {
                this.Fatal($"INNER EXCEPTION: ", exception?.InnerException);
            }
            EscribeTrazaError(message);
        }
        public void Error(string message, Exception exception = null, StackTrace stackTrace = null, params object[] paramValues)
        {
            this.incidenceEscalator?.Error(message, exception, stackTrace);
            if (stackTrace != null) message = $"{stackTrace.ToStringClassMethod()}: {message}{Environment.NewLine}"
                    + $"Params: {ParamsToString(stackTrace.GetFrame(0).GetMethod(), paramValues)}{Environment.NewLine}{stackTrace.ToString()}";
            message = $"ERROR (Exception={exception?.Message}): {message}";
            this.wrappedLog.LogError(exception, message, paramValues);
            if (exception?.InnerException != null)
            {
                this.Error($"INNER EXCEPTION: ", exception?.InnerException);
            }
            EscribeTrazaError(message);
        }
        public void Warn(string message, Exception exception = null, StackTrace stackTrace = null)
        {
            this.incidenceEscalator?.Warn(message, exception, stackTrace);
            if (stackTrace != null) message = $"{stackTrace.ToStringClassMethod()}: {message}{Environment.NewLine}{stackTrace.ToString()}";
            this.wrappedLog.LogWarning(exception, $"WARN (Exception={exception?.Message}): {message}");
            if (exception?.InnerException != null)
            {
                this.Warn($"INNER EXCEPTION: ", exception?.InnerException);
            }
        }
        public void Info(string message, StackTrace stackTrace = null)
        {
            this.incidenceEscalator?.Info(message, stackTrace);
            if (stackTrace != null) message = $"{stackTrace.ToStringClassMethod()}: {message}";
            this.wrappedLog.LogInformation($"INFO: {message}");
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
            //  TODO: Harcodeado. Como el colector del OTEL no me escribe no los LogDebug ni los LogTrace lo escribo con LogInformation. Con el texto 'DEBUG-TRACE' se debe entender
            //  Cuando controle esto habrá que ponerlo bien.
            this.wrappedLog.LogInformation($"DEBUG-TRACE: {message}");
            //this.wrappedLog.LogDebug($"DEBUG-TRACE: {message}");
        }
        public void EscribeTrazaError(string message)
        {
            // Crear y empezar una nueva traza
            using (var span = this.tracer?.StartActiveSpan($"Error-{DateTime.Now.ToString("yyyy-MMM-dd")}"))
            {
                // Añadir el estado de error al span
                span.SetStatus(Status.Error.WithDescription(message));
            }
        }        
        public void Dispose()
        {
            //   En reoría no hay que hacer nada
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

        public void InfoObject(object objToTrace)
        {
            using (var activity = this.activitySource.StartActivity("TrazaConObjetoEnriquecido", ActivityKind.Server))
            {
                this.wrappedLog.LogInformation("Se inició un nuevo trace.");
                if (activity == null)
                {
                    this.wrappedLog.LogError("NO se inició un nuevo trace. ❌ No se pudo iniciar la actividad.");
                    return;
                }

                activity.SetTag("TagManual", "Es manual");
        
                var keyValues = objToTrace.GetKeyValue();
                foreach (var pair in keyValues)
                {
                    activity?.SetTag(pair.Key, pair.Value);
                }

                this.wrappedLog.LogInformation($"El objeto que tendría que salir por el trace: {Newtonsoft.Json.JsonConvert.SerializeObject(objToTrace)}");
            }
        }
    }
}
