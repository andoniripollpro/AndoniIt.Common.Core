using System.Diagnostics;

namespace AndoIt.Common.Common
{
	public static class StackTraceExtender
	{
		public static string ToStringClassMethod(this StackTrace stackTrace)
		{			
			//var callingStackTraceFrame = new StackTrace().GetFrame(2);
			var callingStackTraceFrame = stackTrace?.GetFrame(0);
			return $"{callingStackTraceFrame?.GetMethod().ReflectedType.Name}.{callingStackTraceFrame?.GetMethod().Name}";
		}
	}
}