using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ACEDrivingSchool2.Models;
using static ACEDrivingSchool2.Models.BookingsViewModels;
using System.Web.Security;
using System;
using System.Collections.Generic;
using DayPilot.Web.Mvc.Events.Calendar;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using Microsoft.AspNet.Identity;
using ACEDrivingSchool2.Extensions;
using static ACEDrivingSchool2.Models.Lesson;
using System.Collections;
using System.Threading.Tasks;

namespace ACEDrivingSchool2.Controllers
{
    public class BookingsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ListBookings
        //retrieves the appropriate bookings for the user currently signed in and returns the list bookings page
        public ActionResult ListBookings()
        {
            //shows all bookings in the database if an Admin or Receptionist is logged in
            if (User.IsInRole("Admin") || User.IsInRole("Receptionist"))
            {
                //retrieves all bookings in the database and orders them from most recently booked
                var bookings = db.Booking.OrderByDescending(b => b.DateTimeBooked).Select(BookingListViewModel.ViewModel);
                //returns view with the populated model
                return View(new ListBookingsViewModel() { Bookings = bookings });
            }

            //returns bookings associated with the user if a student is logged in
            if (User.IsInRole("Student"))
            {
                var userId = User.Identity.GetUserId();
                //retrieves all bookings Assigned to the student currently logged in from the database and orders them from most recently booked
                var bookings = db.Booking.OrderByDescending(b => b.DateTimeBooked).Where(b => b.StudentId == userId).Select(BookingListViewModel.ViewModel);
                //returns view with the populated model
                return View(new ListBookingsViewModel() { Bookings = bookings });
            }

            //returns all lessons that the user is assigned to if an Instructor is logged in
            if (User.IsInRole("Instructor"))
            {
                var userId = User.Identity.GetUserId();
                //retrieves all lessons assinged to the instructor currently logged in from the database and orders them from most recently booked
                var lessons = db.Lesson.OrderByDescending(l => l.Start).Where(l => l.InstructorId == userId).Select(LessonViewModels.LessonViewModel.ViewModel);

                BookingListViewModel b = new BookingListViewModel();
                b.Lessons = lessons.AsEnumerable();
                List<BookingListViewModel> bookings = new List<BookingListViewModel>();
                bookings.Add(b);
                //returns view with the populated model
                return View(new ListBookingsViewModel() { Bookings = bookings });
            }

            
            //returns user to homepage if they are not logged in
            return RedirectToAction("Index", "Home");
        }

        //only used by receptionist to search for a students bookings through their id
        [HttpPost]
        public ActionResult ListBookings(string studentId)
        {
            //ensures that a receptionist is logged in
            if (User.IsInRole("Receptionist"))
            {
                //retrieves all bookings from the database that are assinged to the student id passed in
                var bookings = db.Booking.OrderByDescending(b => b.DateTimeBooked).Where(b => b.StudentId == studentId).Select(BookingListViewModel.ViewModel);
                //returns view with the populated model
                if (bookings.Count() == 0)
                {
                    if (db.Users.Find(studentId) == null)
                    {
                        this.AddNotification("A student with that id does not exist, ensure you have entered the id correctly", NotificationType.ERROR);
                        return View();
                    }
                    else
                    {
                        this.AddNotification("That student has not made a booking yet", NotificationType.ERROR);
                        return View();
                    }
                }
                return View(new ListBookingsViewModel() { Bookings = bookings });
            }
            return RedirectToAction("ListBookings");
        }

        // GET: Bookings/Create
        //returns the make booking page
        public ActionResult MakeBooking()
        {
            MakeBookingViewModel booking = new MakeBookingViewModel();
            return View(booking);
        }

