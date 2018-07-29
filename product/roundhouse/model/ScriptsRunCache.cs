using System;

namespace roundhouse.model
{
    public class ScriptsRunCache : Auditable
    {
        public long id { get; set; }
        public string script_name { get; set; }
        public string text_hash { get; set; }

        //auditing
        public DateTime? entry_date { get; set; }
        public DateTime? modified_date { get; set; }
        public string entered_by { get; set; }
    }
}