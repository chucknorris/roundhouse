using System;
using System.Reflection;
using FubuCore.Conversion;

namespace FubuCore.Binding
{
    public class BasicConverterFamily : IConverterFamily
    {
        private readonly ConverterLibrary _library;


        public BasicConverterFamily(ConverterLibrary library)
        {
            if (library == null) throw new ArgumentNullException("library");

            _library = library;
        }

        public bool Matches(PropertyInfo property)
        {
            return _library.CanBeParsed(property.PropertyType);
        }


        public ValueConverter Build(IValueConverterRegistry registry, PropertyInfo property)
        {
            var propertyType = property.PropertyType;

            var strategy = _library.StrategyFor(propertyType);
            return new BasicValueConverter(strategy, propertyType);
        }
    }

    public class BasicValueConverter : ValueConverter
    {
        private readonly IDefaultMaker _defaulter;
        private readonly Type _propertyType;
        private readonly IConverterStrategy _strategy;

        public BasicValueConverter(IConverterStrategy strategy, Type propertyType)
        {
            _strategy = strategy;
            _propertyType = propertyType;
            _defaulter = typeof (DefaultMaker<>).CloseAndBuildAs<IDefaultMaker>(propertyType);
        }

        public object Convert(IPropertyContext context)
        {
            if (context.PropertyValue == null) return _defaulter.Default();


            return context.PropertyValue.GetType().CanBeCastTo(_propertyType)
                       ? context.PropertyValue
                       : _strategy.Convert(context);
        }

        #region Nested type: DefaultMaker

        public class DefaultMaker<T> : IDefaultMaker
        {
            public object Default()
            {
                return default(T);
            }
        }

        #endregion

        #region Nested type: IDefaultMaker

        public interface IDefaultMaker
        {
            object Default();
        }

        #endregion
    }
}