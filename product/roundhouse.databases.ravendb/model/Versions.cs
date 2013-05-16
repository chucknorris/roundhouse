using System.Collections.Generic;
using System.Linq;
using roundhouse.model;

namespace roundhouse.databases.ravendb.model
{
    public class Versions
    {
        private IList<Version> _items = new List<Version>();

        public IEnumerable<Version> Items
        {
            get { return _items; }
            protected set { _items = value.ToList(); }
        }

        public string GetLastVersionNumber(string repository_path)
        {
            var lastVersion = _items.Where(s => s.repository_path == repository_path)
                                           .OrderByDescending(s => s.modified_date)
                                           .FirstOrDefault();

            return lastVersion != null ? lastVersion.version : null;
        }

        public long AddVersionItem(Version version)
        {
            long highestVersionId = 0;

            if (_items.Any())
            {
                highestVersionId = _items.Max(s => s.id);
            }

            version.id = ++highestVersionId;
            _items.Add(version);

            return highestVersionId;
        }
    }
}