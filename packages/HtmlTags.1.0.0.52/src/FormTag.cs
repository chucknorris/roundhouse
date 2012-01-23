namespace HtmlTags
{
    public class FormTag : HtmlTag
    {
        public FormTag(string url) : this()
        {
            Action(url);
        }

        public FormTag() : base("form")
        {
            NoClosingTag();
            Id("mainForm");
            Method("post");
        }

        public FormTag Method(string httpMethod)
        {
            Attr("method", httpMethod);
            return this;
        }

        public FormTag Action(string url)
        {
            Attr("action", url);
            return this;
        }
    }
}