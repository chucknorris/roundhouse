using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FubuCore
{
    public static class FileHashingExtensions
    {
        public static string HashByModifiedDate(this string filename)
        {
            return filename.GetModifiedDateFileText().ToHash();
        }

        public static string GetModifiedDateFileText(this string filename)
        {
            var fullPath = filename.ToFullPath();
            return fullPath + ":" + File.GetLastWriteTime(fullPath).Ticks;
        }

        public static string HashByModifiedDate(this IEnumerable<string> files)
        {
            return files.OrderBy(x => x).Select(x => x.GetModifiedDateFileText()).Join("|").ToHash();
        }
    }
}