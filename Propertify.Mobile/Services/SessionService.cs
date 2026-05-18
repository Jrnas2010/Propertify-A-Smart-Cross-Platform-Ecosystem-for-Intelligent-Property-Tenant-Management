namespace Propertify.Mobile.Services
{
    public class SessionService
    {
        public int    UserId       { get; private set; }
        public int    TenantId     { get; private set; }
        public int    UnitId       { get; private set; }
        public string UnitNumber   { get; private set; } = string.Empty;
        public string PropertyName { get; private set; } = string.Empty;
        public string FullName     { get; private set; } = string.Empty;
        public string Permissions  { get; private set; } = string.Empty;
        public bool   IsLoggedIn   => UserId > 0;

        public void SetSession(int userId, int tenantId, int unitId,
            string unitNumber, string propertyName, string fullName, string permissions)
        {
            UserId       = userId;
            TenantId     = tenantId;
            UnitId       = unitId;
            UnitNumber   = unitNumber;
            PropertyName = propertyName;
            FullName     = fullName;
            Permissions  = permissions;
        }

        public void Clear()
        {
            UserId = TenantId = UnitId = 0;
            UnitNumber = PropertyName = FullName = Permissions = string.Empty;
        }

        // If Permissions is empty, all features are accessible by default.
        public bool HasPermission(string feature) =>
            string.IsNullOrWhiteSpace(Permissions) ||
            Permissions.Contains(feature, StringComparison.OrdinalIgnoreCase);
    }
}
