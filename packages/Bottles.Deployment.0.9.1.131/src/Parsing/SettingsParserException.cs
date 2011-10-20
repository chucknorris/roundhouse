using System;
using System.IO;
using System.Runtime.Serialization;

namespace Bottles.Deployment.Parsing
{


    [Serializable]
    public class SettingsParserException : Exception
    {
        private string _message;

        public SettingsParserException(string message)
        {
            _message = message;
        }

        protected SettingsParserException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override string Message
        {
            get { return _message; }
        }

        public void AppendText(string text)
        {
            _message += "\n" + text;
        }
    }
}