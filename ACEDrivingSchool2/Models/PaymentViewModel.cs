using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ACEDrivingSchool2.Models
{
    //holds data required to creat a custome payment details object
    public class PaymentViewModel
    {
        [Key]
        [Required]
        [MinLength(16)]
        [MaxLength(16)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Card Number must be numeric")]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }
        [Required]
        [MaxLength(80, ErrorMessage = "Name cannot be longer that 80 characters")]
        [MinLength(6, ErrorMessage = "Name must be longer that 6 characters")]
        [Display(Name = "Name On Card")]
        public string NameOnCard { get; set; }
        [Required]
        [MinLength(36, ErrorMessage = "Id cannot be shorter that 36 characters")]
        [MaxLength(36, ErrorMessage = "Id cannot be Longer that 36 characters")]
        public string BookingId { get; set; }
        public string Cost { get; set; }
    }
}