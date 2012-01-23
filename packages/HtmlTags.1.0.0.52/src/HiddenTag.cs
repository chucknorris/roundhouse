using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HtmlTags
{
    public class HiddenTag : HtmlTag
    {
        public HiddenTag()
            : base("input")
        {
            Attr("type", "hidden");
        }
    }
}
