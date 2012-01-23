namespace HtmlTags
{
    public class DivTag : HtmlTag
    {
        public DivTag(string id)
            : base("div")
        {
            Id(id);
        }
    }
}