using System;
using System.Collections.Generic;
using System.Linq;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Environment
{
    [Serializable]
    public class EnvironmentRun
    {
        // folder where the app is. Mandatory!!
        public string ApplicationBase { get; set; }

        // Use to scan for an environment class if not there
        public string AssemblyName { get; set; }

        // Figure this out for yourself if not given
        public string EnvironmentClassName { get; set; }

        // Use web.config if it exists?
        public string ConfigurationFile { get; set; }

        // Can be different than ApplicationBase.  Thanks ASP.Net
        public string ApplicationDirectory { get; set; }

        public AppDomainSetup BuildAppDomainSetup()
        {
            return new AppDomainSetup{
                ApplicationBase = ApplicationBase,
                ConfigurationFile = ConfigurationFile,
                ApplicationName = "Bottles-Environment-Installation" + Guid.NewGuid(),
                ShadowCopyFiles = "true"
            };
        }

        // TODO -- harden with a better exception
        public Type FindEnvironmentType(IPackageLog log)
        {
            if (EnvironmentClassName.IsNotEmpty())
            {
                log.Trace("Environment class was specified '{0}'", EnvironmentClassName);
                return Type.GetType(EnvironmentClassName);
            }

            log.Trace("Scanning AppDomain for IEnvironment");
            var foundClass = AppDomain.CurrentDomain
                                .Load(AssemblyName)
                                .GetExportedTypes()
                                .First(x => x.IsConcreteTypeOf<IEnvironment>());

            log.Trace("Found Environment class '{0}'", foundClass);
            return foundClass;
        }

        public void AssertIsValid()
        {
            var messages = new List<string>();
            if (ApplicationBase.IsEmpty())
            {
                messages.Add("ApplicationBase must be a valid folder");
            }

            if (EnvironmentClassName.IsEmpty() && AssemblyName.IsEmpty())
            {
                messages.Add("Either EnvironmentClassName or AssemblyName must be set");
            }

            if (messages.Any())
            {
                throw new EnvironmentRunnerException(messages.Join("; "));
            }
        }
    }
}