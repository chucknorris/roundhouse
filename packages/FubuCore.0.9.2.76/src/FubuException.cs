using System;
using System.Runtime.Serialization;
using FubuCore;

namespace FubuMVC.Core
{
    [Serializable]
    public class FubuException : Exception
    {
        private readonly int _errorCode;
        private readonly string _message;

        public FubuException(int errorCode, string message)
            : base(message)
        {
            _errorCode = errorCode;
            _message = message;
        }

        private FubuException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            _errorCode = errorCode;
            _message = message;
        }

        protected FubuException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _errorCode = info.GetInt32("errorCode");
            _message = info.GetString("message");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("errorCode", _errorCode);
            info.AddValue("message", _message);
        }

        public FubuException(int errorCode, Exception inner, string template, params string[] substitutions)
            : this(errorCode, template.ToFormat(substitutions), inner)
        {
        }

        public FubuException(int errorCode, string template, params string[] substitutions)
            : this(errorCode, template.ToFormat(substitutions))
        {
        }

        public override string Message { get { return "FubuMVC Error {0}:  \n{1}".ToFormat(_errorCode, _message); } }
        
        public int ErrorCode { get { return _errorCode; } }
    }


    [Serializable]
    public class FubuAssertionException : Exception
    {
        public FubuAssertionException(string message) : base(message) { }
        protected FubuAssertionException(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }
}