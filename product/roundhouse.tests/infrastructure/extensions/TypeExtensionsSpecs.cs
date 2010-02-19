namespace roundhouse.tests.infrastructure.extensions
{
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using bdddoc.core;
    using developwithpassion.bdd.contexts;
    using developwithpassion.bdd.mbunit;
    using developwithpassion.bdd.mbunit.standard;
    using developwithpassion.bdd.mbunit.standard.observations;
    using roundhouse.infrastructure.extensions;

    public abstract class concern_for_type_extensions : observations_for_a_static_sut
    {
    }

    [Concern(typeof(TypeExtensions))]
    public class when_a_type_is_told_to_find_its_greediest_constructor : observations_for_a_static_sut
    {
        static ConstructorInfo result;


        because b = () =>
                        {
                            result = typeof(SomethingWithConstructors).greediest_constructor();
                        };


        [Observation]
        public void should_return_the_constructor_that_takes_the_most_arguments()
        {
            result.GetParameters().Count().should_be_equal_to(2);
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