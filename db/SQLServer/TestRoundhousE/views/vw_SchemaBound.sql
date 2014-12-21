DECLARE @Name VarChar(100)
DECLARE @Type VarChar(20)
SET @Name = 'vw_SchemaBound'
SET @Type = 'VIEW'
IF NOT EXISTS(SELECT * FROM dbo.sysobjects WHERE [name] = @Name)
  BEGIN
	DECLARE @SQL varchar(1000)
	SET @SQL = 'CREATE ' + @Type + ' ' + @Name + ' AS SELECT * FROM sysobjects'
	EXECUTE(@SQL)
  END
Print 'Updating ' + @Type + ' ' + @Name
GO

ALTER VIEW dbo.vw_SchemaBound WITH SCHEMABINDING AS
SELECT  ID FROM dbo.vw_UsedBySchemaBound