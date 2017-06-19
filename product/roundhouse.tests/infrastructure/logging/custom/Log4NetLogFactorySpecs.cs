namespace roundhouse.tests.infrastructure.logging.custom
{
    using roundhouse.infrastructure.logging;
    using roundhouse.infrastructure.logging.custom;

    public class Log4NetLogFactorySpecs
    {
        public abstract class concern_for_Log4NetLogFactory : TinySpec<Log4NetLogFactory>
        {
            private Log4NetLogFactory log4_net_log_factory = new Log4NetLogFactory();

            protected override Log4NetLogFactory sut
            {
                get { return log4_net_log_factory; }
                set { log4_net_log_factory = value; }
            }
        }

        [Concern(typeof(Log4NetLogFactory))]
        public class when_creating_a_log4netfactory : concern_for_Log4NetLogFactory
        {
            protected static object result;

            public override void Context() {}

            public override void Because() { result = sut.create_logger_bound_to(typeof(when_creating_a_log4netfactory)); }

            [Observation]
            public void should_create_an_object_of_type_Logger()
            {
                result.should_be_an_instance_of<Logger>();
            }
        }
    }
}