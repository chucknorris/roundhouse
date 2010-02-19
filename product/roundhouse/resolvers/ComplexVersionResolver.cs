using System.Collections.Generic;

namespace roundhouse.resolvers
{
    public sealed class ComplexVersionResolver : VersionResolver
    {
        private readonly IEnumerable<VersionResolver> resolvers;

        public ComplexVersionResolver(IEnumerable<VersionResolver> resolvers)
        {
            this.resolvers = resolvers;
        }

        public bool meets_criteria()
        {
            return true;
        }

        public string resolve_version()
        {
            string version = "0";
            foreach (VersionResolver resolver in resolvers)
            {
                if (resolver.meets_criteria())
                {
                    version = resolver.resolve_version();
                }
            }

            return version;
        }
    }
}