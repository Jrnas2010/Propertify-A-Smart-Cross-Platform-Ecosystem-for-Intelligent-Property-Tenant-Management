using Microsoft.Extensions.Logging;
using Propertify.Mobile.Services;
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
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<ApiService>();
		builder.Services.AddTransient<AddMaintenancePage>();

		Routing.RegisterRoute("maintenance", typeof(AddMaintenancePage));

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
