using System.Collections.Generic;
using FubuCore.DependencyAnalysis;

namespace Bottles.Deployment.Parsing
{
    public class RecipeSorter : IRecipeSorter
    {
        public IEnumerable<Recipe> Order(IEnumerable<Recipe> recipes)
        {
            var graph = new DependencyGraph<Recipe>(r => r.Name, r => r.Dependencies);
            recipes.Each(graph.RegisterItem);
            return graph.Ordered();
        }
    }
}