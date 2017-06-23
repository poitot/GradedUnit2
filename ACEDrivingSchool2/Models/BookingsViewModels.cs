using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using static ACEDrivingSchool2.Models.LessonViewModels;

namespace ACEDrivingSchool2.Models
{
    public class BookingsViewModels
    {
        //holds data required for making a booking
        public class MakeBookingViewModel
        {
            [Required]
            public List<LessonViewModel> Lessons { get; set; }
            [Required]
            //validates the id is 36 characters long
            [MinLength(36, ErrorMessage = "Id cannot be shorter that 36 characters")]
            [MaxLength(36, ErrorMessage = "Id cannot be Longer that 36 characters")]
            public string StudentId { get; set; }

        }

        //holds data required for displaying bookings to users
        public class BookingListViewModel
        {
            public String Id { get; set; }
            public double TotalCost { get; set; }
            public IEnumerable<LessonViewModel> Lessons { get; set; }
            [Required]
            [DataType(DataType.DateTime)]
            public DateTime DateBooked { get; set; }
            public string Status { get; set; }
            [Required]
            //validates the id is 36 characters long
            [MinLength(36, ErrorMessage = "Id cannot be shorter that 36 characters")]
            [MaxLength(36, ErrorMessage = "Id cannot be Longer that 36 characters")]
            public string StudentId { get; set; }
            [DataType(DataType.Text)]
            [MaxLength(80, ErrorMessage = "Name cannot be longer that 80 characters")]
            [MinLength(6, ErrorMessage = "Name must be longer that 6 characters")]
            public string StudentName { get; set; }

            //converts a booking into a booking list view model
            public static Expression<Func<Booking, BookingListViewModel>> ViewModel
            {
                get
                {
                    return b => new BookingListViewModel()
                    {
                        Id = b.id,
                        TotalCost = b.TotalCost,
                        Lessons = b.Lessons.AsQueryable().Select(LessonViewModel.ViewModel),
                        DateBooked = b.DateTimeBooked,
                        Status = b.Status,
                        StudentId = b.StudentId,
                        StudentName = b.studentName
                };
                }
            }


        }

    }
}