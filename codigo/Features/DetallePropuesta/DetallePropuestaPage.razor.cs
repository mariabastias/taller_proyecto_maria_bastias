using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TruequeTextil.Features.DetallePropuesta.Interfaces;
using TruequeTextil.Shared.Models;

namespace TruequeTextil.Features.DetallePropuesta;

public partial class DetallePropuestaPage
{
    [Parameter] public int PropuestaId { get; set; }

    private PropuestaTrueque? propuesta;
    private Usuario? currentUser;
    private List<MensajeNegociacion> mensajes = new();
    private bool isLoading = true;
    private bool mostrarChat = false;
    private bool enviandoMensaje = false;
    private bool procesandoAccion = false;
    private string nuevoMensaje = string.Empty;
    private int mensajesNoLeidos = 0;

    // Estados de modales
    private bool mostrarModalAceptar = false;
    private bool mostrarModalRechazar = false;
    private bool mostrarModalCancelar = false;
    private bool mostrarModalCompletar = false;

    // Mensajes para modales
    private string? mensajeAceptacion;
    private string? motivoRechazo;
    private string? motivoCancelacion;

    // Propiedades de conveniencia
    private bool EsUsuarioReceptor => currentUser != null && propuesta?.UsuarioReceptorId == currentUser.UsuarioId;
    private bool EsUsuarioProponente => currentUser != null && propuesta?.UsuarioProponenteId == currentUser.UsuarioId;

    protected override async Task OnParametersSetAsync()
    {
        await LoadPropuesta();
    }

