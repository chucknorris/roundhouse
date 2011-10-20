using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using FubuCore.Util;
using FubuMVC.Core;

namespace FubuCore.Configuration
{
    public static class XmlSettingsParser
    {
        public static SettingsData Parse(string file)
        {
            try
            {
                var document = new XmlDocument();
                document.Load(file);

                var data = Parse(document.DocumentElement);
                data.Provenance = file;

                return data;
            }
            catch (Exception ex)
            {
                throw new FubuException(2203, ex, "Tried to parse the file '{0}' but there was a problem", file);
            }
        }

        public static SettingsData Parse(XmlElement element)
        {
            var category = (SettingCategory)(element.HasAttribute("category")
                                               ? Enum.Parse(typeof(SettingCategory), element.GetAttribute("category"))
                                               : SettingCategory.core);

            var data = new SettingsData(category);

            element.SelectNodes("add").OfType<XmlElement>().Each(elem =>
            {
                var key = elem.GetAttribute("key");
                var value = elem.GetAttribute("value");
                data[key] = value;
            });

            return data;
        }

        public static void Write(SettingsData data, string filename)
        {
            var document = new XmlDocument();
            var root = document.WithRoot("Settings");
            root.SetAttribute("category", data.Category.ToString());

            data.AllKeys.Each(key =>
            {
                root.AddElement("add").WithAtt("key", key).WithAtt("value", data[key]);
            });

            document.Save(filename);
        }


    }

    public static class XmlExtensions
    {
        public static XmlElement With(this XmlElement node, Action<XmlElement> action)
        {
            action(node);
            return node;
        }

        public static XmlDocument FromFile(this XmlDocument document, string fileName)
        {
            document.Load(fileName);
            return document;
        }

        public static XmlElement WithRoot(this XmlDocument document, string elementName)
        {
            XmlElement element = document.CreateElement(elementName);
            document.AppendChild(element);

            return element;
        }


        public static XmlDocument WithXmlText(this XmlDocument document, string xml)
        {
            document.LoadXml(xml);

            return document;
        }

        public static XmlElement WithFormattedText(this XmlElement element, string text)
        {
            XmlCDataSection section = element.OwnerDocument.CreateCDataSection(text);
            element.AppendChild(section);

            return element;
        }

        public static XmlElement AddElement(this XmlNode element, string name)
        {
            XmlElement child = element.OwnerDocument.CreateElement(name);
            element.AppendChild(child);

            return child;
        }

        public static void AddComment(this XmlNode element, string text)
        {
            XmlComment comment = element.OwnerDocument.CreateComment(text);
            element.AppendChild(comment);
        }

        public static XmlElement AddElement(this XmlNode element, string name, Action<XmlElement> action)
        {
            XmlElement child = element.OwnerDocument.CreateElement(name);
            element.AppendChild(child);

            action(child);

            return child;
        }

        public static XmlElement WithInnerText(this XmlElement node, string text)
        {
            node.InnerText = text;
            return node;
        }

        public static XmlElement WithAtt(this XmlElement element, string key, string attValue)
        {
            element.SetAttribute(key, attValue);
            return element;
        }

        public static XmlElement WithAttributes(this XmlElement element, string text)
        {
            string[] atts = text.Split(',');
            foreach (string att in atts)
            {
                string[] parts = att.Split(':');

                element.WithAtt(parts[0].Trim(), parts[1].Trim());
            }

            return element;
        }

        public static void SetAttributeOnChild(this XmlElement element, string childName, string attName,
                                               string attValue)
        {
            XmlElement childElement = element[childName];
            if (childElement == null)
            {
                childElement = element.AddElement(childName);
            }

            childElement.SetAttribute(attName, attValue);
        }

        public static XmlElement WithProperties(this XmlElement element, Dictionary<string, string> properties)
        {
            foreach (var pair in properties)
            {
                element.SetAttribute(pair.Key, pair.Value);
            }

            return element;
        }

        public static XmlElement WithProperties(this XmlElement element, Cache<string, string> properties)
        {
            properties.Each((k, v) => element.SetAttribute(k, v));

            return element;
        }

        public static void ForEachElement(this XmlNode node, Action<XmlElement> action)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var element = child as XmlElement;
                if (element != null)
                {
                    action(element);
                }
            }
        }
    }
}