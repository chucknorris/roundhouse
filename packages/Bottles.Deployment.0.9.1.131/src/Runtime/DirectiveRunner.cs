using System;
using System.Collections.Generic;
using Bottles.Deployment.Diagnostics;
using Bottles.Diagnostics;
using System.Linq;
using FubuCore;

namespace Bottles.Deployment.Runtime
{
    public interface IDirectiveRunner
    {
        void Attach(HostManifest host, IDirective directive);
        void InitializeDeployment();
        void Deploy();
        void FinalizeDeployment();
        int InitializerCount { get; }
        int DeployerCount { get; }
        int FinalizerCount { get; }
    }

    public class DirectiveRunner<T> : IDirectiveRunner where T : IDirective
    {
        private readonly IDeploymentDiagnostics _diagnostics;
        private readonly IEnumerable<IDeployer<T>> _deployers;
        private readonly IEnumerable<IInitializer<T>> _initializers;
        private readonly IEnumerable<IFinalizer<T>> _finalizers;
        private HostManifest _host;
        private T _directive;

        public DirectiveRunner(IDeploymentDiagnostics diagnostics, IEnumerable<IDeployer<T>> deployers, IEnumerable<IInitializer<T>> initializers, IEnumerable<IFinalizer<T>> finalizers)
        {
            _diagnostics = diagnostics;
            _deployers = deployers;
            _initializers = initializers;
            _finalizers = finalizers;
        }

        public void Attach(HostManifest host, IDirective directive)
        {
            _host = host;
            _directive = (T)directive;

            _diagnostics.LogDirective(host, directive);
        }

        public void InitializeDeployment()
        {
            runActions(_initializers);
        }

        public void Deploy()
        {
            runActions(_deployers);
        }

        public void FinalizeDeployment()
        {
            runActions(_finalizers);
        }

        public IEnumerable<IDeployer<T>> Deployers
        {
            get { return _deployers; }
        }

        public IEnumerable<IInitializer<T>> Initializers
        {
            get { return _initializers; }
        }

        public IEnumerable<IFinalizer<T>> Finalizers
        {
            get { return _finalizers; }
        }

        public int InitializerCount
        {
            get
            {
                return _initializers.Count();
            }
        }

        public int DeployerCount
        {
            get
            {
                return _deployers.Count();
            }
        }

        public int FinalizerCount
        {
            get
            {
                return _finalizers.Count();
            }
        }

        private void runActions(IEnumerable<IDeploymentAction<T>> actions)
        {
            actions.Each(action =>
            {
                var description = action.GetDescription(_directive);
                var log = _diagnostics.LogAction(_host, _directive, action, description);
                
                // TODO -- maybe combine this
                LogWriter.WithLog(log, () =>
                {
                    //this is swallowing the exception
                    log.Execute(() => action.Execute(_directive, _host, log));
                    if(!log.Success)
                    {
                        var msg = "Bailing on deployment because '{0}' was not successful".ToFormat(description);
                        log.Trace(msg);
                        throw new DeploymentException(msg);
                    }
                });
            });
        }
    }
}