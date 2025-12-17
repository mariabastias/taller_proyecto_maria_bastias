using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace TruequeTextil.Shared.Components.UI;

public partial class Button
{
    [Parameter]
    public string Class { get; set; } = "";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string Variant { get; set; } = "primary";

    [Parameter]
    public string Size { get; set; } = "medium";

    [Parameter]
    public bool IsDisabled { get; set; } = false;

    [Parameter]
    public bool IsLoading { get; set; } = false;

    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object?>? AdditionalAttributes { get; set; }

    private string VariantClasses => Variant switch
    {
        "primary" => "btn btn-primary",
        "secondary" => "btn btn-secondary",
        "outline" => "btn btn-outline-primary",
        "ghost" => "btn btn-link text-stone-600",
        "danger" => "btn btn-danger",
        _ => "btn btn-primary"
    };

    private string SizeClasses => Size switch
    {
        "small" => "btn-sm",
        "medium" => "",
        "large" => "btn-lg",
        _ => ""
    };

    private string AllClasses => string.Join(" ", new[]
    {
        VariantClasses,
        SizeClasses,
        Class
    }.Where(c => !string.IsNullOrEmpty(c)));

    private async Task HandleClick(MouseEventArgs args)
    {
        if (!IsDisabled && !IsLoading && OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync(args);
        }
    }
}
