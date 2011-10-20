using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.IO;
using Bottles.Commands;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{
    public class VdirInput
    {
        private string _folder;

        [Description("Physical file path")]
        public string Folder
        {
            get { return _folder; }
            set { _folder = Path.GetFullPath(value.TrimEnd('/').TrimEnd('\\')); }
        }

        [Description("The name of the virtual directory in IIS")]
        public string VirtualDirectory { get; set; }
    }


    public class VdirAppendInput : VdirInput
    {
        [Description("The subfolder pathing underneath the virtual directory, i.e., '/content/images'")]
        public string VirtualDirectoryFolder { get; set; }
    }

    [CommandDescription("Creates virtual directories in IIS")]
    public class CreateVdirCommand : FubuCommand<VdirInput>
    {
        public override bool Execute(VdirInput input)
        {
            VDirTool.CreateVDirApp(input);
            return new AliasCommand().Execute(new AliasInput{
                Folder = input.Folder,
                Name = input.VirtualDirectory
            });
        }
    }

    [CommandDescription("Appends to virtual directories in IIS")]
    public class AppendVdirCommand : FubuCommand<VdirAppendInput>
    {
        public override bool Execute(VdirAppendInput input)
        {
            VDirTool.AppendVDir(input);
            return true;
        }
    }

    public class VDirTool
    {
        // Authorization flags
        private const int MD_ACCESS_EXECUTE = 0x00000004; //Allow file execution (includes script permission).
        private const int MD_ACCESS_NO_REMOTE_EXECUTE = 0x00002000; // Local execution only.
        private const int MD_ACCESS_NO_REMOTE_READ = 0x00001000; // Local read access only.
        private const int MD_ACCESS_NO_REMOTE_SCRIPT = 0x00004000; // Local host access only.
        private const int MD_ACCESS_NO_REMOTE_WRITE = 0x00000400; // Local write access only.
        private const int MD_ACCESS_READ = 0x00000001; //Allow read access.
        private const int MD_ACCESS_SCRIPT = 0x00000200; // Allow script execution.
        private const int MD_ACCESS_SOURCE = 0x00000010; //Allow source access.
        private const int MD_ACCESS_WRITE = 0x00000002; //Allow write access.
        private const int MD_AUTH_ANONYMOUS = 0x00000001; //Anonymous authentication available.
        private const int MD_AUTH_BASIC = 0x00000002; //Basic authentication available.
        private const int MD_AUTH_NT = 0x00000004; //Windows authentication schemes available.
        private const uint MD_DIRBROW_ENABLED = 0x80000000;
        private const int MD_DIRBROW_LOADDEFAULT = 0x40000000; // Load default page, if it exists.
        private const int MD_DIRBROW_LONG_DATE = 0x00000020; //Show full date.

        // Browse flags
        private const int MD_DIRBROW_SHOW_DATE = 0x00000002; //Show date.
        private const int MD_DIRBROW_SHOW_EXTENSION = 0x00000010; //Show file name extension.
        private const int MD_DIRBROW_SHOW_SIZE = 0x00000008; //Show file size.
        private const int MD_DIRBROW_SHOW_TIME = 0x00000004; // Show time.


        public static void CreateVDirApp(VdirInput input)
        {
            var version = getIISVersionNumber();

            var rootWeb = "IIS://localhost/W3SVC/1/Root";

            if (checkIfExists(rootWeb, input.VirtualDirectory))
            {
                delete(rootWeb, input.VirtualDirectory);
            }

            var folderRoot = new DirectoryEntry(rootWeb);
            folderRoot.RefreshCache();

            var vDir = folderRoot.Children.Add(input.VirtualDirectory, "IIsWebVirtualDir");
            vDir.CommitChanges();

            // Set Properties
            vDir.Properties["Path"].Value = input.Folder;
            vDir.Properties["AuthFlags"].Value = MD_AUTH_ANONYMOUS | MD_AUTH_NT;
            vDir.Properties["DefaultDoc"].Value = "default.aspx, default.htm";
            vDir.Properties["DirBrowseFlags"].Value = MD_DIRBROW_SHOW_DATE |
                                                      MD_DIRBROW_ENABLED |
                                                      MD_DIRBROW_SHOW_SIZE | MD_DIRBROW_SHOW_EXTENSION |
                                                      MD_DIRBROW_LONG_DATE | MD_DIRBROW_LOADDEFAULT;
            vDir.Properties["AccessFlags"].Value = MD_ACCESS_READ |
                                                   MD_ACCESS_SCRIPT;


            if (version[0] < 7)
            {
                vDir.Properties["ScriptMaps"].Add(
                    @"*,%SystemRoot%\Microsoft.Net\Framework\v2.0.50727\aspnet_isapi.dll,0,All");
            }

            var applicationType = new object[]{0};
            vDir.Invoke("AppCreate2", applicationType);

            vDir.CommitChanges();
            folderRoot.CommitChanges();
            vDir.Close();
            folderRoot.Close();
        }

        public static void AppendVDir(VdirAppendInput input)
        {
            var parentRoot = "IIS://localhost/W3SVC/1/Root/" + input.VirtualDirectory;

            if (checkIfExists(parentRoot, input.VirtualDirectoryFolder))
            {
                delete(parentRoot, input.VirtualDirectoryFolder);
            }

            var folderRoot = new DirectoryEntry(parentRoot);
            folderRoot.RefreshCache();

            var vDir = folderRoot.Children.Add(input.VirtualDirectoryFolder, "IIsWebVirtualDir");
            vDir.CommitChanges();

            // Set Properties
            vDir.Properties["Path"].Value = input.Folder;
            vDir.Properties["AccessFlags"].Value = MD_ACCESS_READ | MD_ACCESS_SCRIPT;

            vDir.CommitChanges();
            folderRoot.CommitChanges();
            vDir.Close();
            folderRoot.Close();
        }

        private static int[] getIISVersionNumber()
        {
            using (var entry = new DirectoryEntry("IIS://localhost/W3SVC/Info"))
            {
                var major = (int) entry.Properties["MajorIIsVersionNumber"].Value;
                var minor = (int) entry.Properties["MinorIIsVersionNumber"].Value;

                return new[]{major, minor};
            }
        }


        internal static bool checkIfExists(string rootWeb, string virtualDirectory)
        {
            try
            {
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static void delete(string rootWeb, string virtualDirectory)
        {
            var root = new DirectoryEntry(rootWeb);
            var vdir = root.Children.Find(virtualDirectory, root.SchemaClassName);

            // Remove Entry from container.
            var strName = vdir.Name;
            root.Children.Remove(vdir);
            ConsoleWriter.Write(strName + " entry was removed from container.");
            root.CommitChanges();
            root.Close();
        }
    }
}