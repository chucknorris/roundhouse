using FluentAssertions;
using Xunit;

namespace roundhouse.tests.infrastructure.extensions
{
    using roundhouse.infrastructure.extensions;

    public abstract class concern_for_iteration_extensions 
    {
    }

    public class when_iterating_through_a_range_of_numbers : concern_for_iteration_extensions
    {
        [Fact]
        public void Should_visit_all_numbers()
        {
            var numbers_visited = 0;
            foreach(var number in 1.to(2))
            {
                numbers_visited++;
            }
            numbers_visited.Should().Be(2);
        }
    }
}