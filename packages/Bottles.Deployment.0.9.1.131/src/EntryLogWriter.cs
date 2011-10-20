using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bottles.Diagnostics;
using FubuCore;
using HtmlTags;

namespace Bottles.Deployment
{
    public static class EntryLogWriter
    {
        public static HtmlDocument Write(IEnumerable<LogEntry> entries, string title)
        {
            var tags = createTags(entries);

            
            return DiagnosticHtml.BuildDocument(title, tags.ToArray());
        }
        
        private static IEnumerable<HtmlTag> createTags(IEnumerable<LogEntry> entries)
        {
            foreach (LogEntry log in entries)
            {
                var text = "{0} in {1} milliseconds".ToFormat(log.Description, log.TimeInMilliseconds);
                if (!log.Success)
                {
                    text += " -- Failed!";
                }

                var headerTag = new HtmlTag("h4").Text(text).AddClass("log");

                yield return headerTag;

                if (log.TraceText.IsNotEmpty())
                {
                    var traceTag = new HtmlTag("pre").AddClass("log").Text(log.TraceText);
                    if (!log.Success)
                    {
                        traceTag.AddClass("failure");
                    }

                    yield return traceTag;
                }

                yield return new HtmlTag("hr");
            }
        }

        public static class DiagnosticHtml
        {
            public static HtmlDocument BuildDocument(string title, params HtmlTag[] tags)
            {
                string css = GetDiagnosticCss();

               
                var document = new HtmlDocument{
                    Title = title
                };

                var mainDiv = new HtmlTag("div").AddClass("main");
                mainDiv.Add("h2").Text(title);
                document.Add(mainDiv);

                mainDiv.Append(tags);

                document.AddStyle(css);

                return document;
            }

            public static string GetDiagnosticCss()
            {
                return GetResourceText(typeof(DiagnosticHtml), "diagnostics.css");
            }

            public static string GetResourceText(Type type, string filename)
            {
                var stream = type.Assembly.GetManifestResourceStream(type, filename);
                if (stream == null) return String.Empty;
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }


        }
    }
}