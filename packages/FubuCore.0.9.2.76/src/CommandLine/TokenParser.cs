using System.Collections.Generic;

namespace FubuCore.CommandLine
{
    public class TokenParser
    {
        private readonly List<string> _tokens = new List<string>();
        private List<char> _characters;
        private IMode _mode;

        public TokenParser()
        {
            _mode = new Searching(this);
        }

        public void Read(char c)
        {
            _mode.Read(c);
        }

        private void addChar(char c)
        {
            _characters.Add(c);
        }

        public IEnumerable<string> Tokens
        {
            get
            {
                return _tokens;
            }
        }

        private void startToken(IMode mode)
        {
            _mode = mode;
            _characters = new List<char>();
        }

        private void endToken()
        {
            var @string = new string(_characters.ToArray());
            _tokens.Add(@string);

            _mode = new Searching(this);
        }


        public interface IMode
        {
            void Read(char c);
        }

        public class Searching : IMode
        {
            private readonly TokenParser _parent;

            public Searching(TokenParser parent)
            {
                _parent = parent;
            }

            public void Read(char c)
            {
                if (char.IsWhiteSpace(c)) return;

                if (c == '"')
                {
                    _parent.startToken(new InsideQuotedToken(_parent));
                }
                else
                {
                    var normalToken = new InsideNormalToken(_parent);
                    _parent.startToken(normalToken);
                    normalToken.Read(c);
                }
            }
        }

        public class InsideQuotedToken : IMode
        {
            private readonly TokenParser _parent;

            public InsideQuotedToken(TokenParser parent)
            {
                _parent = parent;
            }


            public void Read(char c)
            {
                if (c == '"')
                {
                    _parent.endToken();
                }
                else
                {
                    _parent.addChar(c);
                }
            }
        }

        public class InsideNormalToken : IMode
        {
            private readonly TokenParser _parent;

            public InsideNormalToken(TokenParser parent)
            {
                _parent = parent;
            }

            public void Read(char c)
            {
                if (char.IsWhiteSpace(c))
                {
                    _parent.endToken();
                }
                else
                {
                    _parent.addChar(c);
                }
            }
        }
    }
}