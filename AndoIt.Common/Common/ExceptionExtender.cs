using System;

namespace AndoIt.Common
{
	public static class ExceptionExtender
	{
		public static string AllMessages(this Exception ex)
		{
			string message = ex.Message;
			if (ex.InnerException != null)
				return $"{message}{Environment.NewLine}-->{ex.InnerException.AllMessages()}";
			else
				return message;
		}
	}
}
