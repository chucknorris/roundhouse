namespace roundhouse.infrastructure.loaders
{
    using System;
    using System.Reflection;
    using logging;

    public class DefaultInstanceCreator
    {
        //Type generic = typeof(Container.get_an_instance_of<>);
        //Type specific = generic.MakeGenericType(typeof(from.system_type));
        //ConstructorInfo ci = specific.GetConstructor(new Type[] { });
        //object o = ci.Invoke(new object[] { });

        //http://www.google.com/search?hl=en&q=passing+runtime+Type+to+Generic&aq=f&oq=&aqi=
        //http://stackoverflow.com/questions/513952/c-specifying-generic-collection-type-param-at-runtime

        public static T create_object_from_string_type<T>(string object_to_create)
        {
            return (T) create_object_from_string_type(object_to_create);
        }
        public static object create_object_from_string_type(string object_to_create)
        {
            Log.bound_to(typeof(DefaultInstanceCreator)).log_a_debug_event_containing("Resolving and creating an instance of \"{0}\".", object_to_create);

            Type object_type = Type.GetType(object_to_create);

            if (object_type == null) throw new NullReferenceException(string.Format("A type could not be created from the object you passed. \"{0}\" resolves to null.", object_to_create));

            ConstructorInfo ci = object_type.GetConstructor(new Type[] { });

            return ci.Invoke(new object[] { });
        }
    }
}