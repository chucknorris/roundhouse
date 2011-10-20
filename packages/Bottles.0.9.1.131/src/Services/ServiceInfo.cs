namespace Bottles.Services
{
    /// <summary>
    /// file look like
    ///  Bootstrapper=the type name
    ///  Name=Name
    /// </summary>
    public class ServiceInfo
    {
        public static readonly string FILE = "svc.txt";
        
        public string Name { get; set; }
        public string Bootstrapper { get; set; }
    }
}