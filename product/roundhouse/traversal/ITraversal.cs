using System;
using roundhouse.folders;

namespace roundhouse.traversal
{
    /// <summary>
    /// Abstraction of traversing a set of scripts.
    /// </summary>
    public interface ITraversal
    {
        void traverse(Action<TraversalConfiguration> configure_traversal);
    }

    public interface IScriptInfo
    {
        MigrationsFolder folder { get; }
        string script_name { get; }
        string script_contents { get; }
    }

    public class ScriptFileInfo : IScriptInfo
    {
        public MigrationsFolder folder { get; set; }
        public string script_name { get; set; }
        public string script_contents { get; set; }
    }

}