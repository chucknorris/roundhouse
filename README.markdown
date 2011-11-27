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
## 0.8.5.362  
* FIX: KeyNotFoundException in NHibernateSessionFactoryBuilder. See [issue 59] (http://code.google.com/p/roundhouse/issues/detail?id=59) for details. (r361)  
* **SQLite Support!**. See details https://github.com/chucknorris/roundhouse/issues/21 (r360)  
* **PostgreSQL Support!** Thanks SiimV! See details https://github.com/chucknorris/roundhouse/issues/30 (r359)  
* **New Configuration Switch!** SearchAllSubdirectoriesInsteadOfTraverse - All migrations subfolders are traversed by default and run in order of each folder's scripts. This option runs all items in subfolders at same time. Thanks SiimV! See details https://github.com/chucknorris/roundhouse/issues/31 (r359)  
* FIX: Transactions not working with restore. See details https://github.com/chucknorris/roundhouse/issues/26 (r357)  
* FIX: Fixed a nasty bug with SQL Server where it tries to hold a connection (interferes with drop/create mode) and gives a transport error. See details https://github.com/chucknorris/roundhouse/issues/12 (r357)  
* **New Version Resolver!** - Script Number Versioning. See details https://github.com/chucknorris/roundhouse/pull/25 (r356)  
* FIX: Custom create script should split batch statements. See details https://github.com/chucknorris/roundhouse/issues/22 (r353)  
* **New Migrations Folder!** RunAfterCreateDatabaseFolder - Runs only one time and only after a database has been created. This works with a limited set of database types at the moment. Please test if you are planning on using. See details https://github.com/chucknorris/roundhouse/issues/20 (r351)  
* Almost everything is now ilmerged internalized. See details https://github.com/chucknorris/roundhouse/issues/8 and https://github.com/chucknorris/roundhouse/issues/15 (r350)  
* FIX: Cannot drop databases with snapshots. See details https://github.com/chucknorris/roundhouse/pull/13 (r349)  
* Create database custom script can handle file paths. See details https://github.com/chucknorris/roundhouse/pull/17 (r348)  
* FIX: SQL Server 2000 needs to create all of its tables. See details https://github.com/chucknorris/roundhouse/issues/18 (r346)  
* RH assemblies are now signed. See details https://github.com/chucknorris/roundhouse/issues/14 (r342)  
* FIX: Removed the temporary log location. See details https://github.com/chucknorris/roundhouse/issues/7 (r340)  
* **New Configuration Switch!** DisableTokenReplacement - Token replacement should be configurable. See [issue 56](http://code.google.com/p/roundhouse/issues/detail?id=56) for details. (r339)  
* FIX: Token replacer should only replace for items it finds. See [issue 56](http://code.google.com/p/roundhouse/issues/detail?id=56) for details. (r339)  
* **Possible Breaking Change!** File encoding will always try to read files as UTF-8, but fall back to ANSI. You can't go wrong if you encode in ANSI. See [issue 39](http://code.google.com/p/roundhouse/issues/detail?id=39) for details. (r337)  
* Restores are a bit smarter about moving files to a default location when one has not been specified. See details https://github.com/chucknorris/roundhouse/issues/9 or [issue 13](http://code.google.com/p/roundhouse/issues/detail?id=13) (r336)  
* FIX: Do not run token replacement on empty text. See details https://github.com/chucknorris/roundhouse/issues/10 (r330)  
* Custom Scripts also run token replacement (r321)  
* **New Configuration Switches!** Two new switches available - CommandTimeout and CommandTimeoutAdmin! (r329)  
* FIX: Migrate doesn't try to configure log4net now (causes issues w/libraries that do) (r326)  
* **New Migrations Folder!** Indexes folder now available (r327)  
* **New Migrations Folder!** AlterDatabase folder now available. See details https://github.com/chucknorris/roundhouse/issues/6 (r324)  
* FIX: Included sample for Oracle doesn't work. See [issue 55] (http://code.google.com/p/roundhouse/issues/detail?id=55) for details. (r322)  
* Custom Restore Options should use token replacement (r321)  
* **MySQL Support!**. Thanks Diyan. See details https://github.com/chucknorris/roundhouse/pull/3 (r320)  
  
## 0.8.0.300  
* RH now does token replacement in the sql files using '{{PropertyName}}'. See [issue 33] (http://code.google.com/p/roundhouse/issues/detail?id=33) for details. (r299)  
* Always run files that have '.EVERYTIME.' in the name. See [issue 51] (http://code.google.com/p/roundhouse/issues/detail?id=51) for details. (r299)  
* RoundhousE ships a DLL for embedding. See [issue 44] (http://code.google.com/p/roundhouse/issues/detail?id=44) for details. It has a semi-fluent interface - see (https://gist.github.com/977990) for details. (r299)  
* FIX: Environment Specific Files run other environments when other environments are part of the name (i.e. BOBTEST is run with TEST). See [issue 50] (http://code.google.com/p/roundhouse/issues/detail?id=50) for details. (r299)  
* A folder that runs after the other anytime scripts folders have run has been added. See https://github.com/chucknorris/roundhouse/pull/1 for details. (r297)  
* Fixing the script modified twice running each time bug. See https://github.com/chucknorris/roundhouse/pull/5 for details. (r296)  
* Sample is now a project in the release folder. (r287)  
* MSBuild is available again. (r288)  
  
## 0.7.0.281  
* Fixed a few issues with using the connection string. You should now be able to only supply the connection string and not server/database as well.  
  
## 0.7.0.276  
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
  
## Prior Release Notes  
**Prior releases notes are on the [wiki](https://github.com/chucknorris/roundhouse/wiki/releasenotes).**  

  
# CREDITS
UppercuT - Automated Builds (automated build in 10 minutes or less?!) [http://projectuppercut.org](http://projectuppercut.org)