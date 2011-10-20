using FubuCore;
using HtmlTags;

namespace Bottles.Diagnostics
{
    public static class LoggingSessionWriter
    {
        public static TableTag Write(LoggingSession session)
        {
            var table = new TableTag();
            table.AddHeaderRow(r =>
            {
                r.Header("Type");
                r.Header("Description");
                r.Header("Provenance");
                r.Header("Timing");
            });

            session.EachLog((target, log) =>
            {
                table.AddBodyRow(row =>
                {
                    row.Cell(PackagingDiagnostics.GetTypeName(target));
                    row.Cell(target.ToString());
                    row.Cell(log.Provenance);
                    row.Cell(log.TimeInMilliseconds.ToString()).AddClass("execution-time");
                });

                if (log.FullTraceText().IsNotEmpty())
                {
                    table.AddBodyRow(row =>
                    {
                        row.Cell().Attr("colspan", "4").Add("pre").AddClass("log").Text(log.FullTraceText());
                        if (!log.Success)
                        {
                            row.AddClass("failure");
                        }
                    });
                }
            });

            return table;
        }
    }
}