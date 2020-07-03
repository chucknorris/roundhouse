namespace roundhouse.infrastructure.extensions
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public static class TypeExtensions
    {
        public static readonly string exception_message_typed_format = "<{0}>";

        public static ConstructorInfo greediest_constructor(this Type type)
        {
            return type.GetConstructors()
                .OrderByDescending(x => x.GetParameters().Count())
                .First();
        }


        public static string proper_name(this Type type)
        {
            var message = new StringBuilder(type.Name);
            if (!type.IsGenericType) return message.ToString();

            type.GetGenericArguments().each(x => message.AppendFormat(exception_message_typed_format, x));

            return message.ToString();
        }
    }
}