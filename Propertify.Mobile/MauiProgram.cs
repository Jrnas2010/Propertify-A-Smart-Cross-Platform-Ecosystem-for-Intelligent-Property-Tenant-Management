using Microsoft.Extensions.Logging;
using Propertify.Mobile.Services;
using Propertify.Mobile.ViewModels;
using Propertify.Mobile.Views;

namespace Propertify.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf",  "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ── Singleton services (shared state across the app lifetime) ──────
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<SessionService>();

        // ── ViewModels (new instance per page) ─────────────────────────────
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<ContractsViewModel>();
        builder.Services.AddTransient<InvoicesViewModel>();
        builder.Services.AddTransient<MaintenanceViewModel>();

        // ── Pages (new instance per navigation) ────────────────────────────
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<ContractsPage>();
        builder.Services.AddTransient<InvoicesPage>();
        builder.Services.AddTransient<MaintenancePage>();
        builder.Services.AddTransient<AddMaintenancePage>();

        // ── Shell (rebuilt after each login) ───────────────────────────────
        builder.Services.AddTransient<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
