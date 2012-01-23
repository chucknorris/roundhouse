using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Util;

namespace FubuCore.Conversion
{
    /// <summary>
    /// Acts as an improved version of the System.ComponentModel.TypeDescriptor class
    /// to store and access strategies for converting a string into a certain Type
    /// </summary>
    public class ConverterLibrary
    {
        private readonly IList<IObjectConverterFamily> _families = new List<IObjectConverterFamily>();
        private readonly Cache<Type, IConverterStrategy> _froms;

        public ConverterLibrary() : this(new IObjectConverterFamily[0])
        {
        }

        public ConverterLibrary(IEnumerable<IObjectConverterFamily> families)
        {
            _froms = new Cache<Type, IConverterStrategy>(createFinder);

            // Strategies that are injected *must* be put first
            _families.AddRange(families);

            _families.Add(new StringConverterStrategy());
            _families.Add(new DateTimeConverter());
            _families.Add(new TimeSpanConverter());
            _families.Add(new TimeZoneConverter());

            _families.Add(new EnumConverterFamily());
            _families.Add(new ArrayConverterFamily());
            _families.Add(new NullableConverterFamily());
            _families.Add(new StringConstructorConverterFamily());
            _families.Add(new TypeDescripterConverterFamily());
        }

        /// <summary>
        /// Register a conversion strategy for a single type by a Func
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="finder"></param>
        public void RegisterConverter<T>(Func<string, T> finder)
        {
            _froms[typeof (T)] = new LambdaConverterStrategy<T>(finder);
        }


        /// <summary>
        /// Register a conversion strategy for a single type TReturnType that uses
        /// an instance of a service type TService
        /// </summary>
        /// <typeparam name="TReturnType"></typeparam>
        /// <typeparam name="TService"></typeparam>
        /// <param name="converter"></param>
        public void RegisterConverter<TReturnType, TService>(Func<TService, string, TReturnType> converter)
        {
            _froms[typeof (TReturnType)] = new LambdaConverterStrategy<TReturnType, TService>(converter);
        }


        public void RegisterConverterFamily(IObjectConverterFamily family)
        {
            _families.Insert(0, family);
        }

        private IConverterStrategy createFinder(Type type)
        {
            var family = _families.FirstOrDefault(x => x.Matches(type, this));
            if (family != null)
            {
                return family.CreateConverter(type, t => _froms[t]);
            }

            throw new ArgumentException("No conversion exists for " + type.AssemblyQualifiedName);
        }

        /// <summary>
        /// Can the ConverterLibrary determine a strategy for parsing a string into the Type?
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool CanBeParsed(Type type)
        {
            return _froms.Has(type) || _families.Any(x => x.Matches(type, this));
        }

        /// <summary>
        /// Locates or resolves a strategy for converting a string into the requested Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IConverterStrategy StrategyFor(Type type)
        {
            return _froms[type];
        }
    }
}