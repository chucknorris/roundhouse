namespace roundhouse.migrators
{
    public interface HashGenerator
    {
        string create_hash(string sql_to_run, bool normalizeEndings);
        bool have_same_hash(string script_name, string sql_to_run, string old_text_hash);
    }
}