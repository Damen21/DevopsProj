using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Predobro.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Add these two properties
        public bool IsSubmitted { get; set; } = false;
        public DateTime? SubmittedAt { get; set; }

        // Foreign key to Customer (User)
        [ForeignKey("Customer")]
        public string CustomerId { get; set; }
        public ApplicationUser Customer { get; set; }

        // Navigation property for order items
        public List<OrderItem> OrderItems { get; set; }
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }
        public int Quantity { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}