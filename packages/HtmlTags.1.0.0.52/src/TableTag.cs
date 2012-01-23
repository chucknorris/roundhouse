using System;
using System.Linq;

namespace HtmlTags
{
    public class TableTag : HtmlTag
    {
        private readonly HtmlTag _body;
        private readonly HtmlTag _header;
        private readonly HtmlTag _footer;

        public TableTag()
            : base("table")
        {
            _header = new HtmlTag("thead", this);
            _footer = new HtmlTag("tfoot", this).Render(false);
            _body = new HtmlTag("tbody", this);
        }

        public TableTag CaptionText(string text)
        {
            HtmlTag caption = existingCaption();
            if (caption == null)
            {
                caption = new HtmlTag("caption");
                Children.Insert(0, caption);
            }

            caption.Text(text);

            return this;
        }

        public string CaptionText()
        {
            var caption = existingCaption();
            return caption == null ? string.Empty : caption.Text();
        }

        private HtmlTag existingCaption()
        {
            return Children.FirstOrDefault(x => x.TagName() == "caption");
        }

        public TableRowTag AddHeaderRow()
        {
            return _header.Add<TableRowTag>();
        }

        public TableTag AddHeaderRow(Action<TableRowTag> configure)
        {
            configure(AddHeaderRow());

            return this;
        }

        public TableRowTag AddBodyRow()
        {
            return _body.Add<TableRowTag>();
        }

        public TableTag AddBodyRow(Action<TableRowTag> configure)
        {
            configure(AddBodyRow());
            return this;
        }

        public TableTag AddFooterRow(Action<TableRowTag> configure)
        {
            _footer.Render(true);
            configure(_footer.Add<TableRowTag>());
            return this;
        }


        public TableTag Caption(string caption)
        {
            var captionTag = existingCaption();
            if (captionTag == null)
            {
                captionTag = new HtmlTag("caption");
                Children.Insert(0, captionTag);
            }

            captionTag.Text(caption);

            return this;
        }
    }

    public class TableRowTag : HtmlTag
    {
        public TableRowTag()
            : base("tr")
        {
        }

        public HtmlTag Header(string text)
        {
            return new HtmlTag("th", this).Text(text);
        }

        public HtmlTag Header()
        {
            return new HtmlTag("th", this);
        }

        public HtmlTag Cell(string text)
        {
            return new HtmlTag("td", this).Text(text);
        }

        public HtmlTag Cell()
        {
            return new HtmlTag("td", this);
        }
    }
}