using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using roundhouse.folders;
using Moq;

namespace roundhouse.tests
{
    class MockKnownFolders: KnownFolders
    {

       private Mock<MigrationsFolder> migrationsfolder_mock = new Mock<MigrationsFolder>();

        public MigrationsFolder alter_database
        {
            get { return mock_object(); }
        }

        public MigrationsFolder run_after_create_database 
        {
            get { return mock_object(); }
        }
        public MigrationsFolder run_before_up 
        {
            get { return mock_object(); }
        }
        public MigrationsFolder up 
        {
            get { return mock_object(); }
        }
        public MigrationsFolder down 
        {
            get { return mock_object(); }
        }
        public MigrationsFolder run_first_after_up 
        {
            get { return mock_object(); }
        }
        public MigrationsFolder functions 
        {
            get { return mock_object(); }
        }
        public MigrationsFolder views 
        {
            get { return mock_object(); }
        }
        public MigrationsFolder sprocs 
        {
            get { return mock_object(); }
        }
        public MigrationsFolder triggers 
        {
            get { return mock_object(); }
        }
        public MigrationsFolder indexes 
        {
            get { return mock_object(); }
        }
        public MigrationsFolder run_after_other_any_time_scripts 
        {
            get { return mock_object(); }
        }
        public MigrationsFolder permissions 
        {
            get { return mock_object(); }
        }
        public MigrationsFolder before_migration 
        {
            get { return mock_object(); }
        }
        public MigrationsFolder after_migration 
        {
            get { return mock_object(); }
        }
        public Folder change_drop 
        {
            get { return mock_object(); }
        }

        private MigrationsFolder mock_object()
        {
            return migrationsfolder_mock.Object;
        }

    }
}
