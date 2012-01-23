using System;

namespace FubuCore.Conversion
{
    public interface IObjectConverterFamily
    {
        // ObjectConverter calls this method on unknown types
        // to ask an IObjectConverterFamily if it "knows" how
        // to convert a string into the given type
        bool Matches(Type type, ConverterLibrary converter);

        // If Matches() returns true for a given type, 
        // ObjectConverter asks this IObjectConverterFamily
        // for a converter Lambda and calls its 
        // RegisterFinder() method behind the scenes to cache
        // the Lambda for later usage
        IConverterStrategy CreateConverter(Type type, Func<Type, IConverterStrategy> converterSource);
    }
}