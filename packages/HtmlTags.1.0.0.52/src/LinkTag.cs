namespace HtmlTags
{
    public class LinkTag : HtmlTag
    {
        public LinkTag(string text, string url, params string[] classes)
            : base("a")
        {
            Text(text);
            Attr("href", url);
            classes.Each(x => AddClass(x));
        }
    }
}