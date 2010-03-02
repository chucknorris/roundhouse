namespace roundhouse.databases
{
    using System;

    public interface DatabaseExtension
    {
        bool CanHandleType(string name);
        Type DatabaseTypeToRegisterInContainer();
    }
}