using Newtonsoft.Json;

namespace roundhouse.databases.ravendb.serializers
{
    public class JsonSerializer : ISerializer
    {
        public string SerializeObject(object data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public T DeserializeObject<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}