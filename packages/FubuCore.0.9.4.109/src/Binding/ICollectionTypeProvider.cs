using System;

namespace FubuCore.Binding
{
    public interface ICollectionTypeProvider
    {
        Type GetCollectionType(Type interfaceType, Type firstGenericArgument);
    }
}