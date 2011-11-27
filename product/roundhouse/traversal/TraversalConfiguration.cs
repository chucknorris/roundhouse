using System;
using System.Collections.Generic;
using System.Linq;
using roundhouse.folders;

namespace roundhouse.traversal
{
    public class TraversalConfiguration
    {
        public class ScriptPair
        {
            public Func<IScriptInfo, bool> should_run { get; set; }
            public Action<IScriptInfo> action { get; set; }
        }

        public class FolderPair
        {
            public Func<MigrationsFolder, bool> should_run { get; set; }
            public Action<MigrationsFolder> action { get; set; }
        }

        private readonly List<Func<MigrationsFolder, bool>> folder_predicates = new List<Func<MigrationsFolder, bool>>();
        private readonly List<FolderPair> folder_before_actions = new List<FolderPair>();
		private readonly List<FolderPair> folder_after_actions = new List<FolderPair>();
        private readonly List<ScriptPair> script_actions = new List<ScriptPair>();


        public void include_all_folders()
        {
            folder_predicates.Add(f=>true);
        }
        public void include_folder(Func<MigrationsFolder, bool> should_include)
        {
            folder_predicates.Add(should_include);
        }
		public void include_folder(MigrationsFolder folder)
        {
            folder_predicates.Add(f=> f==folder);
        }
		
		public void include_folders(params MigrationsFolder[] folders)
		{
			folder_predicates.Add( f => folders.Contains(f) );
		}

        public void for_each_script(Action<IScriptInfo> action)
        {
            for_script_if(s=>true, action);
        }
        public void for_script_if(Func<IScriptInfo, bool> should_run, Action<IScriptInfo> action)
        {
            script_actions.Add( new ScriptPair {should_run = should_run, action = action} );
        }
        public void before_each_folder(Action<MigrationsFolder> action)
        {
            before_folder_if(f=>true, action);
        }
        public void before_folder_if(Func<MigrationsFolder, bool> should_run, Action<MigrationsFolder> action)
        {
            folder_before_actions.Add(new FolderPair { should_run = should_run, action = action });
        }
		public void after_each_folder(Action<MigrationsFolder> action)
		{
			after_folder_if(f => true, action);
		}
		public void after_folder_if(Func<MigrationsFolder, bool> should_run, Action<MigrationsFolder> action)
		{
			folder_after_actions.Add(new FolderPair { should_run = should_run, action = action });
		}

        public IEnumerable<Func<MigrationsFolder, bool>> all_folder_predicates {get { return folder_predicates; }}
		public IEnumerable<FolderPair> all_folder_before_actions { get { return folder_before_actions; } }
		public IEnumerable<FolderPair> all_folder_after_actions { get { return folder_after_actions; } }
        public IEnumerable<ScriptPair> all_script_actions {get { return script_actions; }}
    }
}