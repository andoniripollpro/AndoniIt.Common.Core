using System;

namespace AndoIt.Common.Interface
{
    public abstract class IEnqueable : IEquatable<IEnqueable>
    {
        public abstract string Client { get; set; }
        public abstract string Id { get; }
        public abstract DateTime WhenToHandleNextUtc { get; }
        public EnqueableState State { get; protected set; } = EnqueableState.Pending;
        public abstract string ReplyTo { get; set; }
        public bool DeletedFromQueue { get; set; } = false;

        public abstract void Handle();
        public bool Equals(IEnqueable enqueable)
        {
            return this.Id == enqueable.Id;
        }

        public enum EnqueableState
        {
            Pending,
            HandledOk,
            Error
        }
    }
}