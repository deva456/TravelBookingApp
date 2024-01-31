using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Models
{
    public class PurchasedFlight
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Purchase Purchase { get; set; }

        [Required]
        public DateTime DepartureDate { get; set; }

        [Required]
        public Flight Flight { get; set; }

        [Required]
        public decimal Price { get; set; }

    }
}