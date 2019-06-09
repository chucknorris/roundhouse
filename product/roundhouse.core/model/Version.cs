namespace roundhouse.model
{
    using System;

    public class Version : Auditable
    {
        public long id { get; set; }
        public string repository_path { get; set; }
        public string version { get; set; }

        //auditing
        public DateTime? entry_date { get; set; }
        public DateTime? modified_date { get; set; }
        public string entered_by { get; set; }
    }
}