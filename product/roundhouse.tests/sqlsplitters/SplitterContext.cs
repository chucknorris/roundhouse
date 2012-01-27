namespace roundhouse.tests.sqlsplitters
{
    using roundhouse.sqlsplitters;

    public class SplitterContext
    {

        public class FullSplitter
        {
            public static string tsql_statement = @"
BOB1
GO

/* COMMENT */
BOB2
GO

-- GO

BOB3 GO

--`~!@#$%^&*()-_+=,.;:'""[]\/?<> GO

BOB5
   GO

BOB6
GO

/* GO */

BOB7

/* 

GO

*/

BOB8

--
GO

BOB9

-- `~!@#$%^&*()-_+=,.;:'""[]\/?<>
GO

BOB10GO

CREATE TABLE POGO
{}

INSERT INTO POGO (id,desc) VALUES (1,'GO')

BOB11

  -- TODO: To be good, there should be type column

-- dfgjhdfgdjkgk dfgdfg GO
BOB12

UPDATE Timmy SET id = 'something something go'
UPDATE Timmy SET id = 'something something: go'

ALTER TABLE Inv.something ADD
	gagagag decimal(20, 12) NULL,
    asdfasdf DECIMAL(20, 6) NULL,
	didbibi decimal(20, 6) NULL,
	yeppsasd decimal(20, 6) NULL,
	uhuhhh datetime NULL,
	slsald varchar(15) NULL,
	uhasdf varchar(15) NULL,
    daf_asdfasdf DECIMAL(20,6) NULL;
GO

EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Daily job', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=3, 
		@on_success_step_id=0, 
		@on_fail_action=3, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'
dml statements
GO  
dml statements '

GO

INSERT [dbo].[Foo] ([Bar]) VALUES (N'hello--world.
Thanks!')
INSERT [dbo].[Foo] ([Bar]) VALUES (N'Go speed racer, go speed racer, go speed racer go!!!!! ')

GO";

			public static string tsql_statement_scrubbed = @"
BOB1
" + StatementSplitter.batch_terminator_replacement_string + @"

/* COMMENT */
BOB2
" + StatementSplitter.batch_terminator_replacement_string + @"

-- GO

BOB3 " + StatementSplitter.batch_terminator_replacement_string + @"

--`~!@#$%^&*()-_+=,.;:'""[]\/?<> GO

BOB5
   " + StatementSplitter.batch_terminator_replacement_string + @"

BOB6
" + StatementSplitter.batch_terminator_replacement_string + @"

/* GO */

BOB7

/* 

GO

*/

BOB8

--
" + StatementSplitter.batch_terminator_replacement_string + @"

BOB9

-- `~!@#$%^&*()-_+=,.;:'""[]\/?<>
" + StatementSplitter.batch_terminator_replacement_string + @"

BOB10GO

CREATE TABLE POGO
{}

INSERT INTO POGO (id,desc) VALUES (1,'GO')

BOB11

  -- TODO: To be good, there should be type column

-- dfgjhdfgdjkgk dfgdfg GO
BOB12

UPDATE Timmy SET id = 'something something go'
UPDATE Timmy SET id = 'something something: go'

ALTER TABLE Inv.something ADD
	gagagag decimal(20, 12) NULL,
    asdfasdf DECIMAL(20, 6) NULL,
	didbibi decimal(20, 6) NULL,
	yeppsasd decimal(20, 6) NULL,
	uhuhhh datetime NULL,
	slsald varchar(15) NULL,
	uhasdf varchar(15) NULL,
    daf_asdfasdf DECIMAL(20,6) NULL;
" + StatementSplitter.batch_terminator_replacement_string + @"

EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Daily job', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=3, 
		@on_success_step_id=0, 
		@on_fail_action=3, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'
dml statements
GO  
dml statements '

" + StatementSplitter.batch_terminator_replacement_string + @"

INSERT [dbo].[Foo] ([Bar]) VALUES (N'hello--world.
Thanks!')
INSERT [dbo].[Foo] ([Bar]) VALUES (N'Go speed racer, go speed racer, go speed racer go!!!!! ')

"+ StatementSplitter.batch_terminator_replacement_string + @"";

        	public static string plsql_statement =
				@"
SQL1;
;
SQL2;
;
tmpSql := 'DROP SEQUENCE mutatieStockID';
EXECUTE IMMEDIATE tmpSql; 
;
BEGIN
INSERT into Table (columnname) values ("";"");
UPDATE Table set columnname="";"";
END;
";
			public static string plsql_statement_scrubbed = @"
SQL1;
" + StatementSplitter.batch_terminator_replacement_string + @"
SQL2;
" + StatementSplitter.batch_terminator_replacement_string + @"
tmpSql := 'DROP SEQUENCE mutatieStockID';
EXECUTE IMMEDIATE tmpSql; 
" + StatementSplitter.batch_terminator_replacement_string + @"
BEGIN
INSERT into Table (columnname) values ("";"");
UPDATE Table set columnname="";"";
END;
";
        }
    }
}