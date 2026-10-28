// snippet:complete
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FernPosters;

[HtmlTargetElement("img", Attributes = "fern-poster,asp-append-version,src", TagStructure = TagStructure.WithoutEndTag)]
public sealed class FernImageTagHelper : ImageTagHelper
{
    public FernImageTagHelper(IFileVersionProvider files, HtmlEncoder encoder, IUrlHelperFactory urls)
        : base(files, encoder, urls)
    {
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);
        output.Attributes.RemoveAll("fern-poster");
        output.Attributes.SetAttribute("data-collection", "Fern & Moss");
    }
}
// endsnippet:complete
