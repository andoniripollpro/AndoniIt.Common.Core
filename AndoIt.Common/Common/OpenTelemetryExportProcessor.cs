//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Threading;
//using System.Threading.Tasks;
//using OpenTelemetry;
//using OpenTelemetry.Exporter;
//using OpenTelemetry.Trace;

//public class OpenTelemetryExportProcessor : BaseExportProcessor<Activity>
//{
//    private readonly ZipkinExporter _exporter;

//    public OpenTelemetryExportProcessor(ZipkinExporter exporter)
//    {
//        _exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
//    }

//    public override async Task<ExportResult> ExportAsync(
//        Batch<Activity> batch,
//        CancellationToken cancellationToken)
//    {
//        try
//        {
//            // Export spans using the exporter
//            await _exporter.ExportAsync(batch, cancellationToken);
//            return ExportResult.Success;
//        }
//        catch (Exception ex)
//        {
//            // Handle any exceptions that occur during export
//            Console.WriteLine($"Failed to export spans: {ex}");
//            return ExportResult.Failure;
//        }
//    }

//    public override Task ShutdownAsync(CancellationToken cancellationToken)
//    {
//        // Perform any necessary cleanup or shutdown operations
//        _exporter?.ShutdownAsync(cancellationToken);
//        return Task.CompletedTask;
//    }

//    //--- Generado para implementar clase abstracta
//    protected override void OnExport(Activity data)
//    {
//        throw new NotImplementedException();
//    }
//}