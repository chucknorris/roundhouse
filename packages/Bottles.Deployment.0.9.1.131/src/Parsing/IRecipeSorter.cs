using System.Collections.Generic;

namespace Bottles.Deployment.Parsing
{
    public interface IRecipeSorter
    {
        IEnumerable<Recipe> Order(IEnumerable<Recipe> recipes);
    }
}