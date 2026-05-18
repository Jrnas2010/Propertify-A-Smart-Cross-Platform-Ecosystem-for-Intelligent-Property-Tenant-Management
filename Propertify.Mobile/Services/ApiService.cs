using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Propertify.Mobile.Models;

namespace Propertify.Mobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;

        // Android emulator → host machine localhost.
        // Change to your LAN IP (e.g. http://192.168.1.x:5287/api/mobile/) for a real device.
        private const string BaseUrl = "http://10.0.2.2:5287/api/mobile/";

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService()
        {
            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        }

        // ── Login ──────────────────────────────────────────────────────────
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

        // ── Dashboard ──────────────────────────────────────────────────────
        public async Task<DashboardDto?> GetDashboardAsync(int tenantId)
        {
            try
            {
                var json = await _http.GetStringAsync($"{BaseUrl}dashboard/{tenantId}");
                return JsonSerializer.Deserialize<DashboardDto>(json, JsonOpts);
            }
            catch { return null; }
        }

        // ── Contracts ──────────────────────────────────────────────────────
        public async Task<List<ContractDto>> GetContractsAsync(int tenantId)
        {
            try
            {
                var json = await _http.GetStringAsync($"{BaseUrl}contracts/{tenantId}");
                return JsonSerializer.Deserialize<List<ContractDto>>(json, JsonOpts) ?? new();
            }
            catch { return new(); }
        }

        // ── Invoices ───────────────────────────────────────────────────────
        public async Task<List<InvoiceDto>> GetInvoicesAsync(int tenantId)
        {
            try
            {
                var json = await _http.GetStringAsync($"{BaseUrl}invoices/{tenantId}");
                return JsonSerializer.Deserialize<List<InvoiceDto>>(json, JsonOpts) ?? new();
            }
            catch { return new(); }
        }

        // ── Maintenance list ───────────────────────────────────────────────
        public async Task<List<MaintenanceDto>> GetMaintenanceAsync(int unitId)
        {
            try
            {
                var json = await _http.GetStringAsync($"{BaseUrl}maintenance/{unitId}");
                return JsonSerializer.Deserialize<List<MaintenanceDto>>(json, JsonOpts) ?? new();
            }
            catch { return new(); }
        }

        // ── Submit maintenance request (with optional photo) ───────────────
        public async Task<bool> SubmitMaintenanceAsync(string title, string description, int unitId, FileResult? photo)
        {
            try
            {
                using var form = new MultipartFormDataContent();
                form.Add(new StringContent(title),             "Title");
                form.Add(new StringContent(description),       "Description");
                form.Add(new StringContent(unitId.ToString()), "UnitId");

                if (photo != null)
                {
                    var stream      = await photo.OpenReadAsync();
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
