namespace Propertify.Mobile.Views;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private async void OnMaintenanceClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("maintenance");
	}
}
