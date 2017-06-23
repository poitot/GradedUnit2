using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ACEDrivingSchool2.Controllers
{
    public class HomeController : Controller
    {
        //returns the home page
        public ActionResult Index()
        {
            return View();
        }

    }
}