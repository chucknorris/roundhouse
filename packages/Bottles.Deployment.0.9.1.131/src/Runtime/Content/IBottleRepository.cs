using System.Collections.Generic;

namespace Bottles.Deployment.Runtime.Content
{
    public interface IBottleRepository
    {
        void CopyTo(string bottleName, string destination);
        void ExplodeTo(string bottleName, string destination);

        void ExplodeFiles(BottleExplosionRequest request);

        PackageManifest ReadManifest(string bottleName);

        void AssertAllBottlesExist(IEnumerable<string> names);
    }
}