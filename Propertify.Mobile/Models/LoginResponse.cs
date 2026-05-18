namespace Propertify.Mobile.Models
{
    public class LoginResponse
    {
        public bool   Success      { get; set; }
        public string Message      { get; set; } = string.Empty;
        public int    UserId       { get; set; }
        public int    TenantId     { get; set; }
        public int    UnitId       { get; set; }
        public string UnitNumber   { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string FullName     { get; set; } = string.Empty;
        public string Permissions  { get; set; } = string.Empty;
    }
}
