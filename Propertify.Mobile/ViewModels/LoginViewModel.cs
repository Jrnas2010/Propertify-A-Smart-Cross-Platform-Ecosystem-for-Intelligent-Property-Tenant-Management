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

        [ObservableProperty] private string email        = string.Empty;
        [ObservableProperty] private string password     = string.Empty;
        [ObservableProperty] private string errorMessage = string.Empty;
        [ObservableProperty] private bool   isBusy       = false;
        [ObservableProperty] private bool   hasError     = false;

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

            // Resolve AppShell from DI so all tabs get their pages via constructor injection.
            Application.Current!.MainPage = _services.GetRequiredService<AppShell>();
        }
    }
}
