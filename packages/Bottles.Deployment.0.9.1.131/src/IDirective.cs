namespace Bottles.Deployment
{
    public interface IDirective
    {
    }

    public interface IDirectiveWithRoot : IDirective
    {
        string ApplicationRootDirectory();
    }
}