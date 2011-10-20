namespace FubuCore.Configuration
{
    public class SettingDataSource
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Provenance { get; set; }

        public bool Equals(SettingDataSource other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Key, Key) && Equals(other.Value, Value) && Equals(other.Provenance, Provenance);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (SettingDataSource)) return false;
            return Equals((SettingDataSource) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Key != null ? Key.GetHashCode() : 0);
                result = (result*397) ^ (Value != null ? Value.GetHashCode() : 0);
                result = (result*397) ^ (Provenance != null ? Provenance.GetHashCode() : 0);
                return result;
            }
        }

        public override string ToString()
        {
            return string.Format("Key: {0}, Value: {1}, Provenance: {2}", Key, Value, Provenance);
        }
    }
}