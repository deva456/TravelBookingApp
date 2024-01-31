using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TravelBooking.Data.ViewModels
{
    public class EditAirportVM
    {
        public int Id{get;set;}
        [Display(Name = "Code")]
        [Required(ErrorMessage = "Airport code is required")]
        [RegularExpression(@"^[a-zA-Z0-9]{1,20}$", ErrorMessage = "Only Letters, numbers are allowed, Maximum 20 characters.")]
        public string Code { get; set; }

        [Display(Name = "City")]
        [Required(ErrorMessage = "City is required")]
           
        public string Name { get; set; }

        [Display(Name = "LatitudeValue")]
        [Required(ErrorMessage = "Latitude is required")]
        [Range(typeof(double),"-90.00","90.00", ErrorMessage = "Latitude Value should between -90 and 90")]
        public double LatitudeValue { get; set; }

        [Display(Name = "LatitudeDirection")]
        [Required(ErrorMessage = "LatitudeDirection is required")]
        public string LatitudeDirection { get; set; }

        [Display(Name = "ImageUrl")]
        public string? ImageUrl { get; set; }
        
        [Display(Name = "LongitudeValue")]
        [Required(ErrorMessage = "Longitude is required")]
        [Range(typeof(double),"-180.00","180.00", ErrorMessage = "Longitude Value should between -180 and 180")]
        public double LongitudeValue { get; set; }

        [Display(Name = "LongitudeDirection")]
        [Required(ErrorMessage = "LongitudeDirection is required")]
        public string LongitudeDirection { get; set; }

      
        [Display(Name = "Logo Picture")]  
        public IFormFile? LogoImage { get; set; }  

    }
}
