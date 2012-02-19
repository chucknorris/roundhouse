using System;
using System.Collections.Generic;
using System.Linq;
using roundhouse.infrastructure.app;
using roundhouse.folders;
using roundhouse.infrastructure.logging;
using roundhouse.infrastructure.filesystem;
using System.IO;

namespace roundhouse.workflow
{
    public class WorkflowProvider
    {
        public WorkflowProvider(ConfigurationPropertyHolder configuration, KnownFolders knownFolders, FileSystemAccess fileSystem)
        {
            _configuration = configuration;
            _knownFolders = knownFolders;
            _fileSystem = fileSystem;
        }

        public IEnumerable<MigrationsFolder> GetFolders()
        {
            if (string.IsNullOrEmpty(_configuration.WorkflowConfigFile))
            {
                Log.bound_to(this).log_an_info_event_containing("Using default RoundhousE workflow.");
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
            else
            {
                Log.bound_to(this).log_an_info_event_containing("Using custom workflow from configuration file {0}.", _configuration.WorkflowConfigFile);
                return LoadFromConfigFile(_configuration.WorkflowConfigFile);
            }
        }

        private IEnumerable<MigrationsFolder> LoadFromConfigFile(string configFile)
        {
            var lineNum = 0;
            var folders = new List<MigrationsFolder>();
            using (var stream = LoadConfigFile(configFile))
            using (var reader = new StreamReader(stream))
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                    break;
                if (line.Trim() == string.Empty)
                    continue;

                lineNum++;
                var folder = ParseLineIntoFolder(line, lineNum);
                folders.Add(folder);
            }
            return folders;
        }

        private FileStream LoadConfigFile(string configFile)
        {
            if (_fileSystem.file_exists(configFile) == false)
            {
                throw new ArgumentException(string.Format(
                    "Custom workflow configuration file {0} does not exist.", configFile));
            }

            try
            {
                return _fileSystem.open_file_in_read_mode_from(configFile);
            }
            catch (IOException e)
            {
                throw new IOException(string.Format(
                    "Unable to read custom workflow configuration file {0}.", configFile), e);
            }
        }

        private MigrationsFolder ParseLineIntoFolder(string line, int lineNum)
        {
            var options = line.Split(',').Select(option => option.Trim()).ToArray();
            if (options.Length != 3)
            {
                throw new ArgumentException(string.Format(
                    "Workflow configuration file: line {0} is invalid. " +
                    "Expected the following format: Friendly Name, Folder Name, Timing", lineNum));
            }
            var friendly = options[0];
            var foldername = options[1];
            var timing = options[2].ToLowerInvariant();
            switch(timing)
            {
                case "onetime": 
                    return new DefaultMigrationsFolder(_fileSystem, 
                        _configuration.SqlFilesDirectory,
                        foldername, true, false, friendly);
                case "anytime":
                    return new DefaultMigrationsFolder(_fileSystem, 
                        _configuration.SqlFilesDirectory,
                        foldername, false, false, friendly);
                case "everytime":
                    return new DefaultMigrationsFolder(_fileSystem, 
                        _configuration.SqlFilesDirectory,
                        foldername, false, true, friendly);
                default:
                    throw new ArgumentException(string.Format(
                        "Workflow configuration file: line {0} is invalid. " +
                        "Expected timing to be onetime, anytime, or everytime. " +
                        "Instead received {1}.", lineNum, timing));
            }
        }

        private readonly ConfigurationPropertyHolder _configuration;
        private readonly KnownFolders _knownFolders;
        private readonly FileSystemAccess _fileSystem;
    }
}
