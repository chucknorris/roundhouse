using System.IO;

namespace FubuCore
{
    public static class StreamExtensions
    {
        public static string ReadAllText(this Stream stream)
        {
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}