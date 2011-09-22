@echo off

SET DIR=%~d0%~p0%

SET database.name="${database.name}"
SET sql.files.directory="%DIR%${dirs.db}\SQLServer\${database.name}"
SET server.database="${server.database}"
SET repository.path="${repository.path}"
SET version.file="${file.version}"
SET version.xpath="//buildInfo/version"
SET environment=${environment}
SET custom.create.script="USE master;IF NOT EXISTS(SELECT * FROM sys.databases WHERE [name] = '{{DatabaseName}}')BEGIN; CREATE DATABASE {{DatabaseName}}; END;"

"%DIR%Console\rh.exe" /d=%database.name% /f=%sql.files.directory% /s=%server.database% /vf=%version.file% /vx=%version.xpath% /r=%repository.path% /env=%environment% /drop
"%DIR%Console\rh.exe" /d=%database.name% /f=%sql.files.directory% /s=%server.database% /cds=%custom.create.script% /vf=%version.file% /vx=%version.xpath% /r=%repository.path% /env=%environment% /simple