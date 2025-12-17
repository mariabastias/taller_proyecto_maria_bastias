using Microsoft.Extensions.FileProviders;
using TruequeTextil.Features.Mensajeria;

var builder = WebApplication.CreateBuilder(args);

// Configure database connection
builder.Configuration.AddJsonFile("appsettings.json", optional: true);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MVC controllers for API endpoints
builder.Services.AddControllers();



builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/acceso-denegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        // Configurar para que funcione con el estado en memoria
        options.Events.OnValidatePrincipal = async context =>
        {
            // Integrar con CustomAuthenticationStateProvider si es necesario
        };
    });

builder.Services.AddAuthorization();



// RF-10: Add SignalR for real-time notifications
builder.Services.AddSignalR();

// Authentication is handled in-memory by CustomAuthenticationStateProvider

// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add HttpContext accessor for services that need it
builder.Services.AddHttpContextAccessor();

// Register all application services via centralized DI
builder.Services.AddApplicationServices();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseWhen(context => !context.Request.Path.StartsWithSegments("/auth"), appBuilder =>
{
    appBuilder.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
});
app.UseHttpsRedirection();



var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        // cachear archivos estáticos por 7 días
        ctx.Context.Response.Headers.Append(
            "Cache-Control", "public,max-age=604800"); // 7 días
    }
});



app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map API controllers
app.MapControllers();

// RF-10: Map SignalR hub for real-time notifications
app.MapHub<NotificacionesHub>("/notificacioneshub");

// Map SignalR hub for chat
app.MapHub<ChatHub>("/chathub");

app.Run();
