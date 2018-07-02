using System.Collections.Generic;

namespace roundhouse.tests.infrastructure.app
{
    using consoles;
    using roundhouse.infrastructure.app;
    using Should;

    public class DefaultConfigurationSpecs
    {

        public abstract class concern_for_DefaultConfiguration : TinySpec
        {
            protected ConfigurationPropertyHolder configuration;

            public override void Context()
            {
                configuration = new DefaultConfiguration();
            }
        }

        [Concern(typeof(DefaultConfiguration))]
        public class when_single_environment_present : concern_for_DefaultConfiguration
        {

            public override void Because()
            {
                configuration.EnvironmentNames.Add("Environment1");
            }

            [Observation]
            public void token_dictionary_should_have_single_environment_name()
            {

                var dict = configuration.to_token_dictionary();

#pragma warning disable CS0618 // Type or member is obsolete
                dict.TryGetValue(nameof(ConfigurationPropertyHolder.EnvironmentName), out var environment_name).ShouldBeTrue();
                environment_name.ShouldEqual("Environment1");
#pragma warning restore CS0618 // Type or member is obsolete

                dict.TryGetValue(nameof(ConfigurationPropertyHolder.EnvironmentNames), out var environment_names).ShouldBeTrue();
                environment_names.ShouldEqual("Environment1");

            }

        }

        [Concern(typeof(DefaultConfiguration))]
        public class when_multiple_environments_present : concern_for_DefaultConfiguration
        {

            public override void Because()
            {
                configuration.EnvironmentNames.Add("Environment1");
                configuration.EnvironmentNames.Add("Environment2");
            }

            [Observation]
            public void token_dictionary_should_have_multiple_environment_names()
            {

                var dict = configuration.to_token_dictionary();

#pragma warning disable CS0618 // Type or member is obsolete
                dict.TryGetValue(nameof(ConfigurationPropertyHolder.EnvironmentName), out var environment_name).ShouldBeTrue();
                environment_name.ShouldEqual("Environment1,Environment2");
#pragma warning restore CS0618 // Type or member is obsolete

                dict.TryGetValue(nameof(ConfigurationPropertyHolder.EnvironmentNames), out var environment_names).ShouldBeTrue();
                environment_names.ShouldEqual("Environment1,Environment2");

            }

        }

        [Concern(typeof(DefaultConfiguration))]
        public class when_custom_user_tokens_present : concern_for_DefaultConfiguration
        {

            public override void Because()
            {
                configuration.UserTokens = new Dictionary<string, string>
                {
                    {"UserId", "1"}
                };
            }

            [Observation]
            public void token_dictionary_should_have_multiple_environment_names()
            {

                var dict = configuration.to_token_dictionary();

                dict.TryGetValue("UserId", out var user_id).ShouldBeTrue();
                user_id.ShouldEqual("1");

            }

        }

        [Concern(typeof(DefaultConfiguration))]
        public class when_overriding_user_tokens_present : concern_for_DefaultConfiguration
        {

            public override void Because()
            {
                configuration.ServerName = "original";
                configuration.UserTokens = new Dictionary<string, string>
                {
                    {"ServerName", "overridden"}
                };
            }

            [Observation]
            public void token_dictionary_should_have_multiple_environment_names()
            {

                var dict = configuration.to_token_dictionary();

                dict.TryGetValue("ServerName", out var server_name).ShouldBeTrue();
                server_name.ShouldEqual("overridden");

            }

        }

    }
}
