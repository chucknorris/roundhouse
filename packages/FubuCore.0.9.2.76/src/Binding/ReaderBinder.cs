using System;
using System.Collections.Generic;
using System.Data;
using FubuCore.Util;
using Microsoft.Practices.ServiceLocation;

namespace FubuCore.Binding
{
    public class RowProcessingRequest<T>
    {
        public RowProcessingRequest()
        {
            Callback = x => { };
        }

        public Func<IDataReader, T> Finder { get; set; }
        public Action<T> Callback { get; set; }
        public IDataReader Reader { get; set; }
    }

    public class ReaderBinder
    {
        private readonly Cache<string, string> _aliases = new Cache<string, string>(key => key);
        private readonly IModelBinder _binder;
        private readonly IServiceLocator _services;

        public ReaderBinder(IModelBinder binder, IServiceLocator services)
        {
            _binder = binder;
            _services = services;
        }

        public IEnumerable<T> Build<T>(Func<IDataReader> getReader) where T : new()
        {
            using (IDataReader reader = getReader())
            {
                return Build<T>(reader);
            }
        }

        public IEnumerable<T> Build<T>(IDataReader reader) where T : new()
        {
            var list = new List<T>();

            Build(new RowProcessingRequest<T>
            {
                Callback = list.Add,
                Finder = r => new T(),
                Reader = reader
            });

            return list;
        }

        public void Build<T>(RowProcessingRequest<T> input)
        {
            IDataReader reader = input.Reader;
            var request = new DataReaderRequestData(reader, _aliases);
            var context = new BindingContext(request, _services, new NulloBindingLogger());

            while (reader.Read())
            {
                T target = input.Finder(reader);
                _binder.Bind(typeof (T), target, context);

                input.Callback(target);
            }
        }

        public void SetAlias(string name, string alias)
        {
            _aliases[name] = alias;
        }
    }
}