using Microsoft.AspNetCore.Components;
using TruequeTextil.Shared.Models;
using TruequeTextil.Shared.Services;

namespace TruequeTextil.Shared.Components;

public partial class ComunaDropdown : ComponentBase
{
    [Inject] private ComunaService ComunaService { get; set; } = null!;

    private List<Comuna> Comunas { get; set; } = new();
    private bool IsLoading = false;

    [Parameter] public int SelectedRegionId { get; set; }
    [Parameter] public int SelectedComunaId { get; set; }
    [Parameter] public EventCallback<int> SelectedComunaIdChanged { get; set; }

    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

    private int _previousRegionId;

    protected override async Task OnInitializedAsync()
    {
        _previousRegionId = SelectedRegionId;
        if (SelectedRegionId != 0)
        {
            await LoadComunasAsync();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (SelectedRegionId != _previousRegionId)
        {
            _previousRegionId = SelectedRegionId;
            SelectedComunaId = 0;
            await SelectedComunaIdChanged.InvokeAsync(0);
            if (SelectedRegionId != 0)
            {
                await LoadComunasAsync();
            }
            else
            {
                Comunas.Clear();
                StateHasChanged();
            }
        }
        else if (SelectedComunaId != 0 && !Comunas.Any(c => c.ComunaId == SelectedComunaId))
        {
            SelectedComunaId = 0;
            await SelectedComunaIdChanged.InvokeAsync(0);
        }
    }

    private async Task LoadComunasAsync()
    {
        IsLoading = true;
        StateHasChanged();
        Comunas = await ComunaService.GetComunasByRegionIdAsync(SelectedRegionId);
        IsLoading = false;
        StateHasChanged();
    }
}
