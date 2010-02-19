namespace roundhouse.tests.infrastructure.extensions
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using bdddoc.core;
    using developwithpassion.bdd.mbunit;
    using developwithpassion.bdd.mbunit.standard;
    using developwithpassion.bdd.mbunit.standard.observations;
    using roundhouse.infrastructure.extensions;

    public abstract class concern_for_type_casting : observations_for_a_static_sut
    {
    }

    [Concern(typeof(TypeCasting))]
    public class When_a_legitimate_downcast_is_made : concern_for_type_casting
    {
        [Observation]
        public void Should_retrieve_the_object_back_downcasted_to_the_target_type()
        {
            IList<int> list = new List<int>();
            var to = list.downcast_to<List<int>>();
        }
    }

    [Concern(typeof(TypeCasting))]
    public class When_asking_if_an_object_is_not_an_instance_of_a_specific_type : concern_for_type_casting
    {
        [Observation]
        public void Should_be_true_if_the_object_is_not_an_instance_of_the_specified_type()
        {
            new SqlConnection().is_not_a<IDbCommand>().should_be_true();
        }

        [Observation]
        public void Should_be_false_if_the_object_is_an_instance_of_the_specified_type()
        {
            new SqlConnection().is_not_a<IDbConnection>().should_be_false();
        }
    }
}