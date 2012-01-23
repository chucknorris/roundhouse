using System;
using System.Linq;

namespace HtmlTags
{
    public class SelectTag : HtmlTag
    {
        private const string SelectedAttributeKey = "selected";

        public SelectTag()
            : base("select")
        {
        }

        public SelectTag(Action<SelectTag> configure) : this()
        {
            configure(this);
        }

        public SelectTag TopOption(string display, object value, Action<HtmlTag> optionAction)
        {
            var option = TopOption(display, value);
            if(optionAction != null ) optionAction(option);
            return this;
        }

        public HtmlTag TopOption(string display, object value)
        {
            var option = MakeOption(display, value);
            InsertFirst(option);
            return option;
        }

        public HtmlTag Option(string display, object value)
        {
            var option = MakeOption(display, value);
            Append(option);
            return option;
        }

        public SelectTag DefaultOption(string display)
        {
            var option = TopOption(display, "");
            MarkOptionAsSelected(option);

            return this;
        }

        private static HtmlTag MakeOption(string display, object value)
        {
            return new HtmlTag("option").Text(display).Attr("value", value);
        }

        public void SelectByValue(object value)
        {
            var child = Children.FirstOrDefault(x => x.Attr("value").Equals(value));

            if (child != null)
            {
                MarkOptionAsSelected(child);
            }
        }

        private void MarkOptionAsSelected(HtmlTag optionTag)
        {
            var prevSelected = Children.FirstOrDefault(x => x.HasAttr(SelectedAttributeKey));

            if (prevSelected != null)
            {
                prevSelected.RemoveAttr(SelectedAttributeKey);
            }

            optionTag.Attr(SelectedAttributeKey, SelectedAttributeKey);
        }
    }
}