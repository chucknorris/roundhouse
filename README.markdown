Project RoundhousE - Database Change Management done right
=======
<img src="http://github.com/ferventcoder/roundhouse/raw/master/docs/logo/RoundhousE_Logo.jpg" height="200" alt="RoundhousE - Professional Database Management" />  
  
# LICENSE
Apache 2.0 - see docs\legal (just LEGAL in the zip folder)  
  
# Documentation
[WIKI](https://github.com/chucknorris/roundhouse/wiki)  
  
# IMPORTANT
NOTE: If you are looking at the source - please run build.bat before opening the solution. It creates the SolutionVersion.cs file that is necessary for a successful build.

# INFO
## Overview
RoundhousE is an automated database deployment (change management) system that allows you to use your current idioms and gain much more. Currently it only supports Microsoft SQL Server, but there are future plans for other databases.  
  
It seeks to solve both maintenance concerns and ease of deployment. We follow some of the same idioms as other database management systems (SQL scripts), but we are different in that we think about future maintenance concerns. We want to always apply certain scripts (anything stateless like functions, views, stored procedures, and permissions), so we don't have to throw everything into our change scripts. This seeks to solves future source control concerns. How sweet is it when you can version the database according to your current source control version?  
  
## Getting started with RoundhousE
### Downloads
 You can download RoundhousE from [http://code.google.com/p/roundhouse/downloads/list](http://code.google.com/p/roundhouse/downloads/list)  

 You can also obtain a copy from the build server at [http://teamcity.codebetter.com](http://teamcity.codebetter.com).  
  
### Gems  
If you have Ruby 1.8.6+ (and Gems 1.3.7+) installed, you can get the current release of RoundhousE to your machine quickly!  
  
1. Type `gem install roundhouse`  
2. Then from anywhere you can type `rh [options]`  
  
### NuGet  
With NuGet you can get the current release of RoundhousE to your application quickly!  
  
1. In Visual Studio Package Manager Console type `install-package roundhouse`  
2. There is also `roundhouse.lib`, `roundhouse.msbuild`, and `roundhouse.refreshdatabase`  
  
### Chocolatey  
Chocolatey like apt-get, but for Windows! This is an alternative method to get the current release of RoundhousE to your machine quickly!  
  
1. Type `cinst roundhouse`  
2. Then from anywhere you can type `rh [options]`  
  
### Source
This is the best way to get to the bleeding edge of what we are doing.  

1. Clone the source down to your machine.  
  `git clone git://github.com/chucknorris/roundhouse.git`  
2. Type `cd roundhouse`  
3. Type `git config core.autocrlf false` to leave line endings as they are.  
4. Type `git status`. You should not see any files to change.
5. Run `build.bat`. NOTE: You must have git on the path (open a regular command line and type git).
  
  
# REQUIREMENTS
* .NET Framework 3.5  
* SA access to the sql server (for creation or deletion)  
* change access to the database (for everything else)  

# DONATE
Donations Accepted - If you enjoy using this product or it has saved you time and money in some way, please consider making a donation.  
It helps keep to the product updated, pays for site hosting, etc. https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=9831498

# RELEASE NOTES
=0.8.0.x=  
* FIX: Fixed a nasty bug with SQL Server where it tries to hold a connection (interferes with drop/create mode) and gives a transport error (r331)  
* Custom Scripts also run token replacement (r321, r330)  
* Two new switches available, CommandTimeout and CommandTimeoutAdmin! (r329)  
* FIX: Migrate doesn't try to configure log4net now (causes issues w/libraries that do) (r326)  
* Two new folders available: Indexes and AlterDatabase (r324)  
* FIX: Included sample for Oracle doesn't work. See [issue 55] (http://code.google.com/p/roundhouse/issues/detail?id=55) for details. (r322)  
* RH now works with MySQL. Thanks Diyan. See details https://github.com/chucknorris/roundhouse/pull/3 (r317)  
  
=0.8.0.300=  
* RH now does token replacement in the sql files using '{{PropertyName}}'. See [issue 33] (http://code.google.com/p/roundhouse/issues/detail?id=33) for details. (r299)  
* Always run files that have '.EVERYTIME.' in the name. See [issue 51] (http://code.google.com/p/roundhouse/issues/detail?id=51) for details. (r299)  
* RoundhousE ships a DLL for embedding. See [issue 44] (http://code.google.com/p/roundhouse/issues/detail?id=44) for details. It has a semi-fluent interface - see (https://gist.github.com/977990) for details. (r299)  
* FIX: Environment Specific Files run other environments when other environments are part of the name (i.e. BOBTEST is run with TEST). See [issue 50] (http://code.google.com/p/roundhouse/issues/detail?id=50) for details. (r299)  
* A folder that runs after the other anytime scripts folders have run has been added. See https://github.com/chucknorris/roundhouse/pull/1 for details. (r297)  
* Fixing the script modified twice running each time bug. See https://github.com/chucknorris/roundhouse/pull/5 for details. (r296)  
* Sample is now a project in the release folder. (r287)  
* MSBuild is available again. (r288)  
  
=0.7.0.281=  
* Fixed a few issues with using the connection string. You should now be able to only supply the connection string and not server/database as well.  
  
=0.7.0.276=  
* Fixed a collation issue with RoundhousE id columns in its tracking tables. See [issue 46] (http://code.google.com/p/roundhouse/issues/detail?id=46) for details. (r274)  
* RestoreFromPath can take a relative path. (r269)  
* RH can now upgrade it's internals without user interaction. See [issue 40] (http://code.google.com/p/roundhouse/issues/detail?id=40) for details. (r268)  
* MSBuild / NAnt tasks are deprecated and no longer hooked up. Please use the console and call it from your tasks. (r268)  
* RH has differencing support with NHibernate Schema Generation/Updates (r267 - branch, r268)  
* FluentNhibernate and NHibernate are now being used for the internals (r267 - branch, r268)  
* SMO is deprecated and removed (r203 - branch, r268)  
* Gems and build upgrades, oh my! (r259)  
* SQL2000 to 2005 is now a smooth transition. (r221)  
* Fix: SQL2000 - ScriptsRun now correctly references Version for the foreign key. (r220)  
* Fix: Connection should be initialized before asking the database if it supports ddl transactions. (r215)  
* Fix: Uppercase User names when running with Oracle. (r200)  
* RH has differencing support with RedGate. See sample project for details. (r197)  
* Fix: Scrips run errors now updates version number and path w/out a dependency on scripts run. Allows for it to finish during transactional runs and still capture errors. (r196)  
* Fix: Capture errortastic changes to DDL/DML (up) files in the script run errors table. (r191)  
* Added admin connection string to do administrative tasks. (r190)  
  
=0.5.0.188=  
* RH handles nonsupported DDL transactions. RH will also stop and inform the user when DDL transactions are not supported, so user can choose to continue - see [issue 28](http://code.google.com/p/roundhouse/issues/detail?id=28) for details. (r172 - branch, r185)  
* RH logs when errors occur in the RoundhousE.ScriptRunErrors table - see [issue 28](http://code.google.com/p/roundhouse/issues/detail?id=28) for details. (r167 - branch, r185)  
* RH supports SQL Server 2000 - see [issue 30](http://code.google.com/p/roundhouse/issues/detail?id=30) for details. (r167 - branch, r185)  
* RH supports Oracle - see [issue 34](http://code.google.com/p/roundhouse/issues/detail?id=34) for details. (r157 - branch, r185)  
  
=0.2.0.175=  
* RH should handle when user names have apostrophes (') in them - see [issue 32](http://code.google.com/p/roundhouse/issues/detail?id=32) for details. (r175)  
* Database names should have delimiters to allow for non standard characters - see [issue 31](http://code.google.com/p/roundhouse/issues/detail?id=31) for details. (r174)  
* Added autowiring permissions sample. (r169)  
* If copying to change output fails, log a warning and move on. (r156)  
  
=0.2.0.150=  
* Turning off batch splitting where it is not necessary as it does affect performance. (r150)  
* Splitting statements on GO has been enhanced to catch end of file GOs - see [issue 25](http://code.google.com/p/roundhouse/issues/detail?id=25) for details. (r149)  
  
=0.2.0.148=  
* Splitting statements on GO has been enhanced - see [issue 25](http://code.google.com/p/roundhouse/issues/detail?id=25) for details. (r147)  
  
=0.2.0.144=  
* Splitting statements on GO is now a two-phased approach - see [issue 25](http://code.google.com/p/roundhouse/issues/detail?id=25) for details. (r143)  
* Trying out StructureMap (r136)  
  
=0.2.0.131=  
* More fine-grained splitting by the word GO - see [issue 25](http://code.google.com/p/roundhouse/issues/detail?id=25) for details. (r130)  
* Command timeout became more explicit so database decorators can access and manipulate the value now. (r129)  
* RH should be able to use ADO.NET instead of SMO for SqlServer - see [issue 22](http://code.google.com/p/roundhouse/issues/detail?id=22) for details. (r125)  
* Option not to create a database if none exists - see [issue 24](http://code.google.com/p/roundhouse/issues/detail?id=24) for details. (r123)  
* Console should exit with an error code instead of crashing on errors - see [issue 23](http://code.google.com/p/roundhouse/issues/detail?id=23) for details. (r119)  
  
=0.2.0.117=  
* Restore timeouts can now be specified (with a default timeout of 900 seconds) - see [issue 21](http://code.google.com/p/roundhouse/issues/detail?id=21) for details. (r117)  
* Custom database create scripts are now possible - see [issue 20](http://code.google.com/p/roundhouse/issues/detail?id=20) for details. (r116)  
* All types of migrations will split sql statements from files that have more than one statement into multiple statements and run each consecutively - see [issue 27](http://code.google.com/p/roundhouse/issues/detail?id=17) for details. (r113)  
* Fixed issue with running roundhouse twice during MSBuild - see [issue 16](http://code.google.com/p/roundhouse/issues/detail?id=16) for details. (r112)  
  
=0.2.0.108=  
* Reports version during run - see [issue 9](http://code.google.com/p/roundhouse/issues/detail?id=9) for details. (r108)  
* Logs what type of script it is looking for and where it is looking for them - see [issue 14](http://code.google.com/p/roundhouse/issues/detail?id=14) for details. (r107)  
* Removed double error reporting on exception. (r107)  
* Adding getting started documentation  
* Added a custom restore option to add additional arguments (like MOVE) to a restore - see [issue 12](http://code.google.com/p/roundhouse/issues/detail?id=12) for details. (r104)  
* Fixed a connection string initialization issue (r103)  
  
=0.2.0.101=  
* File Sorting - Fixed a sorting issue with file names to do explicit sorting. (r101)  
* RH has an icon of the logo (r99)  
  
=0.2.0.97=  
* OleDB Support - RH can now be run for advanced database connections with a connection string. (r96)  
  - OleDB may not support creating/restoring/deleting databases.  
  - Microsoft Access is supported through OleDB.  
  - Other databases will need to have scripts written for their type before they would be supported.  
   
=0.0.0.86=  
* Environment awareness - RH determines an environment file first that it has ".ENV." in the file name and then if it is the proper environment file based on whether the file also contains the specific environment name. For example LOCAL.0001_DoSomething.ENV.sql will run in the LOCAL environment but no where else.  
* Runs permissions files every time regardless of changes.  
  
=0.0.0.85=  
* Added synonyms for database types - see [issue 10](http://code.google.com/p/roundhouse/issues/detail?id=10) for details. (r85)  
* Removed some of the logging - no more event logging for limited permissions running (r83)  
  
=0.0.0.82=  
* The command line version (rh.exe) is merged into a single assembly with all dependencies (except Microsoft.SQL dlls) internalized. No configuration file necessary either. (r77, r82)  
* Log from console runner is now copied into the change_drop folder. (r81)  
* Recovery mode is now explicit. Default is full. Otherwise pass in /simple - see [issue 8](http://code.google.com/p/roundhouse/issues/detail?id=8) for details. (r78)  
* Excluded the drop database from transactions (not that you would call /drop with /t). (r76)  
  
=0.0.0.74=  
* Fixed an issue with holding connections - see [issue 7](http://code.google.com/p/roundhouse/issues/detail?id=7) for details. (r74)  
* Added the ability to run with transactions  
  
# CREDITS
UppercuT - Automated Builds (automated build in 10 minutes or less?!) [http://projectuppercut.org](http://projectuppercut.org)