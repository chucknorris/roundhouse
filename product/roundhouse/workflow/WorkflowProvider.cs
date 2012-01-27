using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using roundhouse.infrastructure.app;
using roundhouse.folders;

namespace roundhouse.workflow
{
    public class WorkflowProvider
    {
        public WorkflowProvider(ConfigurationPropertyHolder configuration, KnownFolders knownFolders)
        {
            _configuration = configuration;
            _knownFolders = knownFolders;
        }

        public IEnumerable<MigrationsFolder> GetFolders()
        {
            if (string.IsNullOrEmpty(_configuration.WorkflowConfigFile))
            {
                return new[] {
                    _knownFolders.up,
                    _knownFolders.run_first_after_up,
                    _knownFolders.functions,
                    _knownFolders.views,
                    _knownFolders.sprocs,
                    _knownFolders.indexes,
                    _knownFolders.run_after_other_any_time_scripts
               };
            }
            return new MigrationsFolder[0];
        }

        private readonly ConfigurationPropertyHolder _configuration;
        private readonly KnownFolders _knownFolders;
    }
}
