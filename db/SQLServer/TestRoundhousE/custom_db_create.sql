USE master
IF NOT EXISTS(SELECT * FROM sys.databases WHERE [name] = '{{DatabaseName}}')
BEGIN
	CREATE DATABASE {{DatabaseName}}
	-- signal success
	SELECT 1
END