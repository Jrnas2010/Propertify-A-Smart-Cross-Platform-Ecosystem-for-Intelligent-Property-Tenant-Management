using System.Net.Http.Headers;

namespace Propertify.Mobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        // TODO: Replace with your server URL before deployment
        // For local testing use your machine's IP, e.g. "http://192.168.1.x:5287/api/"
        private const string BaseUrl = "https://your-api-link.com/api/";

        public ApiService()
        {
            _httpClient = new HttpClient();
        }

        // رفع بلاغ الصيانة مع الصورة
        public async Task<bool> UploadMaintenance(FileResult photo, string title, string description, int unitId)
        {
            try
            {
                using var content = new MultipartFormDataContent();

                content.Add(new StringContent(title), "Title");
                content.Add(new StringContent(description), "Description");
                content.Add(new StringContent(unitId.ToString()), "UnitId");

                var stream = await photo.OpenReadAsync();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                    string.IsNullOrEmpty(photo.ContentType) ? "image/jpeg" : photo.ContentType);

                // Field name must match MaintenanceRequestDto.ImageFile
                content.Add(fileContent, "ImageFile", photo.FileName);

                // Correct endpoint: api/maintenance/submit
                var response = await _httpClient.PostAsync($"{BaseUrl}maintenance/submit", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