    private async Task LoadPropuesta()
    {
        isLoading = true;
        
        try
        {
            currentUser = await AuthenticationStateProvider.GetCurrentUserAsync();
            if (currentUser != null)
            {
                propuesta = await DetallePropuestaService.ObtenerDetallePropuesta(PropuestaId, currentUser.UsuarioId);
                
                if (propuesta != null)
                {
                    // Cargar mensajes si la propuesta está aceptada
                    if (propuesta.EstadoPropuestaId == 2)
                    {
                        await CargarMensajes();
                        mensajesNoLeidos = await DetallePropuestaService.ContarMensajesNoLeidos(PropuestaId, currentUser.UsuarioId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar la propuesta: {ex.Message}");
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task CargarMensajes()
    {
        if (currentUser != null)
        {
            mensajes = await DetallePropuestaService.ObtenerMensajes(PropuestaId, currentUser.UsuarioId);
        }
    }

    private async Task CargarMensajesConRefresh()
    {
        await CargarMensajes();
        if (currentUser != null)
        {
            mensajesNoLeidos = await DetallePropuestaService.ContarMensajesNoLeidos(PropuestaId, currentUser.UsuarioId);
        }
        StateHasChanged();
    }

    // Métodos para estados de propuesta
    private (string Class, string Icon, string Text) GetEstadoBadge()
    {
        return propuesta?.EstadoPropuestaId switch
        {
            1 => ("bg-warning text-dark", "bi bi-clock", "Pendiente"),
            2 => ("bg-info text-white", "bi bi-chat-dots", "Aceptada - En negociación"),
            3 => ("bg-danger text-white", "bi bi-x-circle", "Rechazada"),
            4 => ("bg-secondary text-white", "bi bi-arrow-left-right", "Contraoferta"),
            5 => ("bg-dark text-white", "bi bi-hourglass-split", "Expirada"),
            6 => ("bg-success text-white", "bi bi-check-circle", "Completada"),
            7 => ("bg-secondary text-white", "bi bi-x", "Cancelada"),
            _ => ("bg-light text-dark", "bi bi-question-circle", "Desconocido")
        };
    }

    // Métodos para mostrar/ocultar modales
    private void MostrarModalAceptar()
    {
        mostrarModalAceptar = true;
        mensajeAceptacion = string.Empty;
    }

    private void CerrarModalAceptar()
    {
        mostrarModalAceptar = false;
        mensajeAceptacion = null;
    }

    private void MostrarModalRechazar()
    {
        mostrarModalRechazar = true;
        motivoRechazo = string.Empty;
    }

    private void CerrarModalRechazar()
    {
        mostrarModalRechazar = false;
        motivoRechazo = null;
    }

    private void MostrarModalCancelar()
    {
        mostrarModalCancelar = true;
        motivoCancelacion = string.Empty;
    }

    private void CerrarModalCancelar()
    {
        mostrarModalCancelar = false;
        motivoCancelacion = null;
    }

    private void MostrarModalCompletar()
    {
        mostrarModalCompletar = true;
    }

    private void CerrarModalCompletar()
    {
        mostrarModalCompletar = false;
    }

    // Acciones de propuestas
    private async Task AceptarPropuesta()
    {
        if (currentUser == null) return;

        procesandoAccion = true;
        try
        {
            var resultado = await DetallePropuestaService.AceptarPropuesta(PropuestaId, currentUser.UsuarioId, mensajeAceptacion);
            
            if (resultado.Exito)
            {
                CerrarModalAceptar();
                await LoadPropuesta(); // Recargar para obtener el estado actualizado
                mostrarChat = true; // Mostrar chat automáticamente cuando se acepta
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", resultado.Mensaje);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al aceptar la propuesta: {ex.Message}");
            await JSRuntime.InvokeVoidAsync("alert", "Error al aceptar la propuesta. Inténtalo de nuevo.");
        }
        finally
        {
            procesandoAccion = false;
        }
    }

    private async Task RechazarPropuesta()
    {
        if (currentUser == null) return;

        procesandoAccion = true;
        try
        {
            var resultado = await DetallePropuestaService.RechazarPropuesta(PropuestaId, currentUser.UsuarioId, motivoRechazo);
            
            if (resultado.Exito)
            {
                CerrarModalRechazar();
                await LoadPropuesta(); // Recargar para obtener el estado actualizado
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", resultado.Mensaje);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al rechazar la propuesta: {ex.Message}");
            await JSRuntime.InvokeVoidAsync("alert", "Error al rechazar la propuesta. Inténtalo de nuevo.");
        }
        finally
        {
            procesandoAccion = false;
        }
    }

    private async Task CancelarPropuesta()
    {
        if (currentUser == null) return;

        procesandoAccion = true;
        try
        {
            var resultado = await DetallePropuestaService.CancelarPropuesta(PropuestaId, currentUser.UsuarioId, motivoCancelacion);
            
            if (resultado.Exito)
            {
                CerrarModalCancelar();
                await LoadPropuesta(); // Recargar para obtener el estado actualizado
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", resultado.Mensaje);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cancelar la propuesta: {ex.Message}");
            await JSRuntime.InvokeVoidAsync("alert", "Error al cancelar la propuesta. Inténtalo de nuevo.");
        }
        finally
        {
            procesandoAccion = false;
        }
    }

    private async Task CompletarTrueque()
    {
        if (currentUser == null) return;

        procesandoAccion = true;
        try
        {
            var resultado = await DetallePropuestaService.CompletarTrueque(PropuestaId, currentUser.UsuarioId);
            
            if (resultado.Exito)
            {
                CerrarModalCompletar();
                await LoadPropuesta(); // Recargar para obtener el estado actualizado
                
                // Mostrar mensaje de éxito y redirigir a evaluación
                await JSRuntime.InvokeVoidAsync("alert", $"{resultado.Mensaje} Ahora puedes evaluar a tu contraparte.");
                NavigationManager.NavigateTo($"/evaluacion/{PropuestaId}");
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", resultado.Mensaje);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al completar el trueque: {ex.Message}");
            await JSRuntime.InvokeVoidAsync("alert", "Error al completar el trueque. Inténtalo de nuevo.");
        }
        finally
        {
            procesandoAccion = false;
        }
    }

    // Chat de negociación
    private void AlternarChat()
    {
        mostrarChat = !mostrarChat;
        
        if (mostrarChat)
        {
            // Al abrir el chat, recargar mensajes para obtener los no leídos
            _ = Task.Run(async () => await CargarMensajesConRefresh());
        }
    }

    private async Task EnviarMensaje()
    {
        if (string.IsNullOrWhiteSpace(nuevoMensaje) || currentUser == null) return;

        enviandoMensaje = true;
        try
        {
            var resultado = await DetallePropuestaService.EnviarMensaje(PropuestaId, currentUser.UsuarioId, nuevoMensaje.Trim());
            
            if (resultado.Exito)
            {
                nuevoMensaje = string.Empty;
                await CargarMensajesConRefresh();
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", resultado.Mensaje);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar el mensaje: {ex.Message}");
            await JSRuntime.InvokeVoidAsync("alert", "Error al enviar el mensaje. Inténtalo de nuevo.");
        }
        finally
        {
            enviandoMensaje = false;
        }
    }

    private async Task EnviarMensajeEnter(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await EnviarMensaje();
        }
    }

    // Navegación
    private void VolverAPropuestas()
    {
        NavigationManager.NavigateTo("/propuestas-recibidas");
    }

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IDetallePropuestaService DetallePropuestaService { get; set; } = default!;

    [Inject]
    private CustomAuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;
}