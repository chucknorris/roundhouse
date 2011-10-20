using Bottles.Deployment.Diagnostics;
using Bottles.Deployment.Runtime;
using StructureMap.Configuration.DSL;

namespace Bottles.Deployment.Bootstrapping
{
    public class DeploymentRegistry : Registry
    {
        public DeploymentRegistry()
        {
            Scan(x =>
            {
                //TODO: Add diagnostics to the scanning
                x.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.Contains("Deployers"));

                x.Assembly(GetType().Assembly);

                x.ConnectImplementationsToTypesClosing(typeof (IInitializer<>));
                x.ConnectImplementationsToTypesClosing(typeof (IDeployer<>));
                x.ConnectImplementationsToTypesClosing(typeof (IFinalizer<>));

            });

            Scan(x =>
            {
                x.TheCallingAssembly();
                x.WithDefaultConventions();
            });

            // Just need this to be a singleton
            ForSingletonOf<IDeploymentDiagnostics>();
        }
    }
}