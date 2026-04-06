namespace PurchaseSalesManagementSystem.Models
{
    public class Model_KatsuoIssueDate
    {
        public string? UserName { get; set; }
        public int? ID { get; set; }
        public DateTime? IssueDate { get; set; }

        public string IssueDateText =>
            IssueDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) ?? "";
    }
}
