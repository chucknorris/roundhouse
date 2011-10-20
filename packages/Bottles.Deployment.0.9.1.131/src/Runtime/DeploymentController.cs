using System;
using System.Collections.Generic;
using Bottles.Deployment.Diagnostics;
using Bottles.Deployment.Parsing;
using Bottles.Deployment.Runtime.Content;
using Bottles.Diagnostics;
using System.Linq;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Runtime
{
    public class DeploymentController : IDeploymentController
    {
        private readonly IBottleRepository _bottles;
        private readonly IFileSystem _system;
        private readonly IDirectiveRunnerFactory _factory;
        private readonly IProfileReader _reader;
        private readonly IDiagnosticsReporter _reporter;

        public DeploymentController(IProfileReader reader, IDiagnosticsReporter reporter, IDirectiveRunnerFactory factory, IBottleRepository bottles, IFileSystem system)
        {
            _reader = reader;
            _reporter = reporter;
            _factory = factory;
            _bottles = bottles;
            _system = system;
        }

        // TODO -- end to end tests on this monster -- including failure cases
        public void Deploy(DeploymentOptions options)
        {
            // need to log inside of reader
            var plan = BuildPlan(options);

            plan.AssertAllRequiredValuesAreFilled();
            
            try
            {
                var runners = _factory.BuildRunners(plan);

                int totalCount = runners.Sum(x => x.InitializerCount + x.DeployerCount + x.FinalizerCount);
                StartSteps(totalCount, "Running all directives");

                //LOG: running initializers
                runners.Each(x => x.InitializeDeployment());

                //LOG: running deployers
                runners.Each(x => x.Deploy());

                //LOG: running finalizer
                runners.Each(x => x.FinalizeDeployment());
            }
            finally
            {
                _reporter.WriteReport(options, plan);
                _system.DeleteDirectory(plan.Settings.StagingDirectory);
            }

            
        }

        public DeploymentPlan BuildPlan(DeploymentOptions options)
        {
            var plan = _reader.Read(options);

            plan.WriteToConsole();

            _bottles.AssertAllBottlesExist(plan.BottleNames());
            return plan;
        }

        private static void StartSteps(int totalCount, string header)
        {
            _totalSteps = totalCount;
            _currentStep = 1;

            ConsoleWriter.Write(ConsoleColor.White, header);
        }

        public static void RunningStep(string format, params object[] parameters)
        {
            var length = _totalSteps.ToString().Length;
            var count = "  - {0} / {1}:  ".ToFormat(_currentStep.ToString().PadLeft(length, ' '), _totalSteps);
            ConsoleWriter.Write(ConsoleColor.Gray, count + format.ToFormat(parameters));

            _currentStep++;
        }

        private static int _totalSteps = 0;
        private static int _currentStep = 0;
    }
}