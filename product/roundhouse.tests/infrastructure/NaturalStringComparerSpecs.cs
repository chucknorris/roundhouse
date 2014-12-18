using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace roundhouse.tests.infrastructure
{
    using roundhouse.infrastructure;
    using bdddoc.core;
    using developwithpassion.bdd.contexts;
    using developwithpassion.bdd.mbunit;
    using developwithpassion.bdd.mbunit.standard;
    using developwithpassion.bdd.mbunit.standard.observations;

    public class NaturalStringComparerSpecs
    {
        public abstract class concern_for_comparer : observations_for_a_sut_with_a_contract<IComparer<string>, NaturalStringComparer>
        {
            protected static object result;

            context c = () => { };
        }

        [Concern(typeof(NaturalStringComparer))]
        public class when_sorting : concern_for_comparer
        {
            private const string zeroOne = "001_Foo";
            private const string zeroTwo = "002_Foo";
            private const string zeroThree = "003_Foo";
            private const string zeroFour = "004_Foo";
            private const string zeroTen = "010_Foo";
            private const string zeroEleven = "011_Foo";
            private const string zeroTwelve = "012_Foo";
            private const string zeroThirteen = "013_Foo";

            private const string one = "1_Foo";
            private const string two = "2_Foo";
            private const string three = "3_Foo";
            private const string four = "4_Foo";
            private const string ten = "10_Foo";
            private const string eleven = "11_Foo";
            private const string twelve = "12_Foo";
            private const string thirteen = "13_Foo";

            [Observation]
            public void simple_example_should_sort_correctly()
            {
                string unsorted = "egadfcb";
                string sortedExpected = "abcdefg";
                string sortedActual = new string(unsorted.OrderBy(x => x.ToString(), new NaturalStringComparer()).ToArray());
                sortedActual.should_be_equal_to(sortedExpected);
            }

            [Observation]
            public void zero_padded_numbers_should_sort_correctly()
            {
                string[] unsorted = new[] { zeroOne, zeroTen, zeroEleven, zeroTwo, zeroThree };
                string[] sortedExpected = new[] { zeroOne, zeroTwo, zeroThree, zeroTen, zeroEleven };
                string[] sortedActual = unsorted.OrderBy(x => x, new NaturalStringComparer()).ToArray();
                sortedActual.should_be_equal_to(sortedExpected);
            }

            [Observation]
            public void unpadded_numbers_should_sort_correctly()
            {
                string[] unsorted = new[] { one, ten, eleven, two, three };
                string[] sortedExpected = new[] { one, two, three, ten, eleven };
                string[] sortedActual = unsorted.OrderBy(x => x, new NaturalStringComparer()).ToArray();
                sortedActual.should_be_equal_to(sortedExpected);
            }

            [Observation]
            public void mixture_of_zero_padded_and_unpadded_numbers_should_sort_correctly()
            {
                string[] unsorted = new[] { four, eleven, thirteen, zeroOne, zeroThree, two, zeroTen, zeroTwelve };
                string[] sortedExpected = new[] { zeroOne, two, zeroThree, four, zeroTen, eleven, zeroTwelve, thirteen };
                string[] sortedActual = unsorted.OrderBy(x => x, new NaturalStringComparer()).ToArray();
                sortedActual.should_be_equal_to(sortedExpected);
            }
        }
    }
}