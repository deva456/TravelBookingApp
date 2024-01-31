using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Data.ViewModels
{
    public class IdentityVM
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "User name is required")]
        [RegularExpression(@"^[a-zA-Z0-9-._@+]{1,100}$", ErrorMessage = "Unsupported character.")]
        public string UserName { get; set; }

        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Exactly 10 digit numbers allowed.")]
        public string PhoneNumber { get; set; }
    }
}