using OpenTelemetry;
using OpenTelemetry.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Reflection;
//using OpenTelemetry.Instrumentation.Http;

//using OpenTelemetry.Instrumentation.AspNetCore; // => esta ofrece el método AddAspNetCoreInstrumentation 
//using OpenTelemetry.Instrumentation.Http;
//using OpenTelemetry.Exporter.OpenTelemetryProtocol;
//using OpenTelemetry.Exporter.OpenTelemetryProtocol.Logs;
//using OpenTelemetry.Exporter.Console;

namespace AndoIt.Common.Common
{
    public static class OpenTelemetryExtender
    {
        public static void Configure(this TracerProviderBuilder builder, ResourceBuilder resourceBuilder, OpenTelemetryOptions otlOptions, /*WebApplicationBuilder appBuilder,*/ string serviceName)
        {
            builder
                .AddSource(serviceName) //Se suscribe a actividades del Activity Source dado
                .SetResourceBuilder(resourceBuilder) // ID de la entidad que emite telmetría
                .SetSampler(new AlwaysOnSampler()) // Opcional, optimiza envío de trazas procesadores
                .AddAspNetCoreInstrumentation(options => // AutoIntrumentación
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequest = (activity, httpRequest) => // Processor añadiendo tags
                    {
                        activity?.SetTag("UserAgentAttribute", "httpRequest.Headers.UserAgent");
                    };
                })
                .AddHttpClientInstrumentation(options => //Autoinstrumentation
                {
                    options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) => // Processor añadiendo tags
                    {
                        //SetCorrelationId(activity, httpRequestMessage);  //Diferente a como lo tiene Guillermo
                        var system = Assembly.GetExecutingAssembly().GetName().Name;
                        if (activity != null) activity.DisplayName = $"{system}.{httpRequestMessage.Method} {httpRequestMessage.RequestUri?.AbsolutePath}";  //Diferente a como lo tiene Guillermo
                        //activity?.SetDisplayName($"{system}.{httpRequestMessage.Method} {httpRequestMessage.RequestUri?.AbsolutePath}");
                        activity?.SetTag("UserAgentAttribute", system);
                    };
                })
                .AddExporters(); // appBuilder, otlOptions); // exportan telemetria a sistemas externos
        }

        private static TracerProviderBuilder AddExporters(this TracerProviderBuilder builder) //, WebAppplicationBuilder appBuilder, OpenTelemetryOptions otlOptions)
        {
            //var otlpSection = appBuilder.Configuration.GetSection(nameof(ApplicationOptions.Otlp));
            //appBuilder.Services.Configure<OtlpExporterOptions>(otlpSection);
            builder.AddOtlpExporter();
            return builder;
        }
    }

    public class Telemetry
    {
        public static readonly ActivitySource ApiActivitySource = new("MessageConverter");
    }
}
