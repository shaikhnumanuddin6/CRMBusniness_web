
using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessCRM.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(150)]
        [Display(Name = "Customer Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(150)]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
