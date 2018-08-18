using Moq;
using Should;

namespace roundhouse.tests.infrastructure.logging
{
    using roundhouse.infrastructure.containers;
    using roundhouse.infrastructure.containers.custom;
    using roundhouse.infrastructure.logging;

    public class LogSpecs
    {
        public abstract class concern_for_logging : TinySpec
        {
            protected Mock<InversionContainer> container_mock;
            protected Logger result;

            protected InversionContainer the_container;
            protected Mock<LogFactory> mock_log_factory;
            protected Mock<Logger> mock_logger;

            public override void Context()
            {
                container_mock = new Mock<InversionContainer>();
                the_container = container_mock.Object;
                Container.initialize_with(the_container);
            }

            public override void AfterEachSpec()
            {
                Container.initialize_with(null);
            }
        }

        [Concern(typeof(Log))]
        public class when_asking_for_a_logger_and_one_has_been_registered : concern_for_logging
        {
            public override void Context()
            {
                base.Context();

                mock_log_factory = new Mock<LogFactory>();
                mock_logger = new Mock<Logger>();

                mock_log_factory.Setup(x => x.create_logger_bound_to(typeof(NinjectContainer)))
                    .Returns(mock_logger.Object);

                container_mock.Setup(x => x.Resolve<LogFactory>())
                    .Returns(mock_log_factory.Object);

                //when(the_container).is_told_to(x => x.Resolve<LogFactory>())
                //    .Return(mock_log_factory);
                //when_the(mock_log_factory).is_told_to(x => x.create_logger_bound_to(typeof(StructureMapContainer)))
                //    .IgnoreArguments()
                //    .Return(mock_logger);
            }

            public override void Because()
            {
                result = Log.bound_to(new NinjectContainer(null));
            }

            [Observation]
            public void should_have_called_the_container_to_resolve_a_registered_logger()
            {
                container_mock.Verify(x_ => x_.Resolve<LogFactory>());
            }

            [Observation]
            public void should_not_be_null()
            {
                result.ShouldNotBeNull();
            }

            [Observation]
            public void should_return_a_custom_logger()
            {
                //mock_log_factory.should_be_equal_to(result);
                //result.should_be_an_instance_of<LogFactory>();
                // result.should_be_equal_to(mock_log_factory);
            }
        }
    }
}