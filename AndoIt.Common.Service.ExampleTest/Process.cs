using AndoIt.Common.Service.Interface;
using NLog;
using System.Threading;

namespace AndoIt.Common.Service.ExampleTest
{
    public class Process : IProcess
    {
        private const string LOGGER_NAME = "standardLogFile";

        void IProcess.Do()
        {
            Logger logger = LogManager.GetLogger(Process.LOGGER_NAME);
            logger.Info("Process.Do start");
            Thread.Sleep(1000 * 60 * 1);
            //throw new System.Exception("Probando un error en el servicio");
            logger.Info("Process.Do end");
        }

        void IProcess.Dispose()
        {
            Logger logger = LogManager.GetLogger(Process.LOGGER_NAME);
            logger.Info("Process.Dispose start");
        }
    }
}
