namespace roundhouse.tests.infrastructure.extensions
{
    using bdddoc.core;
    using developwithpassion.bdd.mbunit;
    using developwithpassion.bdd.mbunit.standard;
    using developwithpassion.bdd.mbunit.standard.observations;
    using roundhouse.infrastructure.extensions;

    public abstract class concern_for_iteration_extensions : observations_for_a_static_sut
    {
    }

    [Concern(typeof(Iteration))]
    public class when_iterating_through_a_range_of_numbers : concern_for_iteration_extensions
    {
        [Observation]
        public void Should_visit_all_numbers()
        {
            var numbers_visited = 0;
            foreach(var number in 1.to(2))
            {
                numbers_visited++;
            }
            numbers_visited.should_be_equal_to(2);
        }
    }
}