using System;
using System.Collections.Generic;

namespace roundhouse.infrastructure.containers.custom
{
    /// <summary>
    /// This is just a silly class that performs a simple type to implementation mapping.
    /// The ultimate goal must be to get rid of the "Service Locator" pattern altogether,
    /// and rather _inject_ the dependencies in the classes that need them, instead of asking the global
    /// <see cref="Container"/> static class to resolve a dependency.
    ///
    /// However, in the mean time, this class removes the dependency on StructureMap, which has issues
    /// runtime when running on .net core (apparently).
    /// 
    /// </summary>
    public sealed class HardcodedContainer : InversionContainer
    {
        private static readonly IDictionary<Type, object> registrations = new Dictionary<Type, object>();

        public static void Register<TDependency>(TDependency implementation)
        {
            registrations[typeof(TDependency)] = implementation;
        }

        TypeToReturn InversionContainer.Resolve<TypeToReturn>()
        {
            return (TypeToReturn) registrations[typeof(TypeToReturn)];
        }

        public static HardcodedContainer Instance { get; } = new HardcodedContainer();
    }
}