using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static ACEDrivingSchool2.Models.BookingsViewModels;

namespace ACEDrivingSchool2.Models
{
    //holds the list of bookings to display a users bookings
    public class ListBookingsViewModel
    {
        public IEnumerable<BookingListViewModel> Bookings { get; set; }
    }
}