using Markdig;
using Microsoft.AspNetCore.Html;

namespace Aiursoft.EmployeeCenter.Services;

public static class MarkdownService
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    public static HtmlString RenderMarkdown(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return new HtmlString(string.Empty);
        }

        var html = Markdown.ToHtml(markdown, Pipeline);
        return new HtmlString(html);
    }
}
