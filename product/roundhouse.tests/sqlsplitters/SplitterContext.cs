namespace roundhouse.tests.sqlsplitters
{
    using roundhouse.sqlsplitters;

    public class SplitterContext
    {

        public class FullSplitter
        {
            public static string sql_statement = @"
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

-- dfgjhdfgdjkgk dfgdfg GO
BOB12
";

            public static string sql_statement_scrubbed = @"
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

-- dfgjhdfgdjkgk dfgdfg GO
BOB12
";
        }

    }
}