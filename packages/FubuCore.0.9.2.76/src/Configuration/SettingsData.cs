using System;
using System.Collections.Generic;
using FubuCore.Util;
using System.Linq;

namespace FubuCore.Configuration
{


    public class SettingsData
    {
        private readonly Cache<string, string> _values = new Cache<string, string>();

        public SettingsData() : this(SettingCategory.core)
        {
        }

        public SettingsData(SettingCategory category)
        {
            Category = category;
        }

        public string this[string key]
        {
            get { return _values[key]; }
            set { _values[key] = value; }
        }

        public string Provenance { get; set; }

        public SettingCategory Category { get; set; }

        public IEnumerable<string> AllKeys
        {
            get { return _values.GetAllKeys(); }
        }

        public SettingsData With(string key, string value)
        {
            _values[key] = value;
            return this;
        }

        public bool Has(string key)
        {
            return _values.Has(key);
        }

        public string Get(string key)
        {
            return _values[key];
        }

        public SettingsData SubsetPrefixedBy(string prefix)
        {
            var keys = AllKeys.Where(key => key.StartsWith(prefix));
            var subset = new SettingsData(Category){
                Provenance = Provenance
            };

            keys.Each(rawKey =>
            {
                var subsetKey = rawKey.Substring(prefix.Length);
                subset.With(subsetKey, Get(rawKey));
            });

            return subset;
        }

        public SettingsData SubsetByKey(Func<string, bool> keyFilter)
        {
            var subset = new SettingsData(Category){
                Provenance = Provenance
            };

            AllKeys.Where(keyFilter).Each(key => subset.With(key, _values[key]));

            return subset;
        }

        public void Read(string text)
        {
            var parts = text.Split('=');
            if (parts.Length != 2)
            {
                throw new ApplicationException("Invalid settings data text for '{0}'".ToFormat(text));
            }

            _values[parts[0].Trim()] = parts[1].Trim();
        }

        public static SettingsData ReadFromFile(SettingCategory category, string file)
        {
            var data = new SettingsData(category){
                Provenance = file
            };

            ReadFromFile(file, data);

            return data;
        }

        public static void ReadFromFile(string file, SettingsData data)
        {
            new FileSystem().ReadTextFile(file, text =>
            {
                if (text.IsEmpty()) return;

                data.Read(text);
            });
        }
    }
}