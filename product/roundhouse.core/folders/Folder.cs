namespace roundhouse.folders
{
    public interface Folder
    {
        string folder_name { get; set; }
        string folder_path { get; }
        string folder_full_path { get; }
    }
}