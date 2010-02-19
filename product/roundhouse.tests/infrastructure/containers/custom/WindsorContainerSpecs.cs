namespace roundhouse.tests.infrastructure.containers.custom
{
    using System;
    using bdddoc.core;
    using Castle.MicroKernel;
    using Castle.Windsor;
    using developwithpassion.bdd.contexts;
    using developwithpassion.bdd.mbunit;
    using developwithpassion.bdd.mbunit.standard;
    using developwithpassion.bdd.mbunit.standard.observations;
    using Rhino.Mocks;
    using roundhouse.infrastructure.containers;
    using roundhouse.infrastructure.logging;
    using roundhouse.infrastructure.logging.custom;
    using WindsorContainer=roundhouse.infrastructure.containers.custom.WindsorContainer;

    public class WindsorContainerSpecs
    {
        public abstract class concern_for_windsor_container :
            observations_for_a_sut_with_a_contract<InversionContainer, WindsorContainer>
        {
            protected static IWindsorContainer the_container;

            private after_each_observation a = () => Container.initialize_with(null);
        }

        public abstract class concerns_using_a_fake_container : concern_for_windsor_container
        {
            private context c = () =>
                                    {
                                        the_container = an<IWindsorContainer>();
                                        provide_a_basic_sut_constructor_argument(the_container);
                                    };
        }

        public abstract class concerns_using_a_real_container : concern_for_windsor_container
        {
            private context c = () =>
                                    {
                                        the_container = new Castle.Windsor.WindsorContainer();
                                        provide_a_basic_sut_constructor_argument(the_container);
                                    };
        }

        [Concern(typeof (WindsorContainer))]
        public class when_the_container_is_initialized : concerns_using_a_fake_container
        {
            private because b = () => { };

            [Observation]
            public void should_be_an_instance_of_IWindsorContainer()
            {
                the_container.should_be_an_instance_of<IWindsorContainer>();
            }
        }

        [Concern(typeof (WindsorContainer))]
        public class when_asking_the_container_for_an_item_and_it_has_that_that_item_registered :
            concerns_using_a_fake_container
        {
            private static LogFactory result;

            private context c =
                () => { the_container.Stub(x => x.Resolve<LogFactory>()).Return(new Log4NetLogFactory()); };

            private because b = () => { result = sut.Resolve<LogFactory>(); };

            [Observation]
            public void should_return_the_item_successfully()
            {
                result.should_be_an_instance_of<LogFactory>();
            }
        }

        [Concern(typeof (WindsorContainer))]
        public class when_asking_the_container_using_castle_windsor_for_an_item_and_it_has_that_that_item_registered :
            concerns_using_a_real_container
        {
            private static LogFactory result;

            private context c =
                () => { the_container.AddComponent("h", typeof (LogFactory), typeof (Log4NetLogFactory)); };

            private because b = () => { result = sut.Resolve<LogFactory>(); };

            [Observation]
            public void should_return_the_item_successfully()
            {
                result.should_be_an_instance_of<Log4NetLogFactory>();
            }
        }

        [Concern(typeof (WindsorContainer))]
        public class when_asking_the_container_to_resolve_an_item_and_it_does_not_have_the_item_registered :
            concerns_using_a_fake_container
        {
            private static Action attempting_to_get_an_unregistered_item;

            private context c = () =>
                                    {
                                        the_container.Stub(x => x.Resolve<LogFactory>()).Throw(
                                            new ComponentNotFoundException(typeof (LogFactory)));
                                    };

            private because b =
                () => { attempting_to_get_an_unregistered_item = () => the_container.Resolve<LogFactory>(); };

            [Observation]
            public void should_throw_an_exception()
            {
                attempting_to_get_an_unregistered_item.should_throw_an<Exception>();
            }
        }

        [Concern(typeof (WindsorContainer))]
        public class
            when_asking_the_container_using_castle_windsor_to_resolve_an_item_and_it_does_not_have_the_item_registered :
                concerns_using_a_real_container
        {
            private static Action attempting_to_get_an_unregistered_item;

            private because b =
                () => { attempting_to_get_an_unregistered_item = () => the_container.Resolve<LogFactory>(); };

            [Observation]
            public void should_throw_an_exception()
            {
                attempting_to_get_an_unregistered_item.should_throw_an<ComponentNotFoundException>();
            }
        }
    }
}