        // POST: Bookings/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        //handles creating a booking when the user submits the make booking form
        public ActionResult MakeBooking([Bind(Include = "Lessons, StudentId")]Booking model)
        {
            Booking booking = new Booking();

            if (ModelState.IsValid)
            {
                //trys to create a booking from the data passed in from the model and add it to the database
                try
                {
                    //retrieves the lessons from session variable
                    var b = Session["Booking"] as Booking;
                    booking = b;
                    booking.StudentId = model.StudentId;

                    if (booking != null && booking.id != null)
                    {
                        db.Entry(booking).State = EntityState.Modified;
                    }

                    //sets up the details of the booking before adding it to the database
                    booking.DateTimeBooked = DateTime.Now;
                    booking.Status = "UnPaid";

                    //sets the student id to the user currently logged in if a student id was not passed in
                    if (booking.StudentId == "")
                    {
                        booking.StudentId = User.Identity.GetUserId();
                        booking.studentName = db.Users.Find(booking.StudentId).Name;
                    }
                    else
                    {
                        booking.studentName = db.Users.Where(u => u.Id.ToString() == booking.StudentId).Select(u => u.Name).First();
                    }

                    //checks if the users email has been confirmed if not returns an error message
                    if (!db.Users.Find(booking.StudentId).EmailConfirmed)
                    {
                        this.AddNotification("You must confirm your account before you may create a booking, please check your email for the conformation link", NotificationType.ERROR);
                        return RedirectToAction("Index", "Home");
                    }

                    db.Booking.Add(booking);

                    List<Lesson> failed = new List<Lesson>();

                    //sets the details of each lesson then adds it to the database
                    foreach (Lesson l in booking.Lessons)
                    {
                        var lesson = l;
                        l.BookingId = booking.id;
                        l.findInstrutor();
                        booking.TotalCost = booking.TotalCost + l.LessonCost;

                        if (l.InstructorId == "0")
                        {
                            failed.Add(l);
                        }
                        db.Lesson.Add(lesson);
                    }

                    //returns an error message if an instructor was not able to be assigned to a lesson showing the date and time of the lesson so the user knows which lesson could not be booked
                    if (failed.Count > 0)
                    {
                        string error = "";
                        foreach (var l in failed)
                        {
                            error = error + l.Start.ToString() + " - " + l.End.ToString() + " ";
                        }

                        this.AddNotification("please select another date or time for these lessons as these time slots are currently full " + error, NotificationType.ERROR);
                        Session["Booking"] = null;
                        return RedirectToAction("Index", "Home");
                    }
                }



                //if creating a booking from the data passed in from the model and adding it to the database fails returns an error message
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.StackTrace);
                    this.AddNotification("Booking Failed Ensure that your have selected a lesson", NotificationType.ERROR);
                    if (User.IsInRole("Receptionist"))
                    {
                        this.AddNotification(("Also Ensure That " + model.StudentId + " Is A Valid Student Id"), NotificationType.ERROR);
                    }
                    return RedirectToAction("Index", "Home");
                }

                //trys to save the changes to the database and reset the session variable
                try
                {
                    db.SaveChanges();
                    Session["Booking"] = null;
                }

                //if the booking and lessons cannot be saved to the database returns an error message
                catch (DbEntityValidationException ex)
                {
                    foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                    {
                        //retrieves the entry

                        DbEntityEntry entry = item.Entry;
                        string entityTypeName = entry.Entity.GetType().Name;

                        //creates and prints an error message to make debugging easier

                        foreach (DbValidationError subItem in item.ValidationErrors)
                        {
                            string message = string.Format("Error '{0}' occurred in {1} at {2}",
                                        subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                            System.Diagnostics.Debug.WriteLine(message);
                        }
                    }
                    //displays an error message and returns to the home page
                    this.AddNotification("booking lessons failed please try again later", NotificationType.ERROR);
                    return RedirectToAction("Index", "Home");
                }

                //returns a success message if everything ran successfully and redirects the user to the payment page
                this.AddNotification("Booking Create Successfully", NotificationType.SUCCESS);
                PaymentViewModel card = new PaymentViewModel();
                card.BookingId = booking.id;
                card.Cost = booking.TotalCost.ToString();
                return RedirectToAction("Payment", new { cost = card.Cost, id = card.BookingId});
                }
            
