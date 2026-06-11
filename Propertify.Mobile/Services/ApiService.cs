using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Propertify.Mobile.Models;

namespace Propertify.Mobile.Services
{
    /// <summary>
    /// HTTP client wrapper for the Propertify mobile REST API.
    /// All methods return default/empty values on network or deserialisation failure
    /// so the UI stays stable without crashing.
    /// </summary>
    public class ApiService
    {
        private readonly HttpClient _http;

        private const string BaseUrl = "http://10.7.109.240:5287/api/mobile/";

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService()
        {
            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        }

        /// <summary>Sends credentials to <c>POST api/mobile/login</c> and returns the server response.</summary>
        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            try
            {
                var body    = JsonSerializer.Serialize(new { email, password });
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var resp    = await _http.PostAsync($"{BaseUrl}login", content);
                var json    = await resp.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<LoginResponse>(json, JsonOpts)
                       ?? new LoginResponse { Success = false, Message = "Empty response." };
            }
            catch (Exception ex)
            {
                return new LoginResponse { Success = false, Message = ex.Message };
            }
        }

        /// <summary>Fetches the aggregated dashboard data for <paramref name="tenantId"/>. Returns null on failure.</summary>
        public async Task<DashboardDto?> GetDashboardAsync(int tenantId)
        {
            try
            {
                var json = await _http.GetStringAsync($"{BaseUrl}dashboard/{tenantId}");
                return JsonSerializer.Deserialize<DashboardDto>(json, JsonOpts);
            }
            catch { return null; }
        }

        /// <summary>Returns the list of contracts for <paramref name="tenantId"/>, or an empty list on failure.</summary>
        public async Task<List<ContractDto>> GetContractsAsync(int tenantId)
        {
            try
            {
                var json = await _http.GetStringAsync($"{BaseUrl}contracts/{tenantId}");
                return JsonSerializer.Deserialize<List<ContractDto>>(json, JsonOpts) ?? new();
            }
            catch { return new(); }
        }

        /// <summary>Returns all utility bills for <paramref name="tenantId"/>, or an empty list on failure.</summary>
        public async Task<List<InvoiceDto>> GetInvoicesAsync(int tenantId)
        {
            try
            {
                var json = await _http.GetStringAsync($"{BaseUrl}invoices/{tenantId}");
                return JsonSerializer.Deserialize<List<InvoiceDto>>(json, JsonOpts) ?? new();
            }
            catch { return new(); }
        }

        /// <summary>Returns all maintenance requests for <paramref name="unitId"/>, or an empty list on failure.</summary>
        public async Task<List<MaintenanceDto>> GetMaintenanceAsync(int unitId)
        {
            try
            {
                var json = await _http.GetStringAsync($"{BaseUrl}maintenance/{unitId}");
                return JsonSerializer.Deserialize<List<MaintenanceDto>>(json, JsonOpts) ?? new();
            }
            catch { return new(); }
        }

        /// <summary>
        /// Posts a new maintenance request as a multipart form.
        /// Attaches the optional <paramref name="photo"/> as a binary stream.
        /// Returns true if the server responded with a success status code.
        /// </summary>
        public async Task<bool> SubmitMaintenanceAsync(string title, string description, int unitId, string priority, FileResult? photo)
        {
            try
            {
                using var form = new MultipartFormDataContent();
                form.Add(new StringContent(title),             "Title");
                form.Add(new StringContent(description),       "Description");
                form.Add(new StringContent(unitId.ToString()), "UnitId");
                form.Add(new StringContent(priority),          "Priority");

                if (photo != null)
                {
                    await using var stream = await photo.OpenReadAsync();
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                        string.IsNullOrEmpty(photo.ContentType) ? "image/jpeg" : photo.ContentType);
                    form.Add(fileContent, "ImageFile", photo.FileName);
                }

                var resp = await _http.PostAsync($"{BaseUrl}maintenance/submit", form);
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}
