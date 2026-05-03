using Microsoft.EntityFrameworkCore;
using FitnessTracker.Data;
using FitnessTracker.Services;
using FitnessTracker.Components;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ────────────────────────────────────────────────────────────────
// Connection string is in appsettings.json (or appsettings.Development.json for local dev)
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Blazor ──────────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ─── App Services ────────────────────────────────────────────────────────────
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<FitnessService>();
builder.Services.AddScoped<ClientStateService>();
builder.Services.AddSingleton<SessionNotificationService>(); // Singleton so events persist per app

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
