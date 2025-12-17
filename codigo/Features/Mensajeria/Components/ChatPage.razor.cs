using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.IO;
using TruequeTextil.Shared.Models;
using TruequeTextil.Features.Mensajeria.Interfaces;

namespace TruequeTextil.Features.Mensajeria.Components;

public partial class ChatPage : IAsyncDisposable
{
    [Parameter] public int PropuestaId { get; set; }

    // Injected services
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private IMensajeriaService MensajeriaService { get; set; } = default!;
    [Inject] private ILogger<ChatPage> _logger { get; set; } = default!;

    // Data
    private PropuestaTrueque? propuesta;
    private List<MensajeNegociacion> mensajes = new();
    private Usuario? currentUser;
    private bool puedeEnviarMensaje = false;

    // UI state
    private bool cargando = true;
    private bool enviando = false;
    private string mensajeError = string.Empty;

    // Message input
    private string nuevoMensaje = string.Empty;

    // SignalR
    private HubConnection? hubConnection;

    // Typing indicator
    private bool isTyping = false;
    private System.Timers.Timer? typingTimer;

    // File attachment
    private IBrowserFile? selectedFile;
    private bool uploadingFile = false;

    // Pagination
    private int pageSize = 50;
    private bool hasMoreMessages = true;
    private bool loadingMore = false;

