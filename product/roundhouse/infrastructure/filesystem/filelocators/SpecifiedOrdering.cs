using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace roundhouse.infrastructure.filesystem.filelocators
{
    /// <summary>
    /// Supports the use of a specific ordering in a directory, via
    /// the use of a file which specifies the order of each file, using 
    /// a relative path.
    ///
    /// Files in the directory or subdirectories that are not in the specified order file
    /// are sorted according to their relative path.
    ///
    /// The Script Ordering File:
    ///  - Lines starting with # are ignored
    ///  - Each relative file path are on it's own line, without the leading directory seperator
    /// </summary>
    public class SpecifiedOrdering : FileLocator
    {
        /// <summary>
        /// Construct a SpecifiedOrdering FileLocator.
        /// </summary>
        /// <param name="ordering_file_name">The path to the specific ordering file relative to the directory.</param>
        public SpecifiedOrdering(string ordering_file_name)
        {
            this.ordering_file_name = ordering_file_name;
        }

        private readonly string ordering_file_name;

        /// <summary>
        /// Return the files in this directory and all subdirectories,
        /// the order is as follows:
        ///  1. Files specified in the ordering file, in the order of their entry in that file
        ///  2. Files not specified in the ordering, in the culture invariant case-insensitive order
        ///     of the relative file name.
        /// </summary>
        /// <param name="directory">The directory to search</param>
        /// <param name="pattern">The file pattern you are searching for</param>
        /// <returns>The ordered list of files.</returns>
        public string[] locate_all_files_in(string directory, string pattern)
        {
            var ordering_file = Path.Combine(directory, ordering_file_name);
            // Load the specified order file and create a dictionary of the filenames and their positions.
            var specifiedOrder = File.Exists(ordering_file)
            ?   File.ReadAllLines(ordering_file)
                // skip comment lines.
                .Where(line => !line.StartsWith("#"))
            :   Enumerable.Empty<string>();

			var files = Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);
            // Sort the files according to their position in the ordering file
            // or failing that their relative file paths.
            Array.Sort(files, new Comparer(specifiedOrder, directory)); 

            return files;
        }

        /// <summary>
        /// Sorts string according to their position in the specified ordering,
        /// or by the invariant culture ignore case comparer.
        /// </summary>
        public class Comparer : IComparer<string>
        {
            public Comparer(IEnumerable<string> ordering, string directory)
            {
                this.specifiedOrder = ordering
                    .Select((relativeFileName, order) => new KeyValuePair<string, int>(relativeFileName, order))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.InvariantCultureIgnoreCase);
                this.directory = directory;
            }

            private Dictionary<string, int> specifiedOrder;
            private string directory;

            public int Compare(string lhsRaw, string rhsRaw)
            {
                // obtain the file names relative to the search directory.
                var lhs =
                  lhsRaw.Substring(directory.Length).TrimStart(
                      Path.DirectorySeparatorChar);
                var rhs =
                  rhsRaw.Substring(directory.Length).TrimStart(
                      Path.DirectorySeparatorChar);
                int lhsPosition;
                int rhsPosition;

                // Figure out whether the files are in the specified order file.
                bool lhsIsSpecified = specifiedOrder.TryGetValue(lhs, out lhsPosition);
                bool rhsIsSpecified = specifiedOrder.TryGetValue(rhs, out rhsPosition);

                if(lhsIsSpecified && rhsIsSpecified)
                {
                  // Both in specified ordering file,
                  // Order them by their location in that file.
                  return Comparer<int>.Default.Compare(lhsPosition, rhsPosition);
                } else if(lhsIsSpecified)
                {
                  // Left hand side is in the specified file,
                  // so it goes before the right hand side.
                  return -1;
                } else if (rhsIsSpecified)
                {
                  // Right hand side is in the specified file,
                  // so it goes before the left hand side.
                  return 1;
                }

                // we want to ignore the file extension when comparing
                int lhsLengthExcludingFileExtension = lhs.LastIndexOf('.');
                int rhsLengthExcludingFileExtension = rhs.LastIndexOf('.');

                if(lhsLengthExcludingFileExtension != -1)
                {
                    lhs = lhs.Substring(0, lhsLengthExcludingFileExtension);
                }

                if(rhsLengthExcludingFileExtension != -1)
                {
                    rhs = rhs.Substring(0, rhsLengthExcludingFileExtension);
                }

                // neither side is in the ordering file,
                // use the string comparer against the the relative path to
                // each file.
                return StringComparer.InvariantCultureIgnoreCase.Compare(lhs, rhs);
            }
        }

    }
}
