using System;

namespace roundhouse.connections
{
    public interface IConnection<T> : IDisposable
    {
        void open();
        void close();
        T underlying_type();
    }
}