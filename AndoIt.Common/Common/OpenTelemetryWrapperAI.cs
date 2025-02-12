using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using Microsoft.Extensions.Logging;
using System;
using AndoIt.Common.Core.Common;

public class OpenTelemetryWrapperAI
{
    private readonly ILogger<Object> wrappedLog;
    private readonly ActivitySource tracer;
    private readonly TracerProvider tracerProvider;

    public OpenTelemetryWrapperAI(Uri collectorUri)
    {
        // 1️⃣ Configurar logging
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

        // 2️⃣ Configurar tracing correctamente
        this.tracer = new ActivitySource("Common.OpenTelemetryWrapper");

        this.tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetSampler(new AlwaysOnSampler())
            .AddSource("Common.OpenTelemetryWrapper")  // 📌 Necesario para capturar spans
            .AddConsoleExporter()
            .AddOtlpExporter(exporterOptions =>
            {
                exporterOptions.Endpoint = collectorUri;
                exporterOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            })
            .Build();
    }

    public void StartTrace(object objToTrace)
    {
        using (var activity = this.tracer.StartActivity("TrazaConObjetoEnriquecido", ActivityKind.Server))
        {
            this.wrappedLog.LogInformation("Se inició un nuevo trace.");
            if (activity == null)
            {
                this.wrappedLog.LogError("NO se inició un nuevo trace. ❌ No se pudo iniciar la actividad.");
                Console.WriteLine("❌ Error: No se pudo iniciar la actividad.");
                return;
            }

            activity.SetTag("TagManual", "Es manual");
            // Loguear las tags para ver qué se está asignando
            Console.WriteLine($"Trace iniciado: {activity.DisplayName}");
            foreach (var tag in activity.Tags)
            {
                Console.WriteLine($"Tag: {tag.Key} = {tag.Value}");
            }

            // Esto ya no es de la IA
            var keyValues = objToTrace.GetKeyValue();
            foreach (var pair in keyValues)
            {
                activity?.SetTag(pair.Key, pair.Value);
            }
            // End IA

            this.wrappedLog.LogInformation($"El objeto que tendría que salir por el trace: {Newtonsoft.Json.JsonConvert.SerializeObject(objToTrace)}");
        }
    }
}
