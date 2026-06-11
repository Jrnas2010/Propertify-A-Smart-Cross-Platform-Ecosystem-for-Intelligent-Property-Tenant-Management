using Propertify.Mobile.Services;

namespace Propertify.Mobile.Views
{
    public partial class AddMaintenancePage : ContentPage
    {
        private readonly ApiService     _api;
        private readonly SessionService _session;

        private FileResult? _photo;
        private string      _priority = "Normal";

        public AddMaintenancePage(ApiService api, SessionService session)
        {
            InitializeComponent();
            _api     = api;
            _session = session;
        }

        private void SetPriorityChip(Border active)
        {
            var chips = new[] { PrioNormal, PrioHigh, PrioUrgent };
            foreach (var chip in chips)
            {
                chip.BackgroundColor = Color.FromArgb("#FFFFFF");
                chip.StrokeThickness = 1;
                if (chip.Content is Label l) l.TextColor = Color.FromArgb("#64748b");
            }
            active.BackgroundColor = Color.FromArgb("#1e3a5f");
            active.StrokeThickness = 0;
            if (active.Content is Label lbl) lbl.TextColor = Colors.White;
        }

        private void OnPrioNormal(object? sender, TappedEventArgs e) { _priority = "Normal"; SetPriorityChip(PrioNormal); }
        private void OnPrioHigh(object? sender, TappedEventArgs e)   { _priority = "High";   SetPriorityChip(PrioHigh); }
        private void OnPrioUrgent(object? sender, TappedEventArgs e) { _priority = "Urgent"; SetPriorityChip(PrioUrgent); }

        private async void OnCapturePhotoClicked(object? sender, EventArgs e)
        {
            try
            {
                _photo = await MediaPicker.Default.CapturePhotoAsync();
                if (_photo != null)
                {
                    var stream = await _photo.OpenReadAsync();
                    CapturedImage.Source   = ImageSource.FromStream(() => stream);
                    CapturedImage.IsVisible   = true;
                    PhotoPlaceholder.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Camera Error", $"Could not open camera: {ex.Message}", "OK");
            }
        }

        private async void OnSubmitClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleEntry.Text))
            {
                await DisplayAlertAsync("Missing Info", "Please enter an issue title.", "OK");
                return;
            }

            bool confirm = await DisplayAlertAsync("Confirm", "Submit this maintenance request to the property manager?", "Yes", "Cancel");
            if (!confirm) return;

            bool success = await _api.SubmitMaintenanceAsync(
                TitleEntry.Text,
                DescriptionEditor.Text ?? string.Empty,
                _session.UnitId,
                _priority,
                _photo);

            if (success)
            {
                await DisplayAlertAsync("Submitted", "Your maintenance request has been sent to the property manager.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await DisplayAlertAsync("Error", "Could not connect to the server. Please check your internet connection.", "OK");
            }
        }
    }
}
