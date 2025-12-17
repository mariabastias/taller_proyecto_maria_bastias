using Microsoft.AspNetCore.Components;
using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Services;

namespace TruequeTextil.Shared.Components;

public partial class RegionDropdown : ComponentBase
{
    [Inject] private RegionService RegionService { get; set; } = null!;

    private List<Region> Regiones { get; set; } = new();
    private bool IsLoading = true;

    [Parameter] public int SelectedRegionId { get; set; }
    [Parameter] public EventCallback<int> SelectedRegionIdChanged { get; set; }

    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        Regiones = await RegionService.GetAllRegionesAsync();
        IsLoading = false;
        StateHasChanged();
    }
}
