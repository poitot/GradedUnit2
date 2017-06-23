using ACEDrivingSchool2.Models;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Enums;
using DayPilot.Web.Mvc.Events.Calendar;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static ACEDrivingSchool2.Models.BookingsViewModels;
using static ACEDrivingSchool2.Models.Lesson;

namespace ACEDrivingSchool2.Controllers
{
    public class CalendarController : Controller
    {
        //handles the settings of the calander such as if its weekly or monthly
        public ActionResult Backend()
        {
            return new Dpc().CallBack(this);
        }

        class Dpc : DayPilotCalendar
        {   
            ApplicationDbContext db = new ApplicationDbContext();
            //displays a welcome message
            protected override void OnInit(InitArgs e)
            {
                UpdateWithMessage("Welcome!", CallBackUpdateType.Full);
            }


            protected override void OnCommand(CommandArgs e)
            {
                switch (e.Command)
                {
                    //refreshes the calander after a time slot is selected
                    case "refresh":
                        Update();
                        break;

                        //handles chagning the date week displayed on the calander when the user uses the navigator
                    case "navigate":
                        StartDate = (DateTime)e.Data["start"];
                        Update(CallBackUpdateType.Full);
                        break;
                }
            }


        }



    }
}