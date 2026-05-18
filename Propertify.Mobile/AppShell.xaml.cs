using Propertify.Mobile.Services;
using Propertify.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Propertify.Mobile;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services, SessionService session)
    {
        InitializeComponent();

        // Use factory lambdas so each page is resolved from DI (constructor injection works).
        DashboardContent.ContentTemplate  = new DataTemplate(() => services.GetRequiredService<MainPage>());
        ContractsContent.ContentTemplate  = new DataTemplate(() => services.GetRequiredService<ContractsPage>());
        InvoicesContent.ContentTemplate   = new DataTemplate(() => services.GetRequiredService<InvoicesPage>());
        MaintenanceContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<MaintenancePage>());

        // AddMaintenancePage navigated to via route — DI handles it automatically.
        Routing.RegisterRoute("addmaintenance", typeof(AddMaintenancePage));

        // Hide tabs the tenant has no permission for.
        // Empty Permissions = all tabs visible (default tenant access).
        if (!session.HasPermission("Contracts"))
            ContractsTab.IsVisible = false;

        if (!session.HasPermission("Invoices") && !session.HasPermission("Billing"))
            InvoicesTab.IsVisible = false;

        if (!session.HasPermission("Maintenance"))
            MaintenanceTab.IsVisible = false;
    }
}
