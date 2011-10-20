using System.Collections.Generic;
using HtmlTags;

namespace Bottles.Deployment.Diagnostics
{
    public static class TableTagExtensions
    {
        public static void AddProperty(this TableTag table, string header, string body)
        {
            table.AddBodyRow(row =>
            {
                row.Header(header);
                row.Cell(body);
            });
        }

        public static void AddHeaderRow(this TableTag table, params string[] headers)
        {
            table.AddHeaderRow(row => headers.Each(h => row.Header(h)));
        }

        public static void AddBodyRow(this TableTag table, params string[] cells)
        {
            table.AddBodyRow(row => cells.Each(c => row.Cell(c)));
        }
    }
}