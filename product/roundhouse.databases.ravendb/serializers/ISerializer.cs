namespace roundhouse.databases.ravendb.serializers
{

    public interface ISerializer
    {
        string SerializeObject(object data);
        T DeserializeObject<T>(string data);
    }
}
