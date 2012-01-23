using System.IO;
using System.Text;
using FubuCore.Util;
using Microsoft.Practices.ServiceLocation;

namespace FubuCore.Binding
{
    public class FlatFileReader<T>
    {
        private readonly IModelBinder _binder;
        private readonly IServiceLocator _services;

        public FlatFileReader(IModelBinder binder, IServiceLocator services)
        {
            _binder = binder;
            _services = services;
        }

        public void ReadFile(FlatFileRequest<T> request)
        {
            using (var stream = new FileStream(request.Filename, FileMode.Open, FileAccess.Read))
            {
                var reader = new StreamReader(stream, request.Encoding);
                var headers = reader.ReadLine();
                if (headers.IsEmpty()) return;

                processData(request, reader, headers);
            }
        }

        private void processData(FlatFileRequest<T> request, StreamReader reader, string headers)
        {
            var data = new FlatFileRequestData(request.Concatenator, headers);
            _aliases.Each((header, alias) => data.Alias(header, alias));

            var context = new BindingContext(data, _services, new NulloBindingLogger());

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                readTargetFromLine(request, data, line, context);
            }
        }

        private void readTargetFromLine(FlatFileRequest<T> request, FlatFileRequestData data, string line, BindingContext context)
        {
            data.ReadLine(line);
                    
            var target = request.Finder(data);
            _binder.Bind(typeof (T), target, context);

            request.Callback(target);
        }

        private readonly Cache<string, string> _aliases = new Cache<string, string>();
        public void Alias(string header, string alias)
        {
            _aliases[header] = alias;
        }
    }
}