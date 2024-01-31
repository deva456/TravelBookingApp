using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Models
{
    public class PlannedFlight
    {
        [Key]
        public int Id { get; set; }

        public IdentityUser User { get; set; }

        [Required]
        public DateTime DepartureDate { get; set; }

        [Required]
        public Flight Flight { get; set; }
    }
}