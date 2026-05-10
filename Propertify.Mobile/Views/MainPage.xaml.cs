namespace Propertify.Mobile.Views;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private async void OnMaintenanceTapped(object? sender, TappedEventArgs e)
	{
		// Navigate to maintenance request page
		await Shell.Current.GoToAsync("maintenance");
	}
}
