using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FubuCore.Binding
{
    [Serializable]
    public class BindResultAssertionException : Exception
    {
        private readonly Type _type;
        private readonly IList<ConvertProblem> _problems;

        public BindResultAssertionException(Type type, IList<ConvertProblem> problems)
        {
            _type = type;
            _problems = problems;
        }

        protected BindResultAssertionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _type = (Type)info.GetValue("bindType", typeof (Type));
            _problems = (IList<ConvertProblem>)info.GetValue("problems", typeof (IList<ConvertProblem>));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("bindType", _type);
            info.AddValue("problems", _problems);
        }

        public override string Message
        {
            get 
            {
                var builder = new StringBuilder();
                builder.AppendFormat("Failure while trying to bind object of type '{0}'", _type.FullName);
                
                _problems.Each(p =>
                {
                    builder.AppendFormat("Property: {0}, Value: '{1}', Exception:{2}{3}{2}",
                                         p.PropertyName(), p.Value, Environment.NewLine, p.ExceptionText);
                });

                return builder.ToString();
            } 
        }

        public IList<ConvertProblem> Problems { get { return _problems; } }
        public Type Type { get { return _type; } }
    }
}