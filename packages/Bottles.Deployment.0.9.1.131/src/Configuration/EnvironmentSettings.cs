using System.Collections.Generic;
using FubuCore.Configuration;

namespace Bottles.Deployment.Configuration
{
    public class EnvironmentSettings : ProfileBase
    {
        public static readonly string EnvironmentSettingsFileName = "environment.settings";
        public static readonly string ROOT = "root";

        public EnvironmentSettings() : base(SettingCategory.environment, "Environment settings")
        {
        }

        public object GetKey(string key)
        {
            IEnumerable<ISettingsSource> sources = new[]{new SettingsSource(new []{Data})};
            var sp = new SettingsProvider(null, sources);

            var result = sp.SettingFor(key);

            return result;
        }
        public static EnvironmentSettings ReadFrom(string environmentFile)
        {
            var environment = new EnvironmentSettings();
            SettingsData.ReadFromFile(environmentFile, environment.Data);

            return environment;
        }
    }
}