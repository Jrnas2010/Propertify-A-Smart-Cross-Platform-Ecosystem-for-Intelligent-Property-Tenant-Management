namespace Propertify.Web.Models
{
    public static class MaintenanceStatus
    {
        public const string Pending = "Pending";
        public const string InProgress = "InProgress";
        public const string Completed = "Completed";

        public static readonly string[] All = [Pending, InProgress, Completed];
    }

    public static class ContractStatus
    {
        public const string Active = "Active";
        public const string Expired = "Expired";
        public const string Terminated = "Terminated";

        public static readonly string[] All = [Active, Expired, Terminated];
    }

    public static class UnitStatus
    {
        public const string Vacant = "Vacant";
        public const string Occupied = "Occupied";
        public const string Maintenance = "Maintenance";

        public static readonly string[] All = [Vacant, Occupied, Maintenance];
    }

    public static class MaintenancePriority
    {
        public const string Low = "Low";
        public const string Normal = "Normal";
        public const string High = "High";
        public const string Urgent = "Urgent";

        public static readonly string[] All = [Low, Normal, High, Urgent];
    }

    public static class UserRole
    {
        public const string Owner = "Owner";
        public const string Tenant = "Tenant";
    }

    public static class ServiceType
    {
        public const string Electricity = "Electricity";
        public const string Water = "Water";

        public static readonly string[] All = [Electricity, Water];
    }
}
