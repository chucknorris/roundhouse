namespace HtmlTags.Extended
{
    namespace Attributes
    {
        public static class AttributesExtensions
        {
            public static HtmlTag UnEncoded(this HtmlTag tag)
            {
                tag.Encoded(false);
                return tag;
            }

            public static HtmlTag Value(this HtmlTag tag, object value)
            {
                return tag.Attr("value", value);
            }

            public static T Name<T>(this T tag, string name) where T : HtmlTag
            {
                tag.Attr("name", name);
                return tag;
            }


            public static T MultilineMode<T>(this T tag) where T : HtmlTag
            {
                if (tag.HasAttr("value"))
                {
                    tag.Text(tag.Attr("value"));
                    tag.RemoveAttr("value");
                }
                tag.TagName("textarea");
                return tag;
            }

            public static T NoAutoComplete<T>(this T tag) where T : HtmlTag
            {
                tag.Attr("autocomplete", "off");
                return tag;
            }

            public static T PasswordMode<T>(this T tag) where T : HtmlTag
            {
                tag.TagName("input").Attr("type", "password");
                tag.NoAutoComplete();
                return tag;
            }

            public static T FileUploadMode<T>(this T tag) where T : HtmlTag
            {
                tag.Attr("type", "file");
                tag.NoClosingTag();
                return tag;
            }

            public static T HideUnless<T>(this T tag, bool shouldDisplay) where T : HtmlTag
            {
                if (!shouldDisplay)
                {
                    tag.Style("display", "none");
                }

                return tag;
            }

        }
    }
}