using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using developwithpassion.bdd.mbunit.standard.observations;

namespace roundhouse.tests.infrastructure.filesystem.filelocators
{
    using bdddoc.core;
    using developwithpassion.bdd.mbunit;
    using developwithpassion.bdd.mbunit.standard;
    using developwithpassion.bdd.mbunit.standard.observations;
    using roundhouse.infrastructure.extensions;
    using roundhouse.infrastructure.filesystem.filelocators;
    using MbUnit.Framework;

    public class SpecifiedOrderingSpecs
    {
        public abstract class concern_for_specified_ordering : observations_for_a_static_sut
        {
        }

        [Concern(typeof(SpecifiedOrdering.Comparer))]
        public class specified_ordering : concern_for_specified_ordering
        {
            [Observation]
            public void Should_put_items_in_order_of_specified()
            {
                var specified = new[] {
                    @"second\world.sql",
                    @"first\hello.sql"
                };

                var actual = new[] {
                    @"first\hello.sql",
                    @"second\world.sql"
                };

                var comparator = new SpecifiedOrdering.Comparer(specified, "");

                Array.Sort(actual, comparator);

                CollectionAssert.AreEqual(specified, actual);
            }

            [Observation]
            public void Should_put_specified_items_before_unspecified_items()
            {
                var specified = new[] {
                    @"second\world.sql",
                };

                var expected = new[] {
                    @"second\world.sql",
                    @"first\hello.sql"
                };

                var actual = new[] {
                    @"first\hello.sql",
                    @"second\world.sql"
                };

                var comparator = new SpecifiedOrdering.Comparer(specified, "");

                Array.Sort(actual, comparator);

                CollectionAssert.AreEqual(expected, actual);
            }

            [Observation]
            public void Should_put_unspecified_items_in_caseinsensitive_invariant_order()
            {
                var expected = new[] {
                    @"first\hello.sql",
                    @"second\world.sql"
                };

                var actual = new[] {
                    @"second\world.sql",
                    @"first\hello.sql"
                };

                var comparator = new SpecifiedOrdering.Comparer(Enumerable.Empty<string>(), "");

                Array.Sort(actual, comparator);

                CollectionAssert.AreEqual(expected, actual);
            }

            [Observation]
            public void Should_throw_when_item_is_specified_twice()
            {
                bool threw = false;

                try
                {
                    new SpecifiedOrdering.Comparer(new[] { "twice", "twice" }, "");
                }
                catch (ArgumentException ae)
                {
                    threw = true;
                }

                Assert.IsTrue(threw, "ArgumentException should have been thrown, but wasn't");
            }

            // Should return less than when lhs is specified and rhs isn't
            [Observation]
            public void Should_return_less_than_when_lhs_is_specified_and_rhs_is_not()
            {
                string lhs = "specified";

                string rhs = "hello";

                var comparer = new SpecifiedOrdering.Comparer(new [] { lhs }, "");

                Assert.Less(comparer.Compare(lhs, rhs), 0);
            }

            // should return greater than when lhs is not specified and rhs is.
            [Observation]
            public void Should_return_less_than_when_lhs_is_not_specified_and_rhs_is()
            {
                string lhs = "good morning";

                string rhs = "hello";

                var comparer = new SpecifiedOrdering.Comparer(new [] { rhs }, "");

                Assert.Greater(comparer.Compare(lhs, rhs), 0);
            }

            // should return less than when lhs is before rhs in specified list
            [Observation]
            public void Should_return_less_than_when_lhs_is_specified_before_rhs()
            {
                string lhs = "good morning";

                string rhs = "hello";

                var comparer = new SpecifiedOrdering.Comparer(new [] { lhs, rhs }, "");

                Assert.Less(comparer.Compare(lhs, rhs), 0);
            }

            // should return greater than when lhs is after rhs in specified list
            [Observation]
            public void Should_return_greater_than_when_lhs_is_specified_after_rhs()
            {
                string lhs = "good morning";

                string rhs = "hello";

                var comparer = new SpecifiedOrdering.Comparer(new [] { rhs, lhs }, "");

                Assert.Greater(comparer.Compare(lhs, rhs), 0);
            }

            // should return less than when both aren't specified and lhs is less than rhs
            [Observation]
            public void Should_return_less_than_when_neither_are_specified_and_is_lhs_less_than_rhs()
            {
                string lhs = "good morning";

                string rhs = "hello";

                var comparer = new SpecifiedOrdering.Comparer(Enumerable.Empty<string>(), "");

                Assert.Less(comparer.Compare(lhs, rhs), 0);
            }

            // should return greater than when both aren't specified and lhs is greater than rhs

            [Observation]
            public void Should_return_greater_than_when_neither_are_specified_and_is_lhs_greater_than_rhs()
            {
                string lhs = "good morning";

                string rhs = "are you there?";

                var comparer = new SpecifiedOrdering.Comparer(Enumerable.Empty<string>(), "");

                Assert.Greater(comparer.Compare(lhs, rhs), 0);
            }

            // should return 0 when strings are the same.
            [Observation]
            public void Should_return_equal_when_both_sides_are_equal()
            {
                string lhs = "Hello";
                string rhs = "hello";

                var comparer = new SpecifiedOrdering.Comparer(Enumerable.Empty<string>(), "");

                Assert.AreEqual(0, comparer.Compare(lhs, rhs));
            }
        }
    }
}
