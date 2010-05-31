namespace roundhouse.parameters
{
    public interface IParameter<T>
    {
        T underlying_type {get;}
        string name {get;}
        object value{get;}
    }
}