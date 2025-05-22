using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Predobro.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be 0 or greater")]
        public int Quantity { get; set; }

        public string? ImageUrl { get; set; }

        // Foreign key to Store (User)
        [ForeignKey("Store")]
        [BindNever]
        public string StoreId { get; set; }

        [BindNever]
        public ApplicationUser Store { get; set; }
    }
}