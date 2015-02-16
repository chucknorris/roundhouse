using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace roundhouse.infrastructure.filesystem.filelocators
{
    /// <summary>
    /// The traverse files option.
    /// </summary>
    public class Traverse : FileLocator
    {
        /// <summary>
        /// Returns the files in the provided directory first, in sorted order based on their FileName.
        /// Then each subdirectory, with the each subdirectory files, in sorted order based on their FileName.
        /// </summary>
        /// <example>
        ///     It will return like this (files are only sorted local to their directory):
        ///     b.txt
        ///     d.txt
        ///     subdir\a.txt
        ///     subdir\c.txt
        /// </example>
        /// <param name="directory">The directory to find files in</param>
        /// <param name="pattern">The wildcard pattern for those files</param>
        /// <returns>An ordered list of files in this directory and it's subdirectories</returns>
        public string[] locate_all_files_in(string directory, string pattern)
        {
            var files = new List<string>();

            // find all the files, sort them add to our list.
            files.AddRange(Directory.GetFiles(directory, pattern).OrderBy(Path.GetFileName));

            // find all the subdirectories.
            foreach (var subDir in Directory.GetDirectories(directory))
            {
                // find all the files in the subdirectories.
                files.AddRange(locate_all_files_in(subDir, pattern));
            }

            return files.ToArray();
        }
    }
}
