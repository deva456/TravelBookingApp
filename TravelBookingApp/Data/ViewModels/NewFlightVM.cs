using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TravelBooking.Models;

namespace TravelBooking.Data.ViewModels
{
    public class NewFlightVM
    {
        public int Id {get;set;}
        [Display(Name = "Airline")]
        
        public int Airline { get; set; }

        [Required(ErrorMessage = "Flight Number is required")]
        public string Number { get; set; }

        [Required(ErrorMessage = "Origin is required")]
        public int Origin { get; set; }

        [Required(ErrorMessage = "Destination is required")]
        
        public int Destination { get; set; }

        [Display(Name = "Departure Time")]
        [Required(ErrorMessage = "Departure time is required")]
        public TimeSpan DepartureTime { get; set; }

        [Display(Name = "Flight Duration")]
        [Required(ErrorMessage = "Flight duration is required")]
           
        [Range(01,1000000000000,ErrorMessage = "Flight duration must be greter than zero !")]
        public int FlightDurationMinutes { get; set; }

        [Display(Name = "Price")]
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01,100000000,ErrorMessage = "Price must be greter than zero !")]
        public decimal Price{ get; set; }

        public List<Airline>? AirlineList {get;set;}

        public List<Airport>? AirportList {get;set;}

    }
}
