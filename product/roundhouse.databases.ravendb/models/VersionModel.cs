using System;
using System.Collections.Generic;

namespace roundhouse.databases.ravendb.models
{
    public class VersionDocument
    {
        public VersionDocument()
        {
            Versions = new List<VersionModel>();
        }

        public List<VersionModel> Versions{ get; set; }
    }

    public class VersionModel
    {
        public string Id { get; set; }
        public string RepositoryPath { get; set; }
        public string Version{ get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string EnteredBy { get; set; }
    }

    
}
