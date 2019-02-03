namespace roundhouse.model
{
    using System;

    public class ScriptsRunError : Auditable
    {
        public long id { get; set; }
        public string repository_path { get; set; }
        public string version { get; set; }
        public string script_name { get; set; }
        public string text_of_script { get; set; }
        public string erroneous_part_of_script { get; set; }
        public string error_message { get; set; }

        //auditing
        public DateTime? entry_date { get; set; }
        public DateTime? modified_date { get; set; }
        public string entered_by { get; set; }
    }
}