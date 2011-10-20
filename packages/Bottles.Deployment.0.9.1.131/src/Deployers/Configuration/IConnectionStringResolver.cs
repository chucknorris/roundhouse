namespace Bottles.Deployment.Deployers.Configuration
{
    public interface IConnectionStringResolver
    {
        void Resolve(string filename);
    }
}