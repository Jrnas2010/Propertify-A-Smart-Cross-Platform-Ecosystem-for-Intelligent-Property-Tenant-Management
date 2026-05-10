using Propertify.Mobile.Services;

namespace Propertify.Mobile.Views
{
    public partial class AddMaintenancePage : ContentPage
    {
        private readonly ApiService _apiService;

        // TODO: Set this from the logged-in tenant's profile once auth is implemented
        private int _unitId = 0;

        FileResult? _photo;

        public AddMaintenancePage(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
        }

        private async void OnCapturePhotoClicked(object sender, EventArgs e)
        {
            try
            {
                _photo = await MediaPicker.Default.CapturePhotoAsync();

                if (_photo != null)
                {
                    var stream = await _photo.OpenReadAsync();
                    CapturedImage.Source = ImageSource.FromStream(() => stream);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("خطأ", $"تعذر فتح الكاميرا: {ex.Message}", "موافق");
            }
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            if (_photo == null || string.IsNullOrWhiteSpace(TitleEntry.Text))
            {
                await DisplayAlert("تنبيه", "يرجى تصوير العطل وكتابة عنوان البلاغ أولاً", "موافق");
                return;
            }

            bool confirm = await DisplayAlert("تأكيد", "هل تريد إرسال هذا البلاغ للمالك؟", "نعم", "إلغاء");
            if (!confirm) return;

            bool success = await _apiService.UploadMaintenance(
                _photo,
                TitleEntry.Text,
                DescriptionEditor.Text ?? string.Empty,
                _unitId);

            if (success)
            {
                await DisplayAlert("نجاح", "تم إرسال طلب الصيانة بنجاح للمالك.", "موافق");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await DisplayAlert("خطأ", "فشل الاتصال بالسيرفر. تأكد من اتصال الإنترنت أو رابط الـ API.", "موافق");
            }
        }
    }
}
