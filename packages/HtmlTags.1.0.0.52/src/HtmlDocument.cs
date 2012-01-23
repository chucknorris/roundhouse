using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace HtmlTags
{
    public class HtmlDocument
    {
        private readonly List<Func<string, string>> _alterations = new List<Func<string, string>>();
        private readonly HtmlTag _body;

        private readonly Stack<HtmlTag> _currentStack = new Stack<HtmlTag>();
        private readonly HtmlTag _head;
        private readonly HtmlTag _title;

        public HtmlDocument()
        {
            RootTag = new HtmlTag("html");
            DocType = "<!DOCTYPE html>";
            _head = RootTag.Add("head");
            _title = _head.Add("title");
            _body = RootTag.Add("body");
            Last = _body;
        }

        public string DocType { get; set; }
        public HtmlTag RootTag { get; private set; }
        public HtmlTag Head { get { return _head; } }
        public HtmlTag Body { get { return _body; } }
        public string Title { get { return _title.Text(); } set { _title.Text(value); } }

        public HtmlTag Current { get { return _currentStack.Any() ? _currentStack.Peek() : _body; } }
        public HtmlTag Last { get; private set; }
        public Action<string, string> FileWriter = writeToFile;
        public Action<string> FileOpener = openFile;

        public void WriteToFile(string fileName)
        {
            FileWriter(fileName, ToString());
        }

        public void OpenInBrowser()
        {
            var filename = getPath();
            WriteToFile(filename);
            FileOpener(filename);
        }

        protected virtual string getPath()
        {
            return Path.GetTempFileName() + ".htm";
        }

        private static void writeToFile(string fileName, string fileContents)
        {
            ensureFolderExists(fileName);

            using (var writer = new StreamWriter(fileName))
            {
                writer.WriteLine(fileContents);
            }
        }

        private static void ensureFolderExists(string fileName)
        {
            var folder = Path.GetDirectoryName(fileName);

            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        private static void openFile(string fileName)
        {
            Process.Start(fileName);
        }

        public HtmlTag Add(string tagName)
        {
            Last = Current.Add(tagName);
            return Last;
        }

        public void Add(HtmlTag tag)
        {
            Last = tag;
            Current.Append(tag);
        }

        public void Add(ITagSource source)
        {
            source.AllTags().Each(Add);
        }

        public HtmlTag Push(string tagName)
        {
            var tag = Add(tagName);
            _currentStack.Push(tag);

            return tag;
        }

        public void Push(HtmlTag tag)
        {
            Current.Append(tag);
            _currentStack.Push(tag);
        }

        public void PushWithoutAttaching(HtmlTag tag)
        {
            _currentStack.Push(tag);
        }

        public void Pop()
        {
            if (_currentStack.Any())
            {
                _currentStack.Pop();
            }
        }

        private string substitute(string value)
        {
            foreach (var alteration in _alterations)
            {
                value = alteration(value);
            }

            return value;
        }

        public override string ToString()
        {
            var value = DocType + Environment.NewLine + RootTag;
            return substitute(value);
        }

        public HtmlTag AddStyle(string styling)
        {
            var key = Guid.NewGuid().ToString();
            _alterations.Add(html => html.Replace(key, styling));
            return _head.Add("style").Text(key);
        }

        public HtmlTag AddJavaScript(string javascript)
        {
            return AddScript("text/javascript", javascript);
        }

        public HtmlTag AddScript(string scriptType, string scriptContents)
        {
            var key = Guid.NewGuid().ToString();
            _alterations.Add(html => html.Replace(key, Environment.NewLine + scriptContents + Environment.NewLine));
            return _head.Add("script").Attr("type", scriptType).Text(key);
        }

        public HtmlTag ReferenceJavaScriptFile(string path)
        {
            return ReferenceScriptFile("text/javascript", path);
        }

        public HtmlTag ReferenceScriptFile(string scriptType, string path)
        {
            return _head.Add("script").Attr("type", scriptType).Attr("src", path);
        }

        public HtmlTag ReferenceStyle(string path)
        {
            return _head.Add("link")
                .Attr("media", "screen")
                .Attr("href", path)
                .Attr("type", "text/css")
                .Attr("rel", "stylesheet");
        }
    }
}