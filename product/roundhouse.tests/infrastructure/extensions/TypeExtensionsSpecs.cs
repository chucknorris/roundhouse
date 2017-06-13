using FluentAssertions;
using Xunit;

namespace roundhouse.tests.infrastructure.extensions
{
    using System.Data;
    using System.Reflection;
    using roundhouse.infrastructure.extensions;

    public abstract class concern_for_type_extensions 
    {
    }

    public class when_a_type_is_told_to_find_its_greediest_constructor
    {
        ConstructorInfo result;

        public when_a_type_is_told_to_find_its_greediest_constructor()
        {
            result = typeof(SomethingWithConstructors).greediest_constructor();
        }


        [Fact]
        public void should_return_the_constructor_that_takes_the_most_arguments()
        {
            result.GetParameters().Length.Should().Be(2);
        }
    }

    public class SomethingWithConstructors
    {
        public IDbConnection connection { get; set; }

        public IDbCommand command { get; set; }

        public SomethingWithConstructors(IDbConnection connection)
        {
        }

        public SomethingWithConstructors(IDbConnection connection, IDbCommand command)
        {
            this.connection = connection;
            this.command = command;
        }
    }
}