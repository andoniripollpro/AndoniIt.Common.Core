using System;

namespace AndoIt.Common.Dto
{
	public class Envelope<T>
	{
		public T Content { get; set; }
		public bool Error { get; set; } = false;
		public Exception Exception { get; set; } = null;
		public Version Version { get; set; } = null;
		public string CorrelationId { get; set; } = null;
        public string ReplyTo { get; set; }
    }
}
