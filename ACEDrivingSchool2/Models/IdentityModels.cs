using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Security;
using static ACEDrivingSchool2.Models.BookingsViewModels;
using static ACEDrivingSchool2.Models.LessonViewModels;

namespace ACEDrivingSchool2.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        //adds addition information required to user class and also to the database table
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string MobilePhone { get; set; }
        [Required]
        public string DrivingLicence { get; set; }
        [Required]
        public string Address { get; set; }


    }

    //creates the booking class, used to store details of an individual booking and the lessons associated with it
    public class Booking
    {
        [Key]
        //generates a unique id for each booking
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string id { get; set; }
        public string StudentId { get; set; }
        public string studentName { get; set; }
        public double TotalCost { get; set; }
        public DateTime DateTimeBooked { get; set; }
        public string Status { get; set; }
        public List<Lesson> Lessons { get; set; }

        //default constructor
        public Booking()
        {
            StudentId = "";
            TotalCost = 0;
            DateTimeBooked = DateTimeBooked.Date;
            Status = "";
            Lessons = new List<Lesson>();
        }

        //adds a lesson to the lessons stored in a booking
        public void AddLesson(Lesson l)
        {
            Lessons.Add(l);
        }
    }

    //creates the lesson class, used to store the details of an individual lesson
    public class Lesson
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string id { get; set; }
        public string BookingId { get; set; }
        public string Type { get; set; }
        public double LessonCost { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string InstructorId { get; set; }
        public string InstructorName { get; set; }

        //default constructor
        public Lesson()
        {
            Type = "";
            LessonCost = 0;
            Start = DateTime.Parse("1900-01-01T00:00:00.000");
            End = Start = DateTime.Parse("1900-01-01T00:00:00.000");
            InstructorId = "";
            InstructorName = "Default name";
        }

        //finds an available instructor for the lesson and adds their id and name to the lesson
        public void findInstrutor()
        {
            //creates a db context to access and query the database, also creates a user store and user manager to find available instructors
            ApplicationDbContext db = new ApplicationDbContext();
            var userStore = new UserStore<ApplicationUser>(db);
            var userManager = new UserManager<ApplicationUser>(userStore);
            List<ApplicationUser> instructors = new List<ApplicationUser>();
            List<Lesson> lessons = new List<Lesson>();

            //first gets all instructors from the database and adds them to a list
            foreach (var u in db.Users)
            {
                if (userManager.IsInRole(u.Id, "Instructor"))
                {
                    instructors.Add(u);
                }
            }
               
            //receives all lessons stored in the database and removes the instructor assigned to it if the lessons date and time overlaps the current lesson
            foreach (var l in db.Lesson)
            {
                if (l.Start >= Start && l.End <= End)
                {
                    lessons.Add(l);
                    var u = userManager.FindById(l.InstructorId);
                    instructors.Remove(u);
                }
            }
            
            //checks if the instructors list contains atleast one instructor if not returns 0 so that an error message may be displayed 
            if (instructors.Count != 0)
            {
                //sets the lessons instructor id and name to the first instructor in the instructors list
                InstructorId = instructors.First().Id;
                InstructorName = instructors.First().Name;
            }

            else
            {
                InstructorId = "0";
            }
        }

        //calculates the cost of a lesson from the type and duration
        public Lesson calculateCost(LessonViewModel lessonVM)
        {
            Lesson lesson = setType(lessonVM);

            if (lesson.Type == "Test")
            {
                lesson.LessonCost = 40.00;
            }
            else
            {
                if ((lessonVM.End.Hour - lessonVM.Start.Hour) == 1)
                {
                    lesson.LessonCost = 20.00;
                }
                else if ((lessonVM.End.Hour - lessonVM.Start.Hour) == 2)
                {
                    lesson.LessonCost = 40.00;
                }
            }

            return lesson;
        }

        //creates a lesson object and sets its type from a lesson view model then returns the lesson object
        public Lesson setType(LessonViewModel lessonVM)
        {
            Lesson lesson = new Lesson();

            if (lessonVM.Test)
            {
                lesson.Type = "Test";
            }
            else
            {
                lesson.Type = "Lesson";
            }

            return lesson;
        }
    }

    //creates the customer payment details class, used to store customers payment details
    public class CustomerPaymentDetails
    {
        [Key]
        public string CardNumber { get; set; }
        public string NameOnCard { get; set; }
        public string UserId { get; set; }

        //default constructor
        public CustomerPaymentDetails()
        {
            CardNumber = "0";
            NameOnCard = "First Last";
            UserId = "default";
        }
    }




    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        //creates tables in the database for the booking, lesson and customer payment details classes
        public System.Data.Entity.DbSet<ACEDrivingSchool2.Models.Booking> Booking { get; set; }
        public System.Data.Entity.DbSet<ACEDrivingSchool2.Models.Lesson> Lesson { get; set; }
        public System.Data.Entity.DbSet<ACEDrivingSchool2.Models.CustomerPaymentDetails> CustomerPaymentDetails { get; set; }

    }
}
