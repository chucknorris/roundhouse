using System;
using System.Text;

namespace FubuCore.Binding
{
    public class FlatFileRequest<T>
    {
        public FlatFileRequest()
        {
            Encoding = Encoding.Default;
        }

        public Action<T> Callback { get; set; }
        public Func<IRequestData, T> Finder { get; set; }
        public string Filename { get; set; }
        public string Concatenator { get; set; }
        public Encoding Encoding { get; set; }
    }
}