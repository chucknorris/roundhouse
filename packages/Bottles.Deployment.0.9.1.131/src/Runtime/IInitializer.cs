namespace Bottles.Deployment.Runtime
{
    public interface IInitializer<T> : IDeploymentAction<T> where T : IDirective
    {

    }
}