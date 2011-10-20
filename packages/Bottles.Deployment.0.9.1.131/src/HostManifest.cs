using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Bottles.Deployment.Runtime;
using FubuCore.Binding;
using FubuCore.Configuration;
using System.Linq;
using FubuCore;
using FubuCore.Reflection;

namespace Bottles.Deployment
{
    public class HostManifest
    {
        private readonly IList<BottleReference> _bottles = new List<BottleReference>();
        private readonly IList<SettingsData> _data = new List<SettingsData>();


        public HostManifest(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    
        public void RegisterBottle(BottleReference reference)
        {
            _bottles.Add(reference);
        }

        public IEnumerable<BottleReference> BottleReferences
        {
            get { return _bottles; }
        }

        public void RegisterSettings(SettingsData data)
        {
            _data.Add(data);
        }

        public void RegisterValue<T>(Expression<Func<T, object>> expression, object value) where T : IDirective
        {
            var key = "{0}.{1}".ToFormat(typeof (T).Name, expression.ToAccessor().PropertyNames.Join("."));
            var data = new SettingsData(SettingCategory.core).With(key, value.ToString());
            
            RegisterSettings(data);
        }

        public void Append(HostManifest otherHost)
        {
            _bottles.Fill(otherHost._bottles);
            _data.AddRange(otherHost._data);
        }

        /// <summary>
        /// This is only used for testing
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SettingsData> AllSettingsData()
        {
            return _data;
        }

        public void RegisterBottles(IEnumerable<BottleReference> references)
        {
            _bottles.AddRange(references);
        }


        public IEnumerable<string> UniqueDirectiveNames()
        {
            return _data.SelectMany(x => x.AllKeys)
                .Select(x => x.Split('.')
                .First())
                .Distinct();
        }



        public override string ToString()
        {
            return Name;
        }

        public bool HasBottle(string bottle)
        {
            return _bottles.Any(br => br.Name.Equals(bottle));
        }

        public IEnumerable<SettingDataSource> CreateDiagnosticReport()
        {
            return SettingsProvider.For(AllSettingsData().ToArray()).CreateDiagnosticReport();
        }

        public T GetDirective<T>() where T : class, new()
        {
            return SettingsProvider.For(AllSettingsData().ToArray()).SettingsFor<T>();
        }

        private readonly IList<IDirective> _directives = new List<IDirective>();
        public void BuildDirectives(ISettingsProvider provider, IDirectiveTypeRegistry typeRegistry)
        {
            var directives = UniqueDirectiveNames().Select(name =>
            {
                var type = typeRegistry.DirectiveTypeFor(name);

                return (IDirective)provider.SettingsFor(type);
            });

            _directives.Clear();
            _directives.AddRange(directives);
        }

        public virtual IEnumerable<IDirective> Directives
        {
            get { return _directives; }
        }
    }
}