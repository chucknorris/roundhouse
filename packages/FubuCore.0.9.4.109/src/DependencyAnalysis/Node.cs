using System.Diagnostics;

namespace FubuCore.DependencyAnalysis
{
    [DebuggerDisplay("Node:{Name}")]
    public class Node
    {
        public Node(string name)
        {
            Index = -1;
            Name = name;
        }

        public string Name { get; private set; }

        public bool Equals(Node other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Node)) return false;
            return Equals((Node) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public void SetIndex(int index)
        {
            Index = index;
        }

        public void SetLowLink(int lowlink)
        {
            LowLink = lowlink;
        }

        public int Index { get; private set; }
        public int LowLink { get; private set; }
    }
}