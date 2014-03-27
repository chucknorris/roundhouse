DECLARE @Name VarChar(100)
DECLARE @Type VarChar(20)
SET @Name = 'vw_UsedBySchemaBound'
SET @Type = 'VIEW'
IF NOT EXISTS(SELECT * FROM dbo.sysobjects WHERE [name] = @Name)
  BEGIN
	DECLARE @SQL varchar(1000)
	SET @SQL = 'CREATE ' + @Type + ' ' + @Name + ' AS SELECT * FROM sysobjects'
	EXECUTE(@SQL)
  END
Print 'Updating ' + @Type + ' ' + @Name
GO

ALTER VIEW dbo.vw_UsedBySchemaBound WITH SCHEMABINDING AS

SELECT ID, Dude FROM dbo.Timmy