namespace HtmlTags
{
    public class CheckboxTag : HtmlTag
    {
        public CheckboxTag(bool isChecked)
            : base("input")
        {
            Attr("type", "checkbox");
            if (isChecked)
            {
                Attr("checked", "true");
            }
        }
    }
}