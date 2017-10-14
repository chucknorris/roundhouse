using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;

namespace roundhouse.tests
{
    public static class TestExtensions
    {
        public static void should_not_contain(this string item, string test_value)
        {
            Assert.IsFalse(item.Contains(test_value));
        }

        public static void should_be_equal_to(this string item, string test_value)
        {
            Assert.AreEqual(test_value, item);
        }

        public static void should_not_be_an_instance_of<T>(this object item)
        {
            Assert.IsNotInstanceOf<T>(item);
        }

        public static void should_be_an_instance_of<T>(this object item)
        {
            Assert.IsInstanceOf<T>(item);
        }

        public static void should_throw_an<T>(this Action a) where T: Exception
        {
            Assert.Throws<T>(() => a());
        }

        public static void should_only_contain<K,V>(this IDictionary<K,V> dictionary, params KeyValuePair<K,V>[] values)
        {
            Assert.That(dictionary, Has.Count.EqualTo(values.Length));
            foreach(var value in values)
            {
                Assert.That(dictionary, Contains.Item(value));
            }
        }
    }

}