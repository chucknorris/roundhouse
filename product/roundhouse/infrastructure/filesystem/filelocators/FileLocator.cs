namespace roundhouse.infrastructure.filesystem.filelocators
{
    /// <summary>
    /// Interface which abstracts the method for retrieving files
    /// from a directory.
    /// </summary>
    public interface FileLocator
    {
        /// <summary>
        /// Return all the files in the given directory. 
        /// The files are returned in an implementation dependent order.
        /// </summary>
        /// <param name="directory">The full path to the directory</param>
        /// <param name="pattern">The file name pattern</param>
        /// <returns>All the files in the directory and all subdirectories, in an implementation defined order.</returns>
        string[] locate_all_files_in(string directory, string pattern);
    }
}
