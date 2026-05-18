namespace Propertify.Mobile.Models
{
    // Kept for backwards-compatibility; no longer used directly.
    public class FinancialRecord
    {
        public string Type      { get; set; } = string.Empty;
        public string Date      { get; set; } = string.Empty;
        public string Amount    { get; set; } = string.Empty;
        public string Status    { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
    }
}
