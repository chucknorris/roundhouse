namespace roundhouse.model
{
    public class ScriptsRun : ScriptsRunCache
    {
        public long version_id { get; set; }
        public string text_of_script { get; set; }
        public bool one_time_script { get; set; }
    }
}