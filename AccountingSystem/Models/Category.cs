using System;

namespace AccountingSystem.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductCount { get; set; }

        public Category()
        {
            Name = "";
        }
    }
}