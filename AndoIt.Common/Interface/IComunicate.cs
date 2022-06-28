using System;
using System.Diagnostics;

namespace AndoIt.Common.Interface
{
    public interface IComunicate
    {
        ILog Log { get; }
        void Debug(string message, StackTrace stackTrace);
        void Info(string message, string title);
        void Info(string message, StackTrace stackTrace);
        void Warn(string message, string title);
        void Warn(string message, StackTrace stackTrace);
        void Error(string message, StackTrace stackTrace);
        void Error(string message, Exception ex);
        void Fatal(string message, StackTrace stackTrace);
        void Fatal(string message, Exception ex);
        void Warn(string message, Exception ex);
    }
}
