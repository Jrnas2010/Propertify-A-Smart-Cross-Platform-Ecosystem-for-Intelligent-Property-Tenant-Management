namespace Propertify.Mobile.Models
{
    public class InvoiceDto
    {
        public int     BillId      { get; set; }
        public string  ServiceType { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string  IssueDate   { get; set; } = string.Empty;
        public string  Status      { get; set; } = string.Empty;
    }
}
