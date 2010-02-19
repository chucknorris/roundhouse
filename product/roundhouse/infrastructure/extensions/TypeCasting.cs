namespace roundhouse.infrastructure.extensions
{
    public static class TypeCasting
    {
        public static T downcast_to<T>(this object item)
        {
            return (T) item;
        }

        public static bool is_not_a<T>(this object item)
        {
            try
            {
                var typeToCastTo = (T) item;
                return false;
            }
            catch
            {
                return true;
            }
        }
    }
}