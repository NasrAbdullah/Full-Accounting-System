using System;

namespace AccountingSystem.Models
{
    public class Income
    {
        public int IncomeId { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime IncomeDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Notes { get; set; }

        public Income()
        {
            Category = "";
            IncomeDate = DateTime.Now;
        }
    }
}