namespace roundhouse.tests.infrastructure.logging.custom
{
    using bdddoc.core;
    using developwithpassion.bdd.contexts;
    using developwithpassion.bdd.mbunit;
    using developwithpassion.bdd.mbunit.standard;
    using developwithpassion.bdd.mbunit.standard.observations;
    using roundhouse.infrastructure.logging;
    using roundhouse.infrastructure.logging.custom;

    public class Log4NetLogFactorySpecs
    {
        public abstract class concern_for_Log4NetLogFactory : observations_for_a_sut_with_a_contract<LogFactory, Log4NetLogFactory>
        {
        }

        [Concern(typeof(Log4NetLogFactory))]
        public class when_creating_a_log4netfactory : concern_for_Log4NetLogFactory
        {
            protected static object result;

            context c = () => { };

            because b = () => result = sut.create_logger_bound_to(typeof(when_creating_a_log4netfactory));

            [Observation]
            public void should_create_an_object_of_type_Logger()
            {
                result.should_be_an_instance_of<Logger>();
            }
        }
    }
}