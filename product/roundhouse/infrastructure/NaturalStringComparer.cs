using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace roundhouse.infrastructure
{
    /// <summary>
    /// Implements a &quot;natural&quot; sorting order.  For example,
    /// &quot;1_Foo&quot; &lt; &quot;2_Foo&quot; &lt; &quot;10_Foo&quot;.
    /// </summary>
    public sealed class NaturalStringComparer : IComparer<string>
    {
        [DllImport("shlwapi", CharSet=CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string psz1, string psz2);

        int IComparer<string>.Compare(string x, string y)
        {
            return StrCmpLogicalW(x, y);
        }
    }
}
