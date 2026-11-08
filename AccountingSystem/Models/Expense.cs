using System;

namespace AccountingSystem.Models
{
    public class Expense
    {
        public int ExpenseId { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Notes { get; set; }

        public Expense()
        {
            Category = "";
            ExpenseDate = DateTime.Now;
        }
    }
}