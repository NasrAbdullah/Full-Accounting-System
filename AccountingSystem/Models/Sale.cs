using System;
using System.Collections.Generic;

namespace AccountingSystem.Models
{
    public class Sale
    {
        public int SaleId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime SaleDate { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal Paid { get; set; }
        public decimal Remaining { get; set; }
        public string PaymentMethod { get; set; }
        public string Notes { get; set; }
        public int? UserId { get; set; }
        public List<SaleItem> Items { get; set; }

        public Sale()
        {
            InvoiceNumber = "";
            SaleDate = DateTime.Now;
            Items = new List<SaleItem>();
        }
    }

    public class SaleItem
    {
        public int SaleItemId { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public decimal SalePrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }

        public decimal Profit
        {
            get { return (SalePrice - PurchasePrice) * Quantity - Discount; }
        }
    }
}