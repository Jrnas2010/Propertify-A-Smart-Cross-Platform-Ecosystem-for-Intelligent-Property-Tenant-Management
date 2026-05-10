namespace Propertify.Mobile.Models
{
    public class Tenant
    {
        public int Id { get; set; }

        public string FirstNameAr { get; set; } = string.Empty;
        public string? SecondNameAr { get; set; }
        public string? ThirdNameAr { get; set; }
        public string LastNameAr { get; set; } = string.Empty;

        public string FirstNameEn { get; set; } = string.Empty;
        public string? SecondNameEn { get; set; }
        public string? ThirdNameEn { get; set; }
        public string LastNameEn { get; set; } = string.Empty;

        public string IdNumber { get; set; } = string.Empty;
        public string? IdDocumentPath { get; set; }
        public string Phone { get; set; } = string.Empty;

        public DateTime LeaseStartDate { get; set; }
        public DateTime LeaseEndDate { get; set; }
        public bool IsArchived { get; set; } = false;
        public int UnitId { get; set; }

        public string FullNameAr =>
            string.Join(" ", new[] { FirstNameAr, SecondNameAr, ThirdNameAr, LastNameAr }
                .Where(n => !string.IsNullOrWhiteSpace(n)));

        public string FullNameEn =>
            string.Join(" ", new[] { FirstNameEn, SecondNameEn, ThirdNameEn, LastNameEn }
                .Where(n => !string.IsNullOrWhiteSpace(n)));
    }
}
