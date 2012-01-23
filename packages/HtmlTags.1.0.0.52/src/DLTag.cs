using System;

namespace HtmlTags
{
    public class DLTag : HtmlTag
    {
        public DLTag()
            : base("dl")
        {
        }

        public DLTag(Action<DLTag> configure)
            : this()
        {
            configure(this);
        }

        public DLTag AddDefinition(string term, string definition)
        {
            Add("dt").Text(term);
            Add("dd").Text(definition);

            return this;
        }
    }
}