namespace Propertify.Mobile.Models
{
    public class ContractDto
    {
        public int     Id            { get; set; }
        public string  StartDate     { get; set; } = string.Empty;
        public string  EndDate       { get; set; } = string.Empty;
        public decimal RentAmount    { get; set; }
        public decimal MonthlyRent   { get; set; }
        public string  Status        { get; set; } = string.Empty;
        public string  UnitNumber    { get; set; } = string.Empty;
        public string  PropertyName  { get; set; } = string.Empty;
        public int     DaysRemaining { get; set; }
    }
}
