using System;
using System.Diagnostics;

namespace AndoIt.Common.Interface
{
	public interface ILog
	{
		void Fatal(string message, Exception exception = null, StackTrace stackTrace = null, params object[] paramValues);
		void Error(string message, Exception exception = null, StackTrace stackTrace = null, params object[] paramValues);
		void Warn(string message, Exception exception = null, StackTrace stackTrace = null);			
		void Info(string message, StackTrace stackTrace = null);
		void InfoSafe(string message, StackTrace stackTrace);
		void DebugSafe(string message, StackTrace stackTrace);
		void Debug(string message, StackTrace stackTrace = null);        
    }
}