using System.Collections.Generic;

namespace FubuCore.Configuration
{
    public interface ISettingsSource
    {
        IEnumerable<SettingsData> FindSettingData();
    }

    public class SettingsSource : ISettingsSource
    {
        private readonly IList<SettingsData> _settings = new List<SettingsData>();

        public SettingsSource(IEnumerable<SettingsData> settings)
        {
            _settings.AddRange(settings);
        }

        public IEnumerable<SettingsData> FindSettingData()
        {
            return _settings;
        }

        public void Add(SettingsData data)
        {
            _settings.Add(data);
        }

        public static SettingsSource For(params SettingsData[] data)
        {
            return new SettingsSource(data);
        }
    }
}