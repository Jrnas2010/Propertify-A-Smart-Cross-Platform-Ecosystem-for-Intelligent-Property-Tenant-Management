using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Propertify.Mobile.Services;

namespace Propertify.Mobile.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ApiService       _api;
        private readonly SessionService   _session;
        private readonly IServiceProvider _services;

        [ObservableProperty] public partial string Email             { get; set; } = string.Empty;
        [ObservableProperty] public partial string Password          { get; set; } = string.Empty;
        [ObservableProperty] public partial string ErrorMessage      { get; set; } = string.Empty;
        [ObservableProperty] public partial bool   IsBusy            { get; set; } = false;
        [ObservableProperty] public partial bool   HasError          { get; set; } = false;
        [ObservableProperty] public partial bool   IsPasswordVisible { get; set; } = false;

        public string EyeIcon => IsPasswordVisible ? "🙈" : "👁";

        partial void OnIsPasswordVisibleChanged(bool value) => OnPropertyChanged(nameof(EyeIcon));

        [RelayCommand]
        private void TogglePasswordVisibility() => IsPasswordVisible = !IsPasswordVisible;

        public LoginViewModel(ApiService api, SessionService session, IServiceProvider services)
        {
            _api      = api;
            _session  = session;
            _services = services;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter your email and password.";
                HasError     = true;
                return;
            }

            IsBusy   = true;
            HasError = false;

            var result = await _api.LoginAsync(Email.Trim(), Password);

            IsBusy = false;

            if (!result.Success)
            {
                ErrorMessage = string.IsNullOrEmpty(result.Message)
                    ? "Login failed. Please check your credentials."
                    : result.Message;
                HasError = true;
                return;
            }

            _session.SetSession(
                result.UserId,
                result.TenantId,
                result.UnitId,
                result.UnitNumber,
                result.PropertyName,
                result.FullName,
                result.Permissions);

            Application.Current!.Windows[0].Page = _services.GetRequiredService<AppShell>();
        }
    }
}
