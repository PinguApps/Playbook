// snippet:complete
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FernPosters;

[HtmlTargetElement("img", Attributes = "fern-poster,asp-append-version,src", TagStructure = TagStructure.WithoutEndTag)]
public sealed class FernImageTagHelper : ImageTagHelper
{
    // Obsolete construction retained only to reproduce the .NET 10 migration input.
#pragma warning disable CS0618, ASP5001
    public FernImageTagHelper(IWebHostEnvironment environment, TagHelperMemoryCacheProvider cache,
        IFileVersionProvider files, HtmlEncoder encoder, IUrlHelperFactory urls)
        : base(environment, cache, files, encoder, urls)
    {
        // Historical extension read these properties; neither controls base image processing.
        _ = HostingEnvironment;
        _ = Cache;
    }
#pragma warning restore CS0618, ASP5001

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);
        output.Attributes.RemoveAll("fern-poster");
        output.Attributes.SetAttribute("data-collection", "Fern & Moss");
    }
}
// endsnippet:complete
