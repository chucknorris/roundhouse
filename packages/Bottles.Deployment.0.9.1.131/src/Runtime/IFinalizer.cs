namespace Bottles.Deployment.Runtime
{
    public interface IFinalizer<T> : IDeploymentAction<T> where T : IDirective
    {

    }
}