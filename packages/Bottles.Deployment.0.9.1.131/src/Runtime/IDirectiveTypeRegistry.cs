using System;
using System.Collections.Generic;

namespace Bottles.Deployment.Runtime
{
    public interface IDirectiveTypeRegistry
    {
        Type DirectiveTypeFor(string name);
        IEnumerable<Type> DirectiveTypes();
    }
}