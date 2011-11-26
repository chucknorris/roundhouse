namespace roundhouse.databases.postgresql.orm
{
    using FluentNHibernate.Mapping;
    using infrastructure;
    using model;

    public class VersionMapping : ClassMap<Version>
    {
        public VersionMapping()
        {
            //HibernateMapping.Schema(ApplicationParameters.CurrentMappings.roundhouse_schema_name);
            //Table(ApplicationParameters.CurrentMappings.version_table_name);
            Table(ApplicationParameters.CurrentMappings.roundhouse_schema_name + "_" + ApplicationParameters.CurrentMappings.version_table_name);
            Not.LazyLoad();
            HibernateMapping.DefaultAccess.Property();
            HibernateMapping.DefaultCascade.SaveUpdate();

            Id(x => x.id).Column("id").GeneratedBy.Increment().UnsavedValue(0);
            Map(x => x.repository_path);
            Map(x => x.version).Length(50);

            //audit
            Map(x => x.entry_date);
            Map(x => x.modified_date);
            Map(x => x.entered_by).Length(50);
        }
    }
}