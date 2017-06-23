using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ACEDrivingSchool2.Models
{
    //holds data from the login view
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    //holds data from the register view
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        //validates the users name is at least 6 characters long and no longer than 80
        [MaxLength(80, ErrorMessage = "Name cannot be longer that 80 characters")]
        [MinLength(6, ErrorMessage = "Name must be longer that 6 characters")]
        public string Name { get; set; }

        [Required]
        //validates the email field contains an email address
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        //validates that the password is at least 6 characters long
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        //validates that the passwords entered match
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date Of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Home Phone Number")]
        //validates the phone number only contains numerical characters
        [RegularExpression("^[0-9]*$", ErrorMessage = "Phone Number must be numeric")]
        [MaxLength(10, ErrorMessage = "Phone number cannot be longer that 10 characters")]
        [MinLength(6, ErrorMessage = "Phone number cannot be shorter that 6 characters")]
        public string HomePhone { get; set; }

        [Required]
        [Display(Name = "Mobile Phone Number")]
        //validates the phone number only contains numerical characters
        [RegularExpression("^[0-9]*$", ErrorMessage = "Mobile Number must be numeric")]
        public string MobilePhone { get; set; }

        [Required]
        //validates that the driving licence number is 16 characters long
        [MaxLength(16, ErrorMessage = "Driving licence numbers cannot be more than 16 characters long")]
        [MinLength(16, ErrorMessage = "Driving licence numbers cannot be less than 16 characters long")]
        [Display(Name = "Driving Licence Number")]
        public string DrivingLicence { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }
       
        public RegisterViewModel()
        {
            Name = "";
            Email = "";
            Password = "";
            ConfirmPassword = "";
            HomePhone = "";
            MobilePhone = "";
            DrivingLicence = "";
            Role = "Student";
            Address = "";
        }
    }
}
