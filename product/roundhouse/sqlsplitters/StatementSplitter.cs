using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace roundhouse.sqlsplitters
{
    /// <summary>
    /// Provides an interface to Split Sql strings.
    /// </summary>
    public interface StatementSplitter
    {
        /// <summary>
        /// Splits the provided sql into batches.
        /// </summary>
        /// <param name="sql_batch">The sql to split</param>
        /// <returns>Enumeration of Sql Batches</returns>
        IEnumerable<string> split(string sql);
    }
}
