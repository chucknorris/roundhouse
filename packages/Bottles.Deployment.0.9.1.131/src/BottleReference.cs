namespace Bottles.Deployment
{
    public class BottleReference
    {
        public static BottleReference ParseFrom(string text)
        {
            var name = text.Substring(ProfileFiles.BottlePrefix.Length).Trim();
            return new BottleReference(name);
        }

        public string Name { get; set; }

        public BottleReference()
        {
        }

        public BottleReference(string name)
        {
            Name = name;
        }

        public bool Equals(BottleReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (BottleReference)) return false;
            return Equals((BottleReference) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("{0}{1}", ProfileFiles.BottlePrefix, Name);
        }
    }
}