using NUnit.Framework;
using roundhouse.consoles;
using roundhouse.infrastructure.app;
using roundhouse.infrastructure.app.builders;
using roundhouse.infrastructure.filesystem;

namespace roundhouse.tests.infrastructure.filesystem
{
    [TestFixture()]
    public class KnownFolders
    {
        [Test()]
        public void should_never_contain_colons()
        {
            var config = new DefaultConfiguration
            {
                DatabaseName = "DaDatabase",
                OutputPath = "/tmp/rh",
                ServerName = "tcp:database.domain.domain"
            };
            var fileSystem = new DotNetFileSystemAccess(config);

            var known_folders = KnownFoldersBuilder.build(fileSystem, config);
            
            StringAssert.DoesNotContain(":", known_folders.change_drop.folder_full_path);
        }
        
    }
}