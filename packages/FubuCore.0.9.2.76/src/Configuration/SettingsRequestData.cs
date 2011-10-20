using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Binding;

namespace FubuCore.Configuration
{
    public class SettingsRequestData : IRequestData, IKeyValues
    {
        private readonly SettingsStep _profileStep;
        private readonly SettingsStep _environmentStep;
        private readonly SettingsStep _packageStep;
        private readonly SettingsStep _coreStep;
        
        private readonly SettingsStep[] _steps;


        public SettingsRequestData(IEnumerable<SettingsData> settingData)
        {
            _profileStep = new SettingsStep(settingData.Where(x=>x.Category == SettingCategory.profile));
            _environmentStep = new SettingsStep(settingData.Where(x => x.Category == SettingCategory.environment));
            _packageStep = new SettingsStep(settingData.Where(x => x.Category == SettingCategory.package));
            _coreStep = new SettingsStep(settingData.Where(x => x.Category == SettingCategory.core));

            _steps = new []{_profileStep, _environmentStep, _packageStep, _coreStep};
        }

        public object Value(string key)
        {
            object returnValue = null;

            Value(key, o => returnValue = o);

            return (returnValue ?? string.Empty).ToString();
        }

        public bool Value(string key, Action<object> callback)
        {
            return _steps.Any(x => x.Value(key, callback));
        }

        

        public bool HasAnyValuePrefixedWith(string key)
        {
            return _steps.Any(x => x.HasAnyValuePrefixedWith(key));
        }

        public static SettingsRequestData For(params SettingsData[] data)
        {
            return new SettingsRequestData(data);
        }

        public class SettingsStep
        {
            private readonly IEnumerable<SettingsData> _settingData;

            public SettingsStep(IEnumerable<SettingsData> settingData)
            {
                _settingData = settingData;
            }

            public IEnumerable<string> AllKeys
            {
                get { return _settingData.SelectMany(data => data.AllKeys); }
            }

            public bool HasAnyValuePrefixedWith(string key)
            {
                return _settingData.Any(x => x.AllKeys.Any(k => k.StartsWith(key)));
            }

            public bool Value(string key, Action<object> callback)
            {
                var data = _settingData.FirstOrDefault(x => x.Has(key));
                if (data == null) return false;

                callback(data.Get(key));

                return true;
            }

            public SettingDataSource DiagnosticValueOf(string key)
            {
                var setting = _settingData.FirstOrDefault(x => x.Has(key));
                return setting == null ? null : new SettingDataSource(){
                    Key = key, Provenance = setting.Provenance, Value = setting.Get(key)
                };
            }
        }

        public IEnumerable<SettingDataSource> CreateDiagnosticReport()
        {
            return _steps.SelectMany(step => step.AllKeys)
                .Distinct().OrderBy(x=>x)
                .Select(diagnosticSourceForKey);
        }

        private SettingDataSource diagnosticSourceForKey(string key)
        {
            return _steps.FirstValue(x => x.DiagnosticValueOf(key));
        }

        public bool ContainsKey(string key)
        {
            return Value(key, o => { });
        }

        public string Get(string key)
        {
            return Value(key) as string;
        }

        public IEnumerable<string> GetKeys()
        {
            return _steps.SelectMany(s => s.AllKeys);
        }
    }
}