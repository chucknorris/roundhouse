using System;

namespace FubuCore.CommandLine
{
    public abstract class FubuCommand<T> : IFubuCommand<T>
    {
        public Type InputType
        {
            get
            {
                return typeof (T);
            }
        }

        public bool Execute(object input)
        {
            return Execute((T)input);
        }

        public abstract bool Execute(T input);
    }
}