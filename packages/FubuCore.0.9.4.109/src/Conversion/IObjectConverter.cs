using System;

namespace FubuCore.Conversion
{
    public interface IObjectConverter
    {
        /// <summary>
        ///   Given a string and a .Net type, read this string
        ///   and give me back a corresponding instance of that
        ///   type
        /// </summary>
        /// <param name = "stringValue">The value to convert</param>
        /// <param name = "type">The desired destination type</param>
        /// <returns>The value converted into the specified desination type</returns>
        object FromString(string stringValue, Type type);

        /// <summary>
        ///   Given a string and a .Net type, T, read this string
        ///   and give me back a corresponding instance of type T.
        /// </summary>
        /// <typeparam name = "T">The desired destination type</typeparam>
        /// <param name = "stringValue">The value to convert</param>
        /// <returns>The value converted into the specified desination type</returns>
        T FromString<T>(string stringValue);

        /// <summary>
        ///   Determines whether there is conversion support registered for the specified type
        /// </summary>
        /// <param name = "type">The desired destination type</param>
        /// <returns>True if conversion to this type is supported, otherwise False.</returns>
        bool CanBeParsed(Type type);
    }
}