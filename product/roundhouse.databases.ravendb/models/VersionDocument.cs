using System.Collections.Generic;
using roundhouse.model;

namespace roundhouse.databases.ravendb.models
{
    public class VersionDocument
    {
        public VersionDocument()
        {
            Versions = new List<Version>();
        }

        public List<Version> Versions { get; set; }
    }
}