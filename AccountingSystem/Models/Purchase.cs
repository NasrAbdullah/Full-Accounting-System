using System;
using System.Collections.Generic;

namespace AccountingSystem.Models
{
    public class Purchase
    {
        public int PurchaseId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal Paid { get; set; }
        public decimal Remaining { get; set; }
        public string PaymentMethod { get; set; }
        public string Notes { get; set; }
        public int? UserId { get; set; }
        public List<PurchaseItem> Items { get; set; }

        public Purchase()
        {
            InvoiceNumber = "";
            PurchaseDate = DateTime.Now;
            Items = new List<PurchaseItem>();
        }
    }

    public class PurchaseItem
    {
        public int PurchaseItemId { get; set; }
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
    }
}