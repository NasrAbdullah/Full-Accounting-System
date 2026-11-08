using System;

namespace AccountingSystem.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public int? CustomerId { get; set; }
        public int? SupplierId { get; set; }
        public int? SaleId { get; set; }
        public int? PurchaseId { get; set; }
        public string Notes { get; set; }

        public Payment()
        {
            PaymentDate = DateTime.Now;
        }
    }
}