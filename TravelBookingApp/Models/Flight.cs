using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Models
{
    public class Flight
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Number { get; set; }

        [Required]
        public Airline Airline { get; set; }

        [Required]
        public Airport Origin { get; set; }

        [Required]
        public Airport Destination { get; set; }

        [Required]
        public TimeSpan DepartureTime { get; set; }

        [Required]
        public int FlightDurationMinutes { get; set; }

        [Required]
        public decimal Price { get; set; }

     

        
    }
}