using System;

namespace AccountingSystem.Models
{
    public class StockMovement
    {
        public int StockMovementId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string MovementType { get; set; }
        public decimal Quantity { get; set; }
        public int? ReferenceId { get; set; }
        public string ReferenceType { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        public StockMovement()
        {
            MovementType = "";
        }
    }
}