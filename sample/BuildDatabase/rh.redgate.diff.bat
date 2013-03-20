B@echo off
::"C:\Program Files (x86)\Red Gate\SQL Compare 8\sqlcompare.exe" /database1:"TestRoundhousE" /scripts2:"C:\code\RoundhousE\db\TestRoundhouse" /include:table /exclude:table:\[Version\]^|\[ScriptsRun\]^|\[ScriptsRunErrors\] /options:Default,IgnoreConstraintNames,IgnorePermissions /ignoreparsererrors /f /scriptfile:"C:\Users\robz\Desktop\Diff.sql"

SET DIR=%~d0%~p0%
SET DIR=C:\code\roundhouse\code_drop\sample\deployment\

SET database.name="TestRoundhousE"
SET sql.files.directory="%DIR%..\..\..\db\SQLServer\TestRoundhousE"
SET server.database="(local)"
SET repository.path="https://github.com/chucknorris/roundhouse.git"
SET version.file="_BuildInfo.xml"
SET version.xpath="//buildInfo/version"
SET environment=LOCAL

"%DIR%Console\rh.exe" rh.redgate.diff /d=%database.name% /f=%sql.files.directory% /s=%server.database% /vf=%version.file% /vx=%version.xpath% /r=%repository.path% /env=%environment% /simple

pause
