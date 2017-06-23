using ACEDrivingSchool2.Extensions;
using ACEDrivingSchool2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ACEDrivingSchool2.Controllers
{
    public class AdminController : AccountController
    {

        // GET: Admin/Create
        //returns the create user view which allows an admin to create a user with any role
        public ActionResult Create()
        {
            return View("CreateUser");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //creates a new user from the model passed in
        public async Task<ActionResult> Create(RegisterViewModel model)
        {
            //used to verify if the user was create and added to the database or not
            var created = false;
            ApplicationDbContext db = new ApplicationDbContext();
            //uses the model to set the details of the appication user object to store in database
            var user = new ApplicationUser
            {
                Email = model.Email,
                Name = model.Name,
                UserName = model.Email,
                DateOfBirth = model.DateOfBirth,
                DrivingLicence = model.DrivingLicence,
                PhoneNumber = model.HomePhone,
                MobilePhone = model.MobilePhone,
                Address = model.Address
            };
            //check if the role passed in exists in the database
            foreach (var r in db.Roles)
            {
                if (r.Name == model.Role)
                {
                    //creates the user if the role exists in the database
                    var result = await UserManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        //if the user was successfully created and added to the database, adds the user to the role passed in and sets created to true to confirm everything ran successfully
                        await UserManager.AddToRoleAsync(user.Id, model.Role);
                        created = true;
                    }
                }
            }

            //creates a notificaiton to display based on if the user was created successfully or not
            if (created)
            {
                this.AddNotification(model.Role + " User Has been created", NotificationType.INFO);
            }
            else
            {
                    this.AddNotification("User Creation Failed", NotificationType.ERROR);
            }
            


            return RedirectToAction("Index", "Home");
        }
    }
}