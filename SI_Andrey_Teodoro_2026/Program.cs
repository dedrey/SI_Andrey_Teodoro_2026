using MudBlazor;
using MudBlazor.Services;
using SI_Andrey_Teodoro_2026.Components;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.Repositories;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── Blazor Server .NET 9 ─────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── MudBlazor ────────────────────────────────────────────────
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 4000;
    config.SnackbarConfiguration.HideTransitionDuration = 300;
    config.SnackbarConfiguration.ShowTransitionDuration = 300;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

// ── Conexão com banco ────────────────────────────────────────
builder.Services.AddSingleton<DbConnectionFactory>();

// ── Repositórios ─────────────────────────────────────────────
builder.Services.AddScoped<IPaisRepository, PaisRepository>();
builder.Services.AddScoped<IEstadoRepository, EstadoRepository>();
builder.Services.AddScoped<ICidadeRepository, CidadeRepository>();

// ── Serviços ──────────────────────────────────────────────────
builder.Services.AddScoped<IPaisService, PaisService>();
builder.Services.AddScoped<IEstadoService, EstadoService>();
builder.Services.AddScoped<ICidadeService, CidadeService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// ✅ .NET 9 — MapRazorComponents em vez de MapBlazorHub + MapFallbackToPage
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();