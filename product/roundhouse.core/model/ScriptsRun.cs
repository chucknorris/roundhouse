namespace roundhouse.model
{
    using System;

    public class ScriptsRun : Auditable
    {
        public long id { get; set; }
        public long version_id { get; set; }
        public string script_name { get; set; }
        public string text_of_script { get; set; }
        public string text_hash { get; set; }
        public bool one_time_script { get; set; }

        //auditing
        public DateTime? entry_date { get; set; }
        public DateTime? modified_date { get; set; }
        public string entered_by { get; set; }
    }
}