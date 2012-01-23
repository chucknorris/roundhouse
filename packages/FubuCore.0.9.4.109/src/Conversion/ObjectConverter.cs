using System;
using Microsoft.Practices.ServiceLocation;

namespace FubuCore.Conversion
{
    public class ObjectConverter : IObjectConverter
    {
        public const string NULL = "NULL";
        private readonly Func<Type, object> _finder;
        private readonly ConverterLibrary _library;


        public ObjectConverter()
            : this(type => { throw new NotSupportedException("You must regsiter an IServiceLocator to resolve dependencies."); }, new ConverterLibrary(new IObjectConverterFamily[0]))
        {
        }

        public ObjectConverter(IServiceLocator services, ConverterLibrary library)
            : this(type => services.GetInstance(type), library)
        {
        }

        private ObjectConverter(Func<Type, object> finder, ConverterLibrary library)
        {
            _library = library;
            _finder = finder;
        }

        public bool CanBeParsed(Type type)
        {
            return _library.CanBeParsed(type);
        }

        public IConverterStrategy StrategyFor(Type type)
        {
            return _library.StrategyFor(type);
        }

        public virtual object FromString(string stringValue, Type type)
        {
            if (stringValue == NULL) return null;

            var request = new ConversionRequest(stringValue, _finder);
            return StrategyFor(type).Convert(request);

        }

        public virtual T FromString<T>(string stringValue)
        {
            return (T) FromString(stringValue, typeof (T));
        }
    }
}