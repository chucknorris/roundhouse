using System;
using System.Collections.Generic;
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
        private readonly List<FolderPair> folder_actions = new List<FolderPair>();
        private readonly List<ScriptPair> script_actions = new List<ScriptPair>();


        public void include_all_folders()
        {
            folder_predicates.Add(f=>true);
        }
        public void include_folder(Func<MigrationsFolder, bool> should_include)
        {
            folder_predicates.Add(should_include);
        }
        public void for_each_script(Action<IScriptInfo> action)
        {
            for_script_if(s=>true, action);
        }
        public void for_script_if(Func<IScriptInfo, bool> should_run, Action<IScriptInfo> action)
        {
            script_actions.Add( new ScriptPair {should_run = should_run, action = action} );
        }
        public void for_each_folder(Action<MigrationsFolder> action)
        {
            for_folder_if(f=>true, action);
        }
        public void for_folder_if(Func<MigrationsFolder, bool> should_run, Action<MigrationsFolder> action)
        {
            folder_actions.Add(new FolderPair { should_run = should_run, action = action });
        }

        public IEnumerable<Func<MigrationsFolder, bool>> all_folder_predicates {get { return folder_predicates; }}
        public IEnumerable<FolderPair> all_folder_actions {get { return folder_actions; }}
        public IEnumerable<ScriptPair> all_script_actions {get { return script_actions; }}
    }
}