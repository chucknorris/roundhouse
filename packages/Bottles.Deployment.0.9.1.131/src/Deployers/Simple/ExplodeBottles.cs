using FubuCore;

namespace Bottles.Deployment.Deployers.Simple
{
    public class ExplodeBottles : IDirective
    {
        public ExplodeBottles()
        {
            WebContentDirectory = string.Empty;
            BinDirectory = "bin";
        }

        public string RootDirectory { get; set; }
        public string WebContentDirectory { get; set; }
        public string DataDirectory { get; set; }
        public string BinDirectory { get; set; }

        public override string ToString()
        {
            return "Expolding bottles to '{0}'".ToFormat(RootDirectory);
        }
    }

    
}