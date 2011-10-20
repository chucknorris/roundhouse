using Bottles.Diagnostics;

namespace Bottles.Environment
{
    public interface IInstaller
    {
        void Install(IPackageLog log);
        void CheckEnvironment(IPackageLog log);
    }


    // Teardown of the environment?
}