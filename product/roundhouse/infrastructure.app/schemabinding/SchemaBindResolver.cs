using roundhouse.databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace roundhouse.infrastructure.app.schemabinding
{
    public class SchemaBindResolver
    {
        public Database database { get; private set; }

        public SchemaBindResolver(Database database)
        {
            this.database = database;
        }
        
        public string resolve(string sql_text)
        {
            var result = new StringBuilder();
            Dictionary<string, string> definitions_to_restore = new Dictionary<string, string>();

            var matchPattern = @"(?:alter|drop)\s+(?:view)\s+(?:\[?dbo\]?\.)?\[?(\w*\b)\]?";
            foreach (Match match in Regex.Matches(sql_text, matchPattern, RegexOptions.IgnoreCase))
            {
                var objectName = match.Groups[1].Value;
                var dependent_objects = database.get_dependent_schemabound_views(objectName);
                if (dependent_objects.Count > 0)
                {
                    
                    foreach (var dependentObject in dependent_objects)
                    {
                        if (!definitions_to_restore.ContainsKey(dependentObject))
                        {
                            var definition = database.get_object_definition(dependentObject);

                            definition = Regex.Replace(definition, "create view", "alter view", RegexOptions.IgnoreCase);
                            definitions_to_restore.Add(dependentObject, definition);
                            definition = Regex.Replace(definition, "with schemabinding", "", RegexOptions.IgnoreCase);

                            result.AppendLine("GO");
                            result.AppendLine(string.Format("PRINT 'Removing schema binding from {0}'", dependentObject));
                            result.AppendLine("GO");

                            result.AppendLine(resolve(definition));
                            result.AppendLine("GO");
                        }
                    }
                }
            }
            result.AppendLine(sql_text);
            foreach (var def in definitions_to_restore)
            {
                result.AppendLine("GO");
                result.AppendLine(string.Format("PRINT 'Restoring schema binding to {0}'", def.Key));
                result.AppendLine("GO");
                result.AppendLine(def.Value);
                result.AppendLine("GO");
            }
            return result.ToString();
        }
    }
}
