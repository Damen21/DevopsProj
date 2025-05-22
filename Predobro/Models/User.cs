using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Predobro.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        // Add more custom properties as needed
    }
}