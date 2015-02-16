using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace roundhouse.infrastructure.filesystem.filelocators
{
    /// <summary>
    /// The recurse files option.
    /// </summary>
    public class Recurse : FileLocator
    {
        /// <summary>
        /// Returns all files in this directory and it's subdirectories, globally sorted using the 
        /// filename.
        /// </summary>
        /// <example>
        ///     It will return like this:
        ///     subdir\a.txt
        ///     b.txt
        ///     subdir\c.txt
        ///     d.txt
        ///     
        /// </example>
        /// <param name="directory">The directory to search in</param>
        /// <param name="pattern">The filename pattern</param>
        /// <returns>A globally sorted list of files.</returns>
        public string[] locate_all_files_in(string directory, string pattern)
        {
			string[] returnList = Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);
			return returnList.OrderBy(Path.GetFileName).ToArray();
        }
    }
}
