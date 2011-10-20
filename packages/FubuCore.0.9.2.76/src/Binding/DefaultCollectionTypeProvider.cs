using System;
using System.Collections.Generic;

namespace FubuCore.Binding
{
    public class DefaultCollectionTypeProvider : ICollectionTypeProvider
    {
        public Type GetCollectionType(Type interfaceType, Type firstGenericArgument)
        {
            return typeof(List<>).MakeGenericType(firstGenericArgument);
        }
    }
}