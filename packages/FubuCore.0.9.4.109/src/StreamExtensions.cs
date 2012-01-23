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


		public static byte[] ReadAllBytes(this Stream stream)
		{
			using (var content = new MemoryStream())
			{
				var buffer = new byte[4096];

				int read = stream.Read(buffer, 0, 4096);
				while (read > 0)
				{
					content.Write(buffer, 0, read);

					read = stream.Read(buffer, 0, 4096);
				}

				return content.ToArray();
			}
		}
    }
}