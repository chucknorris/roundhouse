@echo off

SET DIR=%~d0%~p0%

SET database.name="${database.name}"
SET sql.files.directory="%DIR%${dirs.db}\SQLServer\${database.name}"
SET server.database="${server.database}"
SET repository.path="${repository.path}"
SET version.file="${file.version}"
SET version.xpath="//buildInfo/version"
SET environment=${environment}

"%DIR%Console\rh.exe" /d=%database.name% /f=%sql.files.directory% /s=%server.database% /vf=%version.file% /vx=%version.xpath% /r=%repository.path% /env=%environment% /drop
"%DIR%Console\rh.exe" /d=%database.name% /f=%sql.files.directory% /s=%server.database% /cds=%sql.files.directory%\custom_db_create.sql /vf=%version.file% /vx=%version.xpath% /r=%repository.path% /env=%environment% /simple