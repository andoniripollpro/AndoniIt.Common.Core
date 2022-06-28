using System;

namespace AndoIt.Common.Interface
{
    public interface IDispenser<T> : IDisposable
    {
        event DispenserHandler<T> Dispense;
        void Listen();
    }

    public delegate void DispenserHandler<T>(object sender, T toBeDispensed);
}
