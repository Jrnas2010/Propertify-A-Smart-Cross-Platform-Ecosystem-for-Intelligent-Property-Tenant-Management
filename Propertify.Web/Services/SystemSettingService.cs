using Propertify.Web.Data;
using Propertify.Web.Models;

namespace Propertify.Web.Services
{
    /// <summary>
    /// Singleton service that caches system appearance/settings from the DB.
    /// Call InvalidateCache() after saving new settings.
    /// </summary>
    public class SystemSettingService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private static Dictionary<string, string>? _cache;
        private static readonly object _lock = new();

        public SystemSettingService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        private Dictionary<string, string> LoadCache()
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return ctx.SystemSettings.ToDictionary(s => s.Key, s => s.Value);
        }

        public string Get(string key, string defaultValue = "")
        {
            if (_cache == null)
            {
                lock (_lock)
                {
                    _cache ??= LoadCache();
                }
            }
            return _cache.TryGetValue(key, out var v) ? v : defaultValue;
        }

        public void InvalidateCache()
        {
            lock (_lock) { _cache = null; }
        }

        // Convenience getters with brand defaults
        public string SidebarColor  => Get("SidebarColor",  "#0f172a");
        public string PrimaryColor  => Get("PrimaryColor",  "#1e3a5f");
        public string AccentColor   => Get("AccentColor",   "#3b82f6");
        public string FontFamily    => Get("FontFamily",    "Inter");

        public string FontCssUrl => FontFamily switch
        {
            "Inter"            => "https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap",
            "Plus Jakarta Sans"=> "https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&display=swap",
            "Roboto"           => "https://fonts.googleapis.com/css2?family=Roboto:wght@400;500;700&display=swap",
            "Poppins"          => "https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600;700&display=swap",
            "Cairo"            => "https://fonts.googleapis.com/css2?family=Cairo:wght@400;600;700&display=swap",
            _                  => "https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap"
        };
    }
}
