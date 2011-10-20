using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bottles.Deployment.Parsing;
using Bottles.Diagnostics;
using FubuCore.Configuration;
using HtmlTags;
using FubuCore;

namespace Bottles.Deployment.Diagnostics
{
    public class DeploymentReport
    {
        private readonly HtmlDocument _document;

        public DeploymentReport(string title)
        {
            _document = new HtmlDocument
                        {
                            Title = title
                        };

            _document.AddStyle(getCss());
            _document.AddJavaScript(getJs("jquery-1.6.1.min.js"));
            _document.AddJavaScript(getJs("sneaky.js"));
            _document.AddStyle(".header {text-indent:20px;background:" + getPngAsCssData("bullet_arrow_right.png") + " 5px 13px no-repeat; cursor:pointer;}");
            _document.AddStyle(".expanded {background:" + getPngAsCssData("bullet_arrow_down.png") + " 5px 13px no-repeat}");

            _document.Push("div").AddClass("main");

            _document.Add("h1").Text(title);
        }

        


        public void WriteDeploymentPlan(DeploymentPlan plan)
        {
            writeOptions(plan);
            writeEnvironmentSettings(plan);
            writeHostSettings(plan);
        }

        private void writeOptions(DeploymentPlan plan)
        {
            wrapInCollapsable("Options", div =>
            {

                var table = new TableTag();
                table.Id("properties");
                table.AddProperty("Written at", DateTime.Now.ToLongTimeString());
                table.AddProperty("Profile", plan.Options.ProfileName + " at " + plan.Options.ProfileFileName); // TODO -- add file name
                table.AddProperty("Recipes", plan.Recipes.Select(x => x.Name).OrderBy(x => x).Join(", "));
                table.AddProperty("Hosts", plan.Hosts.Select(x => x.Name).OrderBy(x => x).Join(", "));
                table.AddProperty("Bottles",
                                  plan.Hosts.SelectMany(host => host.BottleReferences).Select(bottle => bottle.Name).
                                      Distinct().OrderBy(x => x).Join(", "));
                div.Append(table);
            });
        }

        
        private void writeEnvironmentSettings(DeploymentPlan plan)
        {
            wrapInCollapsable("Profile / Environment Substitutions", div =>
            {
                var report = plan.GetSubstitutionDiagnosticReport();

                div.Append(writeSettings(findProvenanceRoot(plan), report));
            });
        }

        private void writeHostSettings(DeploymentPlan plan)
        {
            var provRoot = findProvenanceRoot(plan);
            wrapInCollapsable("Directive Values by Host", div =>
            {
                plan.Hosts.Each(h =>
                {
                    div.Append(writeHostSettings(provRoot, h));
                });
            });
        }

        private string findProvenanceRoot(DeploymentPlan plan)
        {
            return System.Environment.CurrentDirectory;
        }

        private void wrapInCollapsable(string title, Action<HtmlTag> stuff)
        {
            var id = Guid.NewGuid().ToString();
            var hid = "h" + id;
            _document.Add("h2")
                .AddClass("header")
                .Text(title)
                .Id(hid);

            var div = new DivTag(id);
            div.Style("display", "none");

            stuff(div);

            _document.Add(div);
        }

        private IEnumerable<HtmlTag> writeHostSettings(string provRoot, HostManifest host)
        {
            yield return new HtmlTag("h4").Text(host.Name);
            
            var settingDataSources = host.CreateDiagnosticReport();

            yield return writeSettings(provRoot, settingDataSources);
        }

        private static HtmlTag writeSettings(string provRoot ,IEnumerable<SettingDataSource> settingDataSources)
        {
            var table = new TableTag();
            table.AddClass("details");
            table.AddHeaderRow("Key", "Value", "Provenance");

            settingDataSources.Each(s =>
            {
                var prov = s.Provenance.Replace(provRoot, "");
                table.AddBodyRow(s.Key, s.Value, s.Provenance);
            });

            return table;
        }

        public void WriteLoggingSession(LoggingSession session)
        {
            wrapInCollapsable("Logs", div =>
            {
                var tag = LoggingSessionWriter.Write(session);
                tag.AddClass("details");
                div.Append(tag);
            });
        }

        public void WriteSuccessOrFail(LoggingSession session)
        {
            var tag = _document.Add("div");
            var msg = "SUCCESS";
            tag.AddClass("success");

            if(session.HasErrors())
            {
                tag.RemoveClass("success");
                tag.AddClass("failure");

                msg = "FAIL";
            }

            tag.Add("p")
                .Style("margin", "0px 0px")
                .Style("text-indent","10px")
                .Text(msg);
        }


        public HtmlDocument Document
        {
            get { return _document; }
        }

        private static string getCss()
        {
            var type = typeof(DeploymentReport);
            var stream = type.Assembly.GetManifestResourceStream(type, "diagnostics.css");
            if (stream == null) return String.Empty;
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        private static string getJs(string name)
        {
            var type = typeof(DeploymentReport);
            var stream = type.Assembly.GetManifestResourceStream(type, name);
            if (stream == null) return String.Empty;
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        private static string getPngAsCssData(string imageName)
        {
            var type = typeof(DeploymentReport);
            using(var stream = type.Assembly.GetManifestResourceStream(type, imageName))
            {
                if (stream == null) return String.Empty;
                var bytes=stream.ReadAllBytes();

                return "url(data:image/png;base64,{0})".ToFormat(Convert.ToBase64String(bytes));
            }
        }
    }

    public static class StreamExtensions
    {
        public static byte[] ReadAllBytes(this Stream stream)
        {
            var buffer = new Byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }
    }
}