    // Unread messages
    private int unreadCount = 0;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        try
        {
            cargando = true;

            // Get current user
            currentUser = await MensajeriaService.GetCurrentUser();
            if (currentUser == null)
            {
                NavigationManager.NavigateTo("/login");
                return;
            }

            // Get proposal using the service method with proper usuarioId
            propuesta = await MensajeriaService.ObtenerPropuesta(PropuestaId, currentUser.UsuarioId);
            if (propuesta == null)
            {
                cargando = false;
                return;
            }

            // Check permissions
            puedeEnviarMensaje = await MensajeriaService.PuedeEnviarMensaje(PropuestaId, currentUser.UsuarioId);

            // Get messages
            mensajes = await MensajeriaService.ObtenerMensajes(PropuestaId, currentUser.UsuarioId);

            // Initialize SignalR connection
            await InitializeSignalR();

            // Scroll to bottom after render
            await Task.Delay(100);
            await ScrollToBottom();
        }
        catch (Exception ex)
        {
            mensajeError = $"Error al cargar el chat: {ex.Message}";
        }
        finally
        {
            cargando = false;
        }
    }

    private async Task EnviarMensaje()
    {
        if ((string.IsNullOrWhiteSpace(nuevoMensaje) && selectedFile == null) || enviando || currentUser == null) return;

        mensajeError = string.Empty;
        enviando = true;

        try
        {
            string mensajeTexto = nuevoMensaje;

            // If there's a file, upload it first
            if (selectedFile != null)
            {
                await UploadFile();
                if (selectedFile != null) // Upload failed
                {
                    return;
                }
                // File uploaded, now send message with file reference
                if (string.IsNullOrWhiteSpace(mensajeTexto))
                {
                    mensajeTexto = $"[Archivo adjunto]";
                }
            }

            if (!string.IsNullOrWhiteSpace(mensajeTexto))
            {
                var (exito, mensaje, mensajeId) = await MensajeriaService.EnviarMensaje(PropuestaId, currentUser.UsuarioId, mensajeTexto);

                if (exito)
                {
                    nuevoMensaje = string.Empty;
                    await StopTyping();
                    // Message will be added via SignalR
                    await ScrollToBottom();
                }
                else
                {
                    mensajeError = mensaje ?? "Error al enviar el mensaje.";
                }
            }
        }
        catch (Exception ex)
        {
            mensajeError = $"Error al enviar el mensaje: {ex.Message}";
        }
        finally
        {
            enviando = false;
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await EnviarMensaje();
        }
        else if (!string.IsNullOrWhiteSpace(e.Key) && e.Key.Length == 1)
        {
            await StartTyping();
        }
    }

    private async Task StartTyping()
    {
        if (hubConnection != null && currentUser != null && !isTyping)
        {
            isTyping = true;
            await hubConnection.SendAsync("TypingIndicator", PropuestaId, currentUser.UsuarioId, true);

            // Stop typing after 3 seconds of inactivity
            typingTimer?.Dispose();
            typingTimer = new System.Timers.Timer(3000);
            typingTimer.Elapsed += async (sender, e) => await StopTyping();
            typingTimer.Start();
        }
    }

    private async Task StopTyping()
    {
        if (hubConnection != null && currentUser != null && isTyping)
        {
            isTyping = false;
            await hubConnection.SendAsync("TypingIndicator", PropuestaId, currentUser.UsuarioId, false);
            typingTimer?.Dispose();
            typingTimer = null;
        }
    }

    private async Task ScrollToBottom()
    {
        await JSRuntime.InvokeVoidAsync("eval",
            "const container = document.getElementById('messagesContainer'); if (container) { container.scrollTop = container.scrollHeight; }"
        );
        MarkMessagesAsRead();
    }

    private void VolverAtras()
    {
        NavigationManager.NavigateTo("/mensajes");
    }

    private string GetIniciales(string nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return "?";
        var partes = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length >= 2)
            return $"{partes[0][0]}{partes[1][0]}".ToUpper();
        return nombre.Length >= 2 ? nombre[..2].ToUpper() : nombre.ToUpper();
    }

    private string FormatearFecha(DateTime fecha)
    {
        var ahora = DateTime.Now;
        var diferencia = ahora - fecha;

        if (diferencia.TotalMinutes < 1) return "Ahora";
        if (diferencia.TotalMinutes < 60) return $"Hace {(int)diferencia.TotalMinutes} min";
        if (diferencia.TotalHours < 24) return $"Hace {(int)diferencia.TotalHours} h";
        if (diferencia.TotalDays < 7) return $"Hace {(int)diferencia.TotalDays} d";
        return fecha.ToString("dd/MM/yyyy HH:mm");
    }

    private string GetEstadoBadgeClass(int estadoId)
    {
        return estadoId switch
        {
            1 => "bg-amber-100 text-amber-800",
            2 => "bg-success bg-opacity-10 text-success",
            3 => "bg-danger bg-opacity-10 text-danger",
            4 => "bg-amber-100 text-amber-800",
            5 => "bg-secondary bg-opacity-10 text-secondary",
            6 => "bg-primary bg-opacity-10 text-primary",
            7 => "bg-secondary bg-opacity-10 text-secondary",
            _ => "bg-secondary bg-opacity-10 text-secondary"
        };
    }

    private string GetEstadoNombre(int estadoId)
    {
        return estadoId switch
        {
            1 => "Pendiente",
            2 => "Aceptada",
            3 => "Rechazada",
            4 => "Contraoferta",
            5 => "Expirada",
            6 => "Completada",
            7 => "Cancelada",
            _ => "Desconocido"
        };
    }

    private async Task InitializeSignalR()
    {
        hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/chathub"))
            .WithAutomaticReconnect()
            .Build();

        hubConnection.On<MensajeNegociacion>("ReceiveMessage", (mensaje) =>
        {
            // Add message to list if not already present
            if (!mensajes.Any(m => m.MensajeId == mensaje.MensajeId))
            {
                mensajes.Add(mensaje);

                // Increment unread count if message is not from current user
                if (mensaje.UsuarioId != currentUser?.UsuarioId)
                {
                    unreadCount++;
                }

                InvokeAsync(StateHasChanged);
                InvokeAsync(ScrollToBottom);
            }
        });

        hubConnection.On<int, bool>("UserTyping", (usuarioId, typing) =>
        {
            // Handle typing indicator - for now, just log
            _logger.LogInformation("User {UsuarioId} typing: {Typing}", usuarioId, typing);
            // TODO: Update UI to show typing indicator
        });

        hubConnection.Reconnecting += error =>
        {
            _logger.LogWarning("Reconnecting to SignalR hub: {Error}", error?.Message);
            return Task.CompletedTask;
        };

        hubConnection.Reconnected += connectionId =>
        {
            _logger.LogInformation("Reconnected to SignalR hub: {ConnectionId}", connectionId);
            return Task.CompletedTask;
        };

        hubConnection.Reconnecting += error =>
        {
            _logger.LogWarning("Reconnecting to SignalR hub: {Error}", error?.Message);
            return Task.CompletedTask;
        };

        hubConnection.Reconnected += async connectionId =>
        {
            _logger.LogInformation("Reconnected to SignalR hub: {ConnectionId}", connectionId);

            // Volver a unirse al chat tras reconexión
            await hubConnection.SendAsync("JoinChat", PropuestaId);
        };

        try
        {
            await hubConnection.StartAsync();
            await hubConnection.SendAsync("JoinChat", PropuestaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to SignalR hub");
        }
    }

    private async Task OnFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file == null) return;

        // Validate file size (max 10MB)
        if (file.Size > 10 * 1024 * 1024)
        {
            mensajeError = "El archivo es demasiado grande. Máximo 10MB.";
            return;
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx" };
        var extension = Path.GetExtension(file.Name).ToLower();
        if (!allowedExtensions.Contains(extension))
        {
            mensajeError = "Tipo de archivo no permitido. Solo imágenes, PDF y documentos de Word.";
            return;
        }

        selectedFile = file;
        mensajeError = string.Empty;
    }

    private void RemoveFile()
    {
        selectedFile = null;
    }

    private async Task UploadFile()
    {
        if (selectedFile == null || currentUser == null) return;

        uploadingFile = true;
        try
        {
            // TODO: Implement file upload to server
            // For now, just simulate
            await Task.Delay(1000);

            // Create message with file attachment
            var fileMessage = $"[Archivo: {selectedFile.Name}]";
            var (exito, mensaje, mensajeId) = await MensajeriaService.EnviarMensaje(PropuestaId, currentUser.UsuarioId, fileMessage);

            if (exito)
            {
                selectedFile = null;
            }
            else
            {
                mensajeError = mensaje ?? "Error al enviar el archivo.";
            }
        }
        catch (Exception ex)
        {
            mensajeError = $"Error al subir el archivo: {ex.Message}";
        }
        finally
        {
            uploadingFile = false;
        }
    }

    private async Task LoadMoreMessages()
    {
        if (!hasMoreMessages || loadingMore || currentUser == null) return;

        loadingMore = true;
        try
        {
            // TODO: Implement pagination in service
            // For now, just simulate loading more messages
            var olderMessages = await MensajeriaService.ObtenerMensajes(PropuestaId, currentUser.UsuarioId);
            var newMessages = olderMessages.Where(m => !mensajes.Any(existing => existing.MensajeId == m.MensajeId)).ToList();

            if (newMessages.Any())
            {
                mensajes.InsertRange(0, newMessages);
                await InvokeAsync(StateHasChanged);
            }
            else
            {
                hasMoreMessages = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading more messages");
        }
        finally
        {
            loadingMore = false;
        }
    }

    private void MarkMessagesAsRead()
    {
        unreadCount = 0;
        InvokeAsync(StateHasChanged);
    }


    public async ValueTask DisposeAsync()
    {
        typingTimer?.Dispose();

        if (hubConnection is not null)
        {
            try
            {
                if (hubConnection.State == HubConnectionState.Connected)
                {
                    await hubConnection.SendAsync("LeaveChat", PropuestaId);
                    await hubConnection.StopAsync();
                }
            }
            catch
            {
                // Ignorar errores en dispose (el circuito ya puede estar muerto)
            }
            finally
            {
                await hubConnection.DisposeAsync();
                hubConnection = null;
            }
        }
    }

}