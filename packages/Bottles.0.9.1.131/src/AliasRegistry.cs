using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Bottles
{

    [XmlType("aliases")]
    public class AliasRegistry
    {
        public static readonly string ALIAS_FILE = ".bottle-alias";

        private readonly IList<AliasToken> _aliases = new List<AliasToken>();

        public AliasToken[] Aliases
        {
            get { return _aliases.ToArray(); }
            set
            {
                _aliases.Clear();
                _aliases.AddRange(value);
            }
        }

        public void CreateAlias(string alias, string folder)
        {
            var token = AliasFor(alias);
            if (token == null)
            {
                token = new AliasToken
                {
                    Folder = folder,
                    Name = alias
                };

                _aliases.Add(token);
            }
            else
            {
                token.Folder = folder;
            }
        }

        public AliasToken AliasFor(string alias)
        {
            return _aliases.SingleOrDefault(x => x.Name == alias);
        }

        public void RemoveAlias(string alias)
        {
            _aliases.RemoveAll(x => x.Name == alias);
        }
    }

    [XmlType("alias")]
    public class AliasToken
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("folder")]
        public string Folder { get; set; }
    }
}