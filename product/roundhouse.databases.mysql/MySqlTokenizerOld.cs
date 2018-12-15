using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace roundhouse.databases.mysql
{
    class MySqlTokenizerOld
    {
        private string sql;
        private int startIndex;
        private int stopIndex;
        private bool ansiQuotes;
        private bool backslashEscapes;
        private bool returnComments;
        private bool multiLine;
        private bool sqlServerMode;
        private bool quoted;
        private bool isComment;
        private int pos;

        public string Text
        {
            get
            {
                return this.sql;
            }
            set
            {
                this.sql = value;
                this.pos = 0;
            }
        }

        public bool AnsiQuotes
        {
            get
            {
                return this.ansiQuotes;
            }
            set
            {
                this.ansiQuotes = value;
            }
        }

        public bool BackslashEscapes
        {
            get
            {
                return this.backslashEscapes;
            }
            set
            {
                this.backslashEscapes = value;
            }
        }

        public bool MultiLine
        {
            get
            {
                return this.multiLine;
            }
            set
            {
                this.multiLine = value;
            }
        }

        public bool SqlServerMode
        {
            get
            {
                return this.sqlServerMode;
            }
            set
            {
                this.sqlServerMode = value;
            }
        }

        public bool Quoted
        {
            get
            {
                return this.quoted;
            }
            private set
            {
                this.quoted = value;
            }
        }

        public bool IsComment
        {
            get
            {
                return this.isComment;
            }
        }

        public int StartIndex
        {
            get
            {
                return this.startIndex;
            }
            set
            {
                this.startIndex = value;
            }
        }

        public int StopIndex
        {
            get
            {
                return this.stopIndex;
            }
            set
            {
                this.stopIndex = value;
            }
        }

        public int Position
        {
            get
            {
                return this.pos;
            }
            set
            {
                this.pos = value;
            }
        }

        public bool ReturnComments
        {
            get
            {
                return this.returnComments;
            }
            set
            {
                this.returnComments = value;
            }
        }

        public MySqlTokenizerOld()
        {
            this.backslashEscapes = true;
            this.multiLine = true;
            this.pos = 0;
        }

        public MySqlTokenizerOld(string input)
            : this()
        {
            this.sql = input;
        }

        public List<string> GetAllTokens()
        {
            List<string> list = new List<string>();
            for (string str = this.NextToken(); str != null; str = this.NextToken())
                list.Add(str);
            return list;
        }

        public string NextToken()
        {
            if (this.FindToken())
                return this.sql.Substring(this.startIndex, this.stopIndex - this.startIndex);
            else
                return (string)null;
        }

        public static bool IsParameter(string s)
        {
            return !string.IsNullOrEmpty(s) && ((int)s[0] == 63 || s.Length > 1 && (int)s[0] == 64 && (int)s[1] != 64);
        }

        public string NextParameter()
        {
            while (this.FindToken())
            {
                if (this.stopIndex - this.startIndex >= 2)
                {
                    char ch1 = this.sql[this.startIndex];
                    char ch2 = this.sql[this.startIndex + 1];
                    if ((int)ch1 == 63 || (int)ch1 == 64 && (int)ch2 != 64)
                        return this.sql.Substring(this.startIndex, this.stopIndex - this.startIndex);
                }
            }
            return (string)null;
        }

        public bool FindToken()
        {
            this.isComment = this.quoted = false;
            this.startIndex = this.stopIndex = -1;
            while (this.pos < this.sql.Length)
            {
                char ch = this.sql[this.pos++];
                if (!char.IsWhiteSpace(ch))
                {
                    if ((int)ch == 96 || (int)ch == 39 || (int)ch == 34 || (int)ch == 91 && this.SqlServerMode)
                        this.ReadQuotedToken(ch);
                    else if ((int)ch == 35 || (int)ch == 45 || (int)ch == 47)
                    {
                        if (!this.ReadComment(ch))
                            this.ReadSpecialToken();
                    }
                    else
                        this.ReadUnquotedToken();
                    if (this.startIndex != -1)
                        return true;
                }
            }
            return false;
        }

        public string ReadParenthesis()
        {
            StringBuilder stringBuilder = new StringBuilder("(");
            int startIndex = this.StartIndex;
            for (string str = this.NextToken(); str != null; str = this.NextToken())
            {
                stringBuilder.Append(str);
                if (str == ")" && !this.Quoted)
                    return ((object)stringBuilder).ToString();
            }
            throw new InvalidOperationException("Unable to parse SQL");
        }

        private bool ReadComment(char c)
        {
            if ((int)c == 47 && (this.pos >= this.sql.Length || (int)this.sql[this.pos] != 42) || (int)c == 45 && (this.pos + 1 >= this.sql.Length || (int)this.sql[this.pos] != 45 || (int)this.sql[this.pos + 1] != 32))
                return false;
            string str = "\n";
            if ((int)this.sql[this.pos] == 42)
                str = "*/";
            int num1 = this.pos - 1;
            int num2 = this.sql.IndexOf(str, this.pos);
            if (str == "\n")
                num2 = this.sql.IndexOf('\n', this.pos);
            int num3 = num2 != -1 ? num2 + str.Length : this.sql.Length - 1;
            this.pos = num3;
            if (this.ReturnComments)
            {
                this.startIndex = num1;
                this.stopIndex = num3;
                this.isComment = true;
            }
            return true;
        }

        private void CalculatePosition(int start, int stop)
        {
            this.startIndex = start;
            this.stopIndex = stop;
            int num = this.MultiLine ? 1 : 0;
        }

        private void ReadUnquotedToken()
        {
            this.startIndex = this.pos - 1;
            if (!this.IsSpecialCharacter(this.sql[this.startIndex]))
            {
                for (; this.pos < this.sql.Length; ++this.pos)
                {
                    char c = this.sql[this.pos];
                    if (char.IsWhiteSpace(c) || this.IsSpecialCharacter(c))
                        break;
                }
            }
            this.Quoted = false;
            this.stopIndex = this.pos;
        }

        private void ReadSpecialToken()
        {
            this.startIndex = this.pos - 1;
            this.stopIndex = this.pos;
            this.Quoted = false;
        }

        private void ReadQuotedToken(char quoteChar)
        {
            if ((int)quoteChar == 91)
                quoteChar = ']';
            this.startIndex = this.pos - 1;
            bool flag1 = false;
            bool flag2 = false;
            for (; this.pos < this.sql.Length; ++this.pos)
            {
                char ch = this.sql[this.pos];
                if ((int)ch == (int)quoteChar && !flag1)
                {
                    flag2 = true;
                    break;
                }
                else if (flag1)
                    flag1 = false;
                else if ((int)ch == 92 && this.BackslashEscapes)
                    flag1 = true;
            }
            if (flag2)
                ++this.pos;
            this.Quoted = flag2;
            this.stopIndex = this.pos;
        }

        private bool IsQuoteChar(char c)
        {
            if ((int)c != 96 && (int)c != 39)
                return (int)c == 34;
            else
                return true;
        }

        private bool IsParameterMarker(char c)
        {
            if ((int)c != 64)
                return (int)c == 63;
            else
                return true;
        }

        private bool IsSpecialCharacter(char c)
        {
            return !char.IsLetterOrDigit(c) && (int)c != 36 && ((int)c != 95 && (int)c != 46) && !this.IsParameterMarker(c);
        }
    }
}