            //returns an error message and takes the user to the homepage if the model passed in is not valid
            this.AddNotification("Booking failed", NotificationType.ERROR);
            return RedirectToAction("Index", "Home");
        }
    
        //returns a partial view that is used with an ajax call to show the lessons of each booking
        public ActionResult BookingLessonsById(string id)
        {
            //if the user is not an instructor returns all lessons contains in the booking with the id that id passed in
            if (id != "Instructor" && id != null)
            {
                var bookingLessons = db.Booking.Where(b => b.id == id).Select(BookingsViewModels.BookingListViewModel.ViewModel).FirstOrDefault();
                return PartialView("_BookingsLessons", bookingLessons);
            }

            //Instructors shouldnt see a students bookings so this returns all lessons assigned to the current instructor for the next 2 weeks
            else
            {
                try
                {
                    var instId = User.Identity.GetUserId();
                    var dayLimit = DateTime.Now.AddDays(14);
                    var lessons = db.Lesson.Where(l => l.InstructorId == instId).Where(l => l.Start > DateTime.Now).Where(l => l.Start < dayLimit).Select(LessonViewModels.LessonViewModel.ViewModel);
                    List<LessonViewModels.LessonViewModel> updatedLessons = new List<LessonViewModels.LessonViewModel>();
                    foreach (var l in lessons)
                    {
                        //finds the details of the lesson
                        var lessonBookingId = db.Lesson.Where(dbl => dbl.End == l.End && dbl.Start == l.Start && dbl.InstructorName == l.InstructorName && dbl.LessonCost == l.LessonCost && dbl.Type == l.Type).Select(dbl => dbl.BookingId);
                        bool bookingPaid = (db.Booking.Find(lessonBookingId.First()).Status == "Paid");
                        var studentName = db.Booking.Find(lessonBookingId.First()).studentName;
                        l.StudentName = studentName;
                        //only displays lessons that have been paid for to the instructor
                        if (bookingPaid)
                        {
                            updatedLessons.Add(l);
                        }

                    }
                    //returns the partial view
                    BookingListViewModel bookingLessons = new BookingListViewModel();
                    bookingLessons.Lessons = updatedLessons.AsQueryable();
                    return PartialView("_BookingsLessons", bookingLessons);
                }
                catch
                {
                    this.AddNotification("You do not currently have any lessons assigned to you", NotificationType.INFO);
                    return RedirectToAction("Index", "Home");
                }
            }


                                 
        }

        //Get: MakePayment
        //retrieves the payment page
        public ActionResult Payment(double cost, string id)
        {
            PaymentViewModel pay = new PaymentViewModel();
            pay.BookingId = id;
            pay.Cost = cost.ToString();
            return View("MakePayment", pay);
        }

        [HttpPost]
        //verifies the details entered and stores the details in the database
        public async Task<ActionResult> MakePayment(PaymentViewModel model)
        {
            if (ModelState.IsValid)
            {
                //checks that the data passed in is not empty
                if (model.CardNumber != null && model.NameOnCard != null && model.BookingId != null && model.Cost != null)
                {
                    //creates a payment detail object to store the users payment details
                    var userId = User.Identity.GetUserId();
                    CustomerPaymentDetails paymentDetails = new CustomerPaymentDetails();
                    paymentDetails.CardNumber = model.CardNumber;
                    paymentDetails.NameOnCard = model.NameOnCard;
                    paymentDetails.UserId = userId;

                    //retrieves the booking being paid for from the database and updates it
                    Booking booking = new Booking();
                    booking = db.Booking.Where(b => b.id == model.BookingId).First();
                    booking.Status = "Paid";
                    booking.Lessons = await db.Lesson.Where(l => l.BookingId == booking.id).ToListAsync();
                    //checks if the payment details are already in the database if not adds them to the database
                    if ((db.CustomerPaymentDetails.Find(paymentDetails.CardNumber) == null))
                    {
                        db.CustomerPaymentDetails.Add(paymentDetails);
                        db.Entry(paymentDetails).State = EntityState.Added;
                    }
                    //if the payment details exist in the database ensures that the users name matchs the name stored in the database
                    else
                    {
                        var name = db.CustomerPaymentDetails.Find(paymentDetails.CardNumber).NameOnCard;
                        //returns an appropriate error message if the names do not match
                        if ((name != paymentDetails.NameOnCard))
                        {
                            this.AddNotification("Payment Failed Name on card did not match our records", NotificationType.ERROR);
                            return RedirectToAction("Index", "Home");
                        }
                        var id = db.CustomerPaymentDetails.Find(paymentDetails.CardNumber).UserId;
                        if (id != paymentDetails.UserId)
                        {
                            this.AddNotification("Sorry but this card belongs to another user", NotificationType.ERROR);
                            return RedirectToAction("Index", "Home");
                        }

                    }
                    //saves changes to the database, returns the user to the home page and displays a success message
                    db.Entry(booking).State = EntityState.Modified;
                    db.SaveChanges();
                    this.AddNotification("Payment for booking " + model.BookingId + " processed succesfully", NotificationType.SUCCESS);
                    return RedirectToAction("Index", "Home");
                }
            }

            //returns the user to the home page with an error message if the model is not valid
            this.AddNotification("Payment for booking " + model.BookingId + " was unable to be processed succesfully, please try again later. You may continute your payment through view your bookings", NotificationType.ERROR);
            return RedirectToAction("Index", "Home");
        }

        //cancels the boking with the id passed in

        public ActionResult Cancel(string id)
        {
            Booking booking = db.Booking.Find(id);
            booking.Status = "Cancelled";
            db.SaveChanges();
            this.AddNotification("Booking Cancelled successfully", NotificationType.SUCCESS);
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
