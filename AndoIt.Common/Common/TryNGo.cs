using AndoIt.Common.Interface;
using System;
namespace AndoIt.Common.Core.Common
{
    public class TryNGo
    {
        public enum ELogLevel
        {
            Warn,
            Error
        }
        public ELogLevel Level { get; private set; }

        public TryNGo (ELogLevel level = ELogLevel.Warn)
        {
            Level = level;
        }

        public void Do(Action action, ILog log = null)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex) 
            {
                if (log != null)
                {
                    string message = $"Ha saltado una excepción al realizar la acción {action.GetType()} entro de un TryNGo";
                    if (this.Level == ELogLevel.Warn)
                        log.Warn(message, ex);
                    else
                        log.Error(message, ex);
                }
            }
        }
    }
}
