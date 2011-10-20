using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Commands
{
    public class LinkInput
    {
        [Description("The physical folder (or valid alias) of the main application")]
        [RequiredUsage("list", "create", "remove", "clean")]
        public string AppFolder { get; set; }

        [Description("The physical folder (or valid alias) of a package")]
        [RequiredUsage("create", "remove")]
        public string PackageFolder { get; set; }

        [Description("Remove the package folder link from the application")]
        [RequiredUsage("remove")]
        [ValidUsage("remove")]
        public bool RemoveFlag { get; set; }

        [Description("Remove all links from an application manifest file")]
        [ValidUsage("clean")]
        public bool CleanAllFlag { get; set; }

        public string RelativePathOfPackage()
        {
            var pkg = Path.GetFullPath(PackageFolder);
            var app = Path.GetFullPath(AppFolder);

            return pkg.PathRelativeTo(app);
        }
    }

    [Usage("list", "List the current links for the application")]
    [Usage("create", "Create a new link for the application to the package")]
    [Usage("remove", "Remove any existing link for the application to the package")]
    [Usage("clean", "Remove any and all existing links from the application to any package folder")]
    [CommandDescription("Links a package folder to an application folder in development mode")]
    public class LinkCommand : FubuCommand<LinkInput>
    {
        public override bool Execute(LinkInput input)
        {
            input.AppFolder = AliasCommand.AliasFolder(input.AppFolder);
            input.PackageFolder = AliasCommand.AliasFolder(input.PackageFolder);


            Execute(input, new FileSystem());
            return true;
        }

        public void Execute(LinkInput input, IFileSystem fileSystem)
        {
            var manifest = fileSystem.LoadLinkManifestFrom(input.AppFolder);

            if (input.CleanAllFlag && fileSystem.FileExists(input.AppFolder, LinkManifest.FILE))
            {
                manifest.RemoveAllLinkedFolders();

                persist(input, manifest, fileSystem);

                ConsoleWriter.Write("Removed all package links from the manifest file for " + input.AppFolder);

                listCurrentLinks(input, manifest);

                return;
            }



            if (input.PackageFolder.IsNotEmpty())
            {
                updateManifest(input, fileSystem, manifest);
            }
            else
            {
                listCurrentLinks(input, manifest);
            }

        }

        private void listCurrentLinks(LinkInput input, LinkManifest manifest)
        {
            var appFolder = input.AppFolder;

            ListCurrentLinks(appFolder, manifest);
        }

        public static void ListCurrentLinks(string appFolder, LinkManifest manifest)
        {
            if (manifest.LinkedFolders.Any())
            {
                ConsoleWriter.Write("  Links for " + appFolder);
                manifest.LinkedFolders.Each(x => { ConsoleWriter.Write("    " + x); });
            }
            else
            {
                ConsoleWriter.Write("  No package links for " + appFolder);
            }
        }

        private void updateManifest(LinkInput input, IFileSystem fileSystem, LinkManifest manifest)
        {
            if (input.RemoveFlag)
            {
                remove(input, manifest);
            }
            else
            {
                add(input, manifest);
            }

            persist(input, manifest, fileSystem);
        }

        private void persist(LinkInput input, LinkManifest manifest, IFileSystem fileSystem)
        {
            fileSystem.PersistToFile(manifest, input.AppFolder, LinkManifest.FILE);
        }

        private void remove(LinkInput input, LinkManifest manifest)
        {
            manifest.RemoveLink(input.RelativePathOfPackage());
            ConsoleWriter.Write("Folder {0} was removed from the application at {1}", input.PackageFolder, input.AppFolder);
        }

        private static void add(LinkInput input, LinkManifest manifest)
        {
            var wasAdded = manifest.AddLink(input.RelativePathOfPackage());
            var msg = wasAdded
                          ? "Folder {0} was added to the application at {1}"
                          : "Folder {0} is already included in the application at {1}";

            ConsoleWriter.Write(msg, input.PackageFolder, input.AppFolder);
        }
    }
}