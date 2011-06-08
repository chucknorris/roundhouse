
    if exists (select * from dbo.sysobjects where id = object_id(N'dbo.SampleItems') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table dbo.SampleItems

    create table dbo.SampleItems (
        Id BIGINT IDENTITY NOT NULL,
       name NVARCHAR(255) null,
       firstname NVARCHAR(255) null,
       lastname NVARCHAR(255) null,
       primary key (Id)
    )
