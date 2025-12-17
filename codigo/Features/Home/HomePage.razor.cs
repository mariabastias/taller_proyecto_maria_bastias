using Microsoft.AspNetCore.Components;

namespace TruequeTextil.Features.Home;

public partial class HomePage : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private void NavigateToLogin()
    {
        NavigationManager.NavigateTo("/login");
    }

    private void NavigateToRegistro()
    {
        NavigationManager.NavigateTo("/registro");
    }
}
