namespace HtmlTags
{
    public class TextboxTag : HtmlTag
    {
        public TextboxTag()
            : base("input")
        {
            Attr("type", "text");
        }

        public TextboxTag(string name, string value) : this()
        {
            Attr("name", name);
            Attr("value", value);
        }
    }
}