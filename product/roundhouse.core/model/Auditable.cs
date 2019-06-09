namespace roundhouse.model
{
    using System;

    public interface Auditable
    {
        DateTime? entry_date { get; set; }
        DateTime? modified_date { get; set; }
        string entered_by { get; set; }
    }
}