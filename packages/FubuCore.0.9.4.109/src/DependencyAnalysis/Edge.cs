using System.Diagnostics;

namespace FubuCore.DependencyAnalysis
{
    [DebuggerDisplay("{Source}->{Target}")]
    public class Edge
    {
       
        public Edge(Node source, Node target)
        {
            //source depends on target
            Source = source;
            Target = target;
        }


        public Node Source { get; private set; }
        public Node Target { get; private set; }

        public bool Equals(Edge other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Source, Source) && Equals(other.Target, Target);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Edge)) return false;
            return Equals((Edge) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Source != null ? Source.GetHashCode() : 0)*397) ^ (Target != null ? Target.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format("Edge: {0}->{1}", Source, Target);
        }
    }
}