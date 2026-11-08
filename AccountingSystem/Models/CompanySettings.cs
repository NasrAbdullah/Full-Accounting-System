namespace AccountingSystem.Models
{
    public class CompanySettings
    {
        public int SettingId { get; set; }
        public string CompanyName { get; set; }
        public string LogoPath { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Currency { get; set; }
        public decimal TaxRate { get; set; }
        public string InvoiceFooter { get; set; }

        public CompanySettings()
        {
            SettingId = 1;
            CompanyName = "شركتي";
            Currency = "ر.ي";
        }
    }
}