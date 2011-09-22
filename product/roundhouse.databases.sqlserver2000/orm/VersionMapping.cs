namespace roundhouse.databases.sqlserver2000.orm
{
    using System;
    using FluentNHibernate.Mapping;
    using roundhouse.infrastructure;

    [CLSCompliant(false)]
    public class VersionMapping : ClassMap<roundhouse.model.Version>
    {
        public VersionMapping()
        {
            //HibernateMapping.Schema(ApplicationParameters.CurrentMappings.roundhouse_schema_name);
            Table(ApplicationParameters.CurrentMappings.version_table_name);
            Not.LazyLoad();
            HibernateMapping.DefaultAccess.Property();
            HibernateMapping.DefaultCascade.SaveUpdate();

            Id(x => x.id).Column("id").GeneratedBy.Identity().UnsavedValue(0);
            Map(x => x.repository_path);
            Map(x => x.version).Length(50);
           
            //audit
            Map(x => x.entry_date);
            Map(x => x.modified_date);
            Map(x => x.entered_by).Length(50);
        }
    }
}