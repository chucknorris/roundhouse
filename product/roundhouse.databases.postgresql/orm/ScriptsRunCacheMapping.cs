using FluentNHibernate.Mapping;
using roundhouse.infrastructure;
using roundhouse.model;

namespace roundhouse.databases.postgresql.orm
{
    public class ScriptsRunCacheMapping : ClassMap<ScriptsRunCache>
    {
        public ScriptsRunCacheMapping()
        {
            Table(ApplicationParameters.CurrentMappings.roundhouse_schema_name + "." + ApplicationParameters.CurrentMappings.scripts_run_table_name);
            Not.LazyLoad();
            HibernateMapping.DefaultAccess.Property();
            HibernateMapping.DefaultCascade.SaveUpdate();

            Id(x => x.id).Column("id").GeneratedBy.Increment().UnsavedValue(0);
            Map(x => x.script_name);
            Map(x => x.text_hash).Length(512);

            //audit
            Map(x => x.entry_date);
            Map(x => x.modified_date);
            Map(x => x.entered_by).Length(50);
        }
    }
}