using System.Collections.Generic;

namespace HtmlTags
{
    public interface ITagSource
    {
        IEnumerable<HtmlTag> AllTags();
    }
}