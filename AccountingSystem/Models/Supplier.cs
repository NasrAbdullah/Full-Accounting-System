using System;

namespace AccountingSystem.Models
{
    public class Supplier
    {
        public int SupplierId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public decimal OpeningBalance { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public decimal TotalPurchases { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Balance { get; set; }

        public Supplier()
        {
            Name = "";
            IsActive = true;
        }
    }
}