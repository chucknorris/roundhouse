namespace roundhouse.infrastructure.filesystem
{
    using System.IO;

    /// <summary>
    /// Handles all access to the file system objects
    /// </summary>
    public interface FileSystemAccess
    {
        #region File

        /// <summary>
        /// Determines if a file exists
        /// </summary>
        /// <param name="file_path">Path to the file</param>
        /// <returns>True if there is a file already existing, otherwise false</returns>
        bool file_exists(string file_path);

        /// <summary>
        /// Creates a file
        /// </summary>
        /// <param name="file_path">Path to the file name</param>
        /// <returns>A file stream object for use after creating the file</returns>
        FileStream create_file(string file_path);

        /// <summary>
        /// Opens a file
        /// </summary>
        /// <param name="file_path">Path to the file name</param>
        /// <returns>A file stream object for use after accessing the file</returns>
        FileStream open_file_in_read_mode_from(string file_path);

        /// <summary>
        /// Returns the contents of a file
        /// </summary>
        /// <param name="file_path">Path to the file name</param>
        /// <returns>A string of the file contents</returns>
        string read_file_text(string file_path);

        /// <summary>
        /// Determines the file information given a path to an existing file
        /// </summary>
        /// <param name="file_path">Path to an existing file</param>
        /// <returns>FileInfo object</returns>
        FileInfo get_file_info_from(string file_path);

        /// <summary>
        /// Determines the FileVersion of the file passed in
        /// </summary>
        /// <param name="file_path">Relative or full path to a file</param>
        /// <returns>A string representing the FileVersion of the passed in file</returns>
        string get_file_version_from(string file_path);

        /// <summary>
        /// Copies a file from one directory to another
        /// </summary>
        /// <param name="source_file_name">Where is the file now?</param>
        /// <param name="destination_file_name">Where would you like it to go?</param>
        /// <param name="overwrite_the_existing_file">If there is an existing file already there, would you like to delete it?</param>
        void file_copy(string source_file_name, string destination_file_name, bool overwrite_the_existing_file);

        /// <summary>
        /// Copies a file from one directory to another using PInvoke
        /// </summary>
        /// <param name="source_file_name">Where is the file now?</param>
        /// <param name="destination_file_name">Where would you like it to go?</param>
        /// <param name="overwrite_the_existing_file">If there is an existing file already there, would you like to delete it?</param>
        void file_copy_unsafe(string source_file_name, string destination_file_name, bool overwrite_the_existing_file);

        /// <summary>
        /// Determines if a file is a system file
        /// </summary>
        /// <param name="file">File to check</param>
        /// <returns>True if the file has the System attribute marked, otherwise false</returns>
        bool is_system_file(FileInfo file);

        /// <summary>
        /// Determines if a file is encrypted or not
        /// </summary>
        /// <param name="file">File to check</param>
        /// <returns>True if the file has the Encrypted attribute marked, otherwise false</returns>
        bool is_encrypted_file(FileInfo file);

        /// <summary>
        /// Determines if a file has the same extension as in the list of types
        /// </summary>
        /// <param name="file_name">File to check</param>
        /// <param name="file_types">File types to check against, listed as file extensions</param>
        /// <returns>True if the file in question has a file type in the list</returns>
        bool file_in_file_types(string file_name, string[] file_types);

        /// <summary>
        /// Determines the older of the file dates, Creation Date or Modified Date
        /// </summary>
        /// <param name="file_path">File to analyze</param>
        /// <returns>The oldest date on the file</returns>
        string get_file_date(string file_path);

        /// <summary>
        /// Determines the file name from the filepath
        /// </summary>
        /// <param name="file_path">Full path to file including file name</param>
        /// <returns>Returns only the file name from the filepath</returns>
        string get_file_name_from(string file_path);

        /// <summary>
        /// Determines the file name from the filepath without the extension
        /// </summary>
        /// <param name="file_path">Full path to file including file name</param>
        /// <returns>Returns only the file name minus extensions from the filepath</returns>
        string get_file_name_without_extension_from(string file_path);

        /// <summary>
        /// Determines the file extension for a given path to a file
        /// </summary>
        /// <param name="file_path">The file to find the extension for</param>
        /// <returns>The extension of the file.</returns>
        string get_file_extension_from(string file_path);

        /// <summary>
        /// Gets a list of files inside of an existing directory
        /// </summary>
        /// <param name="directory">Path to the directory</param>
        /// <returns>A list of files inside of an existing directory</returns>
        string[] get_all_file_name_strings_in(string directory);

        #endregion

        #region Directory

        /// <summary>
        /// Determines if a directory exists
        /// </summary>
        /// <param name="directory">Path to the directory</param>
        /// <returns>True if there is a directory already existing, otherwise false</returns>
        bool directory_exists(string directory);

        /// <summary>
        /// Verifies a directory exists, if it doesn't, it creates a new directory at that location
        /// </summary>
        /// <param name="directory">Directory to verify exists</param>
        void verify_or_create_directory(string directory);

        /// <summary>
        /// Determines the directory name for a given file path. Useful when working with relative files
        /// </summary>
        /// <param name="file_path">File to get the directory name from</param>
        /// <returns>Returns only the path to the directory name</returns>
        string get_directory_name_from(string file_path);

        /// <summary>
        /// Returns a DirectoryInfo object from a string
        /// </summary>
        /// <param name="directory">Full path to the directory you want the directory information for</param>
        /// <returns>DirectoryInfo object</returns>
        DirectoryInfo get_directory_info_from(string directory);

        /// <summary>
        /// Returns a DirectoryInfo object from a string to a filepath
        /// </summary>
        /// <param name="file_path">Full path to the file you want directory information for</param>
        /// <returns>DirectoryInfo object</returns>
        DirectoryInfo get_directory_info_from_file_path(string file_path);

        /// <summary>
        /// Creates a directory
        /// </summary>
        /// <param name="directory">Path to the directory</param>
        /// <returns>A directory information object for use after creating the directory</returns>
        DirectoryInfo create_directory(string directory);

        /// <summary>
        /// Deletes a directory
        /// </summary>
        /// <param name="directory">Path to the directory</param>
        /// <param name="recursive">Would you like to delete the directories inside of this directory? Almost always true.</param>
        void delete_directory(string directory, bool recursive);

        /// <summary>
        /// Gets a list of directories inside of an existing directory
        /// </summary>
        /// <param name="directory">Directory to look for subdirectories in</param>
        /// <returns>A list of subdirectories inside of the existing directory</returns>
        string[] get_all_directory_name_strings_in(string directory);

        /// <summary>
        /// Gets a list of files inside of an existing directory
        /// </summary>
        /// <param name="directory">Path to the directory</param>
        /// <param name="pattern">Pattern or extension</param>
        /// <returns>A list of files inside of an existing directory</returns>
        string[] get_all_file_name_strings_in(string directory, string pattern);

        #endregion

        /// <summary>
        /// Determines the full path to a given directory. Useful when working with relative directories
        /// </summary>
        /// <param name="path">Where to get the full path from</param>
        /// <returns>Returns the full path to the file or directory</returns>
        string get_full_path(string path);

        /// <summary>
        /// Combines a set of paths into one path
        /// </summary>
        /// <param name="paths">Each item in order from left to right of the path</param>
        /// <returns></returns>
        string combine_paths(params string[] paths);

    }
}