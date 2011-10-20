using System.Collections.Generic;
using System.Linq;

namespace FubuCore.DependencyAnalysis
{
    public class Cycle
    {
        List<Node> _nodes;

        public Cycle()
        {
            _nodes = new List<Node>();
        }

        public string Name { get { return _nodes.Select(n => n.Name.ToString()).Aggregate((current, next) => string.Format("{0}->{1}", current, next)); } }

        public int Count
        {
            get { return _nodes.Count; }
        }

        public void Add(Node node)
        {
            _nodes.Add(node);
        }
    }
}