using NHibernate.SqlTypes;

namespace roundhouse.databases.oracle.orm
{
    using System;
    using FluentNHibernate.Mapping;
    using roundhouse.infrastructure;
    using roundhouse.model;

    [CLSCompliant(false)]
    public class ScriptsRunMapping : ClassMap<ScriptsRun>
    {
        public ScriptsRunMapping()
        {
            //HibernateMapping.Schema(ApplicationParameters.CurrentMappings.roundhouse_schema_name);
            Table(ApplicationParameters.CurrentMappings.roundhouse_schema_name + "_" + ApplicationParameters.CurrentMappings.scripts_run_table_name);
            Not.LazyLoad();
            HibernateMapping.DefaultAccess.Property();
            HibernateMapping.DefaultCascade.SaveUpdate();

            Id(x => x.id).Column("id").GeneratedBy.Sequence(ApplicationParameters.CurrentMappings.roundhouse_schema_name + "_" + ApplicationParameters.CurrentMappings.scripts_run_table_name + "id").UnsavedValue(0);
            Map(x => x.version_id);
            Map(x => x.script_name);
            Map(x => x.text_of_script).CustomSqlType("Clob").CustomType("StringClob");
            Map(x => x.text_hash).Length(512);
            Map(x => x.one_time_script);

            //audit
            Map(x => x.entry_date);
            Map(x => x.modified_date);
            Map(x => x.entered_by).Length(50);
        }
    }
}