using System;

namespace Bottles.Deployment
{
    public class DependsOnAttribute : Attribute
    {
        public DependsOnAttribute(Type dependentType)
        {
            DependentType = dependentType;
        }

        public Type DependentType { get; private set; }
    }
}