using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ACEDrivingSchool2.Models;
using ACEDrivingSchool2.Extensions;


namespace ACEDrivingSchool2.Controllers
{
    public class LessonsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Booking booking = new Booking();

        //GET: List
        //retrieves the list view
        public ActionResult List()
        {
            if (ViewData["SelectedLessons"] != null)
            {
                List<Lesson> lessons = ViewData["SelectedLessons"] as List<Lesson>;
                return View(lessons);
            }
            return View();
        }

        //used to validate the timeslot and duration a user selects and sets up a lesson view model to store the data in
        public ActionResult Create(DateTime start, DateTime end)
        {
            //validates the timeslot has not already past
            if (start < DateTime.Now)
            {
                ViewBag.Reload = true;
                ViewBag.ReloadLesson = true;
                this.AddNotification("Sorry but you are not able to book a lesson in the past", NotificationType.ERROR);
                return View();
            }

            //validates the timeslot is during opening hours
            if (!(start.Hour >= 9 && end.Hour <= 20))
            {
                ViewBag.Reload = true;
                ViewBag.ReloadLesson = true;
                this.AddNotification("Sorry but we only offer lessons between 9AM and 7PM, please select another time", NotificationType.ERROR);
                return View();
            }

            //validates that the timeslot is not longer than 2 hours
            if ((end.Hour - start.Hour) > 2)
            {
                ViewBag.Reload = true;
                ViewBag.ReloadLesson = true;
                this.AddNotification("Sorry but we only offer lessons with a max duration of 2 hour(s)", NotificationType.ERROR);
                return View();
            }

            LessonViewModels.LessonViewModel lesson = new LessonViewModels.LessonViewModel();

            lesson.Start = start;
            lesson.End = end;
            //sets the cost of the lesson from how long the lesson will last
            if ((lesson.End.Hour - lesson.Start.Hour) == 1)
            {
                lesson.LessonCost = 20.00;
            }
            else if ((lesson.End.Hour - lesson.Start.Hour) == 2)
            {
                lesson.LessonCost = 40.00;
            }


            //returns the create view with the generated model so the user can validate the details of their lesson and also select if they are booking a test
            return View("Create", lesson);
        }


        // POST: Lessons/Create
        //creates the lesson and adds it to a booking stored in a session variable
        [HttpPost]
        public ActionResult Create(LessonViewModels.LessonViewModel lessonVM)
        {
            Lesson lesson = new Lesson();
            if (ModelState.IsValid)
            {
                //validates that if the user would like to book a test that it is not longer than 1 hour
                if ((lessonVM.End.Hour - lessonVM.Start.Hour) > 1 && lessonVM.Test)
                {
                    ViewBag.Reload = true;
                    ViewBag.ReloadLesson = true;
                    this.AddNotification("Sorry but tests cannot be booked for longer than 1 hour", NotificationType.ERROR);
                    return View();
                }
                
                //adds the lesson to the session if the session already exists
                if (Session["Booking"] != null)
                {
                    if (lessonVM.Start.Hour >= 9 && lessonVM.End.Hour <= 20)
                    {
                        lesson = lesson.calculateCost(lessonVM);

                        lesson.Start = lessonVM.Start;
                        lesson.End = lessonVM.End;
                        Booking booking = Session["Booking"] as Booking;
                        booking.AddLesson(lesson);
                        Session["Booking"] = booking;
                    }

                }
                //creates a session variable and adds the lesson to the session
                else
                {
                    if (lessonVM.Start.Hour >= 9 && lessonVM.End.Hour <= 20)
                    {
                        lesson = lesson.calculateCost(lessonVM);

                        //creates the lesson object to add to the lessons stored in the booking object in the session variable
                        lesson.Start = lessonVM.Start;
                        lesson.End = lessonVM.End;
                        Booking booking = new Booking();
                        booking.AddLesson(lesson);
                        Session["Booking"] = booking;
                    }


                    
                }

                //used to close the create partial and reload the make booking page to update the selected lessons partial
                ViewBag.Reload = true;
                ViewBag.ReloadLesson = true;

                return View();
            }

            return View(lesson);
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
