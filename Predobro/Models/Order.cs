using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Predobro.Models
{
    public enum OrderItemStatus
    {
        Processing,
        ReadyForPickup,
        Completed
    }

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
        
        // Helper property to get overall order status
        public OrderItemStatus OverallStatus 
        { 
            get 
            {
                if (OrderItems == null || !OrderItems.Any()) 
                    return OrderItemStatus.Processing;
                
                if (OrderItems.All(oi => oi.Status == OrderItemStatus.Completed))
                    return OrderItemStatus.Completed;
                
                if (OrderItems.All(oi => oi.Status == OrderItemStatus.ReadyForPickup || oi.Status == OrderItemStatus.Completed))
                    return OrderItemStatus.ReadyForPickup;
                
                return OrderItemStatus.Processing;
            }
        }
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }
        public int Quantity { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        
        // Add status for each individual item
        public OrderItemStatus Status { get; set; } = OrderItemStatus.Processing;
    }
}