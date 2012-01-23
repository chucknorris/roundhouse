using System;

namespace FubuCore.Util
{
    public class Indexer<TKey, TValue>
    {
        private readonly Func<TKey, TValue> _getter;
        private readonly Action<TKey, TValue> _setter;

        public Indexer(Func<TKey, TValue> getter, Action<TKey, TValue> setter)
        {
            _getter = getter;
            _setter = setter;
        }

        public TValue this[TKey key] { get { return _getter(key); } set { _setter(key, value); } }
    }
}