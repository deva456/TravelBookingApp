using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Models
{
    public class Purchase
    {
        [Key]
        public int Id { get; set; }

        public IdentityUser User { get; set; }

        [Required]
        public string PurchaseConfirmationCode { get; set; }

        [Required]
        public DateTime WhenPaid { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }
    }
}