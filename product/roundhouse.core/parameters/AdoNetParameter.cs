namespace roundhouse.parameters
{
    using System.Data;

    public class AdoNetParameter : IParameter<IDbDataParameter>
    {
        private readonly IDbDataParameter parameter;

        public AdoNetParameter(IDbDataParameter parameter)
        {
            this.parameter = parameter;
        }

        public IDbDataParameter underlying_type
        {
            get { return parameter; }
        }

        public string name
        {
            get { return parameter != null ? parameter.ParameterName : string.Empty; }
        }

        public object value
        {
            get { return parameter != null ? parameter.Value : string.Empty; }
        }
    }
}