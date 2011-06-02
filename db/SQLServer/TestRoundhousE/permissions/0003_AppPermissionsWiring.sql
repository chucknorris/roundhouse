SELECT sc.name as SchemaName, ao.name AS  ItemName, ao.type_desc AS TypeDescription
INTO #TempPermissions
FROM sys.all_objects ao
LEFT JOIN sys.schemas sc
  ON ao.schema_id = sc.schema_id
WHERE is_ms_shipped = 0 
  AND type_desc IN ('USER_TABLE','VIEW','SQL_STORED_PROCEDURE') 

DECLARE @schema VarChar(255), @name VarChar(255), @type VarChar(255)
DECLARE @AppRole VarChar(50), @AppReadOnlyRole VarChar(50), @AppViewsOnlyRole VarChar(50)
SET @AppRole = 'App-ApplicationRole'
SET @AppReadOnlyRole = 'App-ReadOnlyRole'
DECLARE @sql VarChar(500)

/*
	Tables
*/

DECLARE App_TablePermissions CURSOR FAST_FORWARD FOR
  SELECT SchemaName, ItemName, TypeDescription
  FROM #TempPermissions
  WHERE TypeDescription ='USER_TABLE' ;
OPEN App_TablePermissions;


FETCH NEXT FROM App_TablePermissions into @schema, @name, @type;
WHILE @@FETCH_STATUS = 0
   BEGIN
    SET @sql = 'GRANT SELECT ON [' + @schema + '].[' + @name + '] TO [' + @AppRole + ']'
    PRINT @sql
    EXEC(@sql)
    SET @sql = 'GRANT INSERT ON [' + @schema + '].[' + @name + '] TO [' + @AppRole + ']'
    PRINT @sql
    EXEC(@sql)
    SET @sql = 'GRANT UPDATE ON [' + @schema + '].[' + @name + '] TO [' + @AppRole + ']'
    PRINT @sql
    EXEC(@sql)
    SET @sql = 'GRANT DELETE ON [' + @schema + '].[' + @name + '] TO [' + @AppRole + ']'
    PRINT @sql
    EXEC(@sql)
    SET @sql = 'GRANT SELECT ON [' + @schema + '].[' + @name + '] TO [' + @AppReadOnlyRole + ']'
    PRINT @sql
    EXEC(@sql)

    FETCH NEXT FROM App_TablePermissions INTO @schema, @name, @type;
   END;
CLOSE App_TablePermissions;
DEALLOCATE App_TablePermissions;

/*
	Views
*/

DECLARE App_ViewPermissions CURSOR FAST_FORWARD FOR
  SELECT SchemaName, ItemName, TypeDescription
  FROM #TempPermissions
  WHERE TypeDescription ='VIEW' ;
OPEN App_ViewPermissions;

FETCH NEXT FROM App_ViewPermissions into @schema, @name, @type;
WHILE @@FETCH_STATUS = 0
   BEGIN
    SET @sql = 'GRANT SELECT ON [' + @schema + '].[' + @name + '] TO [' + @AppRole + ']'
    PRINT @sql
    EXEC(@sql)
    SET @sql = 'GRANT SELECT ON [' + @schema + '].[' + @name + '] TO [' + @AppReadOnlyRole + ']'
    PRINT @sql
    EXEC(@sql)

    FETCH NEXT FROM App_ViewPermissions INTO @schema, @name, @type;
   END;
CLOSE App_ViewPermissions;
DEALLOCATE App_ViewPermissions;

/*
	Stored Procedures
*/

DECLARE App_SprocPermissions CURSOR FAST_FORWARD FOR
  SELECT SchemaName, ItemName, TypeDescription
  FROM #TempPermissions
  WHERE TypeDescription ='SQL_STORED_PROCEDURE' ;
OPEN App_SprocPermissions;

FETCH NEXT FROM App_SprocPermissions into @schema, @name, @type;
WHILE @@FETCH_STATUS = 0
   BEGIN
    SET @sql = 'GRANT EXECUTE ON [' + @schema + '].[' + @name + '] TO [' + @AppRole + ']'
    PRINT @sql
    EXEC(@sql)
    
    FETCH NEXT FROM App_SprocPermissions INTO @schema, @name, @type;
   END;
CLOSE App_SprocPermissions;
DEALLOCATE App_SprocPermissions;


DECLARE AppReadOnly_SprocPermissions CURSOR FAST_FORWARD FOR
  SELECT SchemaName, ItemName, TypeDescription
  FROM #TempPermissions
  WHERE TypeDescription ='SQL_STORED_PROCEDURE' 
  AND ItemName NOT LIKE '%Insert%'
  AND ItemName NOT LIKE '%Update%'
  AND ItemName NOT LIKE '%Delete%';
OPEN AppReadOnly_SprocPermissions;

FETCH NEXT FROM AppReadOnly_SprocPermissions into @schema, @name, @type;
WHILE @@FETCH_STATUS = 0
   BEGIN
    SET @sql = 'GRANT EXECUTE ON [' + @schema + '].[' + @name + '] TO [' + @AppReadOnlyRole + ']'
    PRINT @sql
    EXEC(@sql)
    
    FETCH NEXT FROM AppReadOnly_SprocPermissions INTO @schema, @name, @type;
   END;
CLOSE AppReadOnly_SprocPermissions;
DEALLOCATE AppReadOnly_SprocPermissions;

DROP TABLE #TempPermissions


SELECT sc.name AS [SchemaName],tp.[name] AS [ItemName],tp.[is_table_type] AS [IsTableType]
INTO #UserTypePermissions
FROM [sys].[types] tp 
LEFT JOIN sys.schemas sc
  ON tp.schema_id = sc.schema_id
WHERE tp.is_user_defined = 1
	AND tp.is_assembly_type = 0

DECLARE App_UserTypePermissions CURSOR FAST_FORWARD FOR
  SELECT SchemaName, ItemName
  FROM #UserTypePermissions
OPEN App_UserTypePermissions;

FETCH NEXT FROM App_UserTypePermissions into @schema, @name;
WHILE @@FETCH_STATUS = 0
   BEGIN
    SET @sql = 'GRANT CONTROL ON TYPE::[' + @schema + '].[' + @name + '] TO [' + @AppRole + ']'
    PRINT @sql
    EXEC(@sql)
    SET @sql = 'GRANT CONTROL ON TYPE::[' + @schema + '].[' + @name + '] TO [' + @AppReadOnlyRole + ']'
    PRINT @sql
    EXEC(@sql)
    
    FETCH NEXT FROM App_UserTypePermissions INTO @schema, @name;
   END;
CLOSE App_UserTypePermissions;
DEALLOCATE App_UserTypePermissions;
  
DROP TABLE #UserTypePermissions