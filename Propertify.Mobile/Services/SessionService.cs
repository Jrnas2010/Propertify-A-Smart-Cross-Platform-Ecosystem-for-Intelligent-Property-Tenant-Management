namespace Propertify.Mobile.Services
{
    /// <summary>
    /// In-memory session store for the currently logged-in tenant.
    /// Registered as a singleton so any page or ViewModel can inject it to read the active identity.
    /// </summary>
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

        /// <summary>Populates all session fields after a successful login response.</summary>
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

        /// <summary>Resets all session fields to their default values (logout).</summary>
        public void Clear()
        {
            UserId = TenantId = UnitId = 0;
            UnitNumber = PropertyName = FullName = Permissions = string.Empty;
        }

        /// <summary>Returns true if <paramref name="feature"/> appears in the permissions list, or if the list is empty (all-access).</summary>
        public bool HasPermission(string feature) =>
            string.IsNullOrWhiteSpace(Permissions) ||
            Permissions.Contains(feature, StringComparison.OrdinalIgnoreCase);
    }
}
