using System;
using System.Collections.Generic;
using System.IO;
using Bottles.Deployment.Configuration;
using Bottles.Deployment.Parsing;
using FubuCore;
using FubuCore.Configuration;
using System.Linq;

namespace Bottles.Deployment
{
    public class DeploymentSettings
    {
        //path points to ~/deployment
        public DeploymentSettings(string path)
        {
            _allFolders.Add(path);
            DeploymentDirectory = path;
            BottlesDirectory = FileSystem.Combine(path, ProfileFiles.BottlesDirectory);
            RecipesDirectory = FileSystem.Combine(path, ProfileFiles.RecipesDirectory);
            TargetDirectory = FileSystem.Combine(path, ProfileFiles.TargetDirectory);
            ProfilesDirectory = FileSystem.Combine(path, ProfileFiles.ProfilesDirectory);
            DeployersDirectory = FileSystem.Combine(path, ProfileFiles.DeployersDirectory);
        }

        public DeploymentSettings() : this(".".ToFullPath())
        {
        }

        public string DeployersDirectory { get; set; }

        public string DeploymentDirectory { get; set; }
        public string TargetDirectory { get; set; }
        public string BottlesDirectory { get; set; }
        public string RecipesDirectory { get; set; }

        private readonly IList<string> _allFolders = new List<string>();
        public void AddImportedFolders(IEnumerable<string> folders)
        {
            _allFolders.Fill(folders);
        }

        public void AddImportedFolder(string folder)
        {
            _allFolders.Fill(folder);
        }

        public IEnumerable<string> Directories
        {
            get { return _allFolders; }
        }


        public string EnvironmentFile()
        {
            return new FileSystem().FindFileInDirectories(_allFolders, EnvironmentSettings.EnvironmentSettingsFileName)
                   ?? DeploymentDirectory.AppendPath(EnvironmentSettings.EnvironmentSettingsFileName);
        }

        public string ProfilesDirectory { get; set; }

        public EnvironmentSettings Environment { get; set; }
        public Profile Profile { get; set; }

        public string StagingDirectory
        {
            get { return FileSystem.Combine(TargetDirectory, ProfileFiles.StagingDirectory); }
        }

        public DeploymentPlan Plan { get; set; }

        public static DeploymentSettings ForDirectory(string directory)
        {
            if (directory.IsNotEmpty())
            {
                return new DeploymentSettings(directory);
            }

            var path = FileSystem.Combine(".".ToFullPath(), ProfileFiles.DeploymentFolder);
            return new DeploymentSettings(path);
        }


        public virtual IKeyValues SubstitutionValues()
        {
            return new SettingsRequestData(Plan.Substitutions());
        }


        public string GetRecipeDirectory(string recipe)
        {
            var directories = _allFolders.Select(x => x.AppendPath(ProfileFiles.RecipesDirectory)).ToList();

            return new FileSystem().FindDirectoryInDirectories(directories, recipe)
                   ?? DeploymentDirectory.AppendPath(ProfileFiles.RecipesDirectory, recipe);
        }

        public string GetHost(string recipe, string host)
        {
            var p = GetRecipeDirectory(recipe);

            //TODO: harden
            p = FileSystem.Combine(p, host + ".host");

            return p;
        }

        public string ProfileFileNameFor(string profileName)
        {
            var directories = _allFolders.Select(x => x.AppendPath(ProfileFiles.ProfilesDirectory)).ToList();
            var filename = profileName + "." + ProfileFiles.ProfileExtension;

            return new FileSystem().FindFileInDirectories(directories, filename) 
                ?? DeploymentDirectory.AppendPath(ProfileFiles.ProfilesDirectory, filename);
        }

        public string BottleFileFor(string bottleName)
        {
            var filename = bottleName;
            if (!bottleName.EndsWith(BottleFiles.Extension))
            {
                filename = bottleName + "." + BottleFiles.Extension;
            }

            return
                _allFolders.Select(x => x.AppendPath(ProfileFiles.BottlesDirectory)).FindFileInDirectories(filename)
                ??
                _allFolders.Select(x => x.AppendPath(ProfileFiles.DeployersDirectory)).FindFileInDirectories(filename)
                ??
                _allFolders.First().AppendPath(ProfileFiles.BottlesDirectory, filename);
        }

        public IEnumerable<string> DeployerBottleFiles()
        {
            return _allFolders.SelectMany(x =>
            {
                var deployerFolder = x.AppendPath(ProfileFiles.DeployersDirectory);
                return new FileSystem().FindFiles(deployerFolder, new FileSet(){
                    Include = "*.zip"
                });
            });
        }

        public virtual IEnumerable<string> DeployerBottleNames()
        {
            return DeployerBottleFiles().Select(Path.GetFileNameWithoutExtension).Distinct().ToList();
        }

        public virtual IEnumerable<string> AllBottleNames()
        {
            return Plan.BottleNames();
        }
    }

    public static class StringExtensions
    {
        static readonly char[] newline = System.Environment.NewLine.ToCharArray();

        public static string[] SplitOnNewLine(this string input)
        {    
            return input.Split(newline);
        }
        public static string FindFileInDirectories(this IEnumerable<string> directories, string filename)
        {
            return new FileSystem().FindFileInDirectories(directories, filename);
        }

        public static string FindDirectoryInDirectories(this IFileSystem fileSystem, IEnumerable<string> directories, string directory)
        {
            return directories
                .Select(dir => dir.AppendPath(directory))
                .FirstOrDefault(fileSystem.DirectoryExists);
        }
    }

    
}