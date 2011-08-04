using System;
using roundhouse.folders;
using roundhouse.infrastructure.app;
using roundhouse.infrastructure.app.tokens;
using roundhouse.infrastructure.logging;
using roundhouse.migrators;
using roundhouse.traversal;
using Environment = roundhouse.environments.Environment;

namespace roundhouse.runners
{
    public class ScriptRunner
    {
        private readonly ConfigurationPropertyHolder configuration;
        private readonly DatabaseMigrator database_migrator;
        private readonly long version_id;
        private readonly string repository_path;
        private readonly Action<string, MigrationsFolder> copy_to_change_drop_folder;
        private readonly Environment environment;
        private readonly string new_version;

        public ScriptRunner(
            ConfigurationPropertyHolder configuration,
            DatabaseMigrator database_migrator,
            long version_id,
            Environment environment,
            string new_version,
            string repository_path,
            Action<string , MigrationsFolder> copy_to_change_drop_folder = null
            )
        {
            this.configuration = configuration;
            this.database_migrator = database_migrator;
            this.version_id = version_id;
            this.environment = environment;
            this.new_version = new_version;
            this.repository_path = repository_path;
            this.copy_to_change_drop_folder = copy_to_change_drop_folder ?? new Action<string, MigrationsFolder>((s, f) => { });
        }

        public void execute_script(IScriptInfo script, ConnectionType connection_type)
        {
            string script_file_text = replace_tokens(script.script_contents);
            bool the_sql_ran = database_migrator.run_sql(script_file_text,
                                                         script.script_name,
                                                         script.folder.
                                                             should_run_items_in_folder_once,
                                                         script.folder.
                                                             should_run_items_in_folder_every_time,


                                                         version_id,
                                                         environment,
                                                         new_version,
                                                         repository_path,
                                                         connection_type);
            if (the_sql_ran)
            {
                try
                {
                    copy_to_change_drop_folder(script.script_name,
                                               script.folder);
                }
                catch (Exception ex)
                {
                    Log.bound_to(this).log_a_warning_event_containing(
                        "Unable to copy {0} to {1}. {2}{3}",
                        script.script_name, script.folder.folder_full_path,
                        System.Environment.NewLine, ex.ToString());
                }
            }
        }

        private string replace_tokens(string sql_text)
        {
            return TokenReplacer.replace_tokens(configuration, sql_text);
        }
    }
}