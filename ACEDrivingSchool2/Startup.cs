using ACEDrivingSchool2.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using System;

[assembly: OwinStartupAttribute(typeof(ACEDrivingSchool2.Startup))]
namespace ACEDrivingSchool2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            Seed();
        }

        private void Seed()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));


            // creates the default admin role and adds a default admin user to the role    
            if (!roleManager.RoleExists("Admin"))
            {

                // first we create Admin rool   
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Admin";
                roleManager.Create(role);

                //Here we create a Admin super user who will maintain the website                  

                var user = new ApplicationUser();
                user.UserName = "admin@admin.admin";
                user.Email = "admin@admin.admin";
                user.Name = "Default Admin";
                user.DrivingLicence = "12345678910";
                user.DateOfBirth = DateTime.Today;
                user.PhoneNumber = "11";
                user.MobilePhone = "22";

                string userPWD = "default";

                var chkUser = UserManager.Create(user, userPWD);

                //Add default User to Role Admin   
                if (chkUser.Succeeded)
                {
                    var result1 = UserManager.AddToRole(user.Id, "Admin");

                }
            }

            // creating isntructor role
            if (!roleManager.RoleExists("Instructor"))
            {

                // creates the role for instructor   
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Instructor";
                roleManager.Create(role);

                // creates the default instructor that will be assinged lessons                  

                var user = new ApplicationUser();
                user.UserName = "Inst@Inst.Inst";
                user.Email = "Inst@Inst.Inst";
                user.Name = "Default Admin";
                user.DrivingLicence = "12345678910";
                user.DateOfBirth = DateTime.Today;
                user.PhoneNumber = "11";
                user.MobilePhone = "22";

                string userPWD = "default";

                var chkUser = UserManager.Create(user, userPWD);

                //Add default User to Role instructor   
                if (chkUser.Succeeded)
                {
                    var result1 = UserManager.AddToRole(user.Id, "Instructor");

                }
            }

            // creating Creating Manager role    
            if (!roleManager.RoleExists("Manager"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Manager";
                roleManager.Create(role);

                //Here we create a default Manager                

                var user = new ApplicationUser();
                user.UserName = "man@man.man";
                user.Email = "man@man.man";
                user.Name = "Default Manager";
                user.DrivingLicence = "12345678910";
                user.DateOfBirth = DateTime.Today;
                user.PhoneNumber = "11";
                user.MobilePhone = "22";

                string userPWD = "default";

                var chkUser = UserManager.Create(user, userPWD);

                //Add default Manager to Role Manager   
                if (chkUser.Succeeded)
                {
                    var result1 = UserManager.AddToRole(user.Id, "Manager");

                }
            }

            // creating the student role   
            if (!roleManager.RoleExists("Student"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Student";
                roleManager.Create(role);

                //Here we create a Admin super user who will maintain the website                  

                var user = new ApplicationUser();
                user.UserName = "stu@stu.stu";
                user.Email = "stu@stu.stu";
                user.Name = "Default Student";
                user.DrivingLicence = "12345678910";
                user.DateOfBirth = DateTime.Today;
                user.PhoneNumber = "11";
                user.MobilePhone = "22";

                string userPWD = "default";

                var chkUser = UserManager.Create(user, userPWD);

                //Add default Student to Role student   
                if (chkUser.Succeeded)
                {
                    var result1 = UserManager.AddToRole(user.Id, "Student");

                }
            }

            // creating the receptionist role
            if(!roleManager.RoleExists("Receptionist"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Receptionist";
                roleManager.Create(role);

                // creating a default receptionist that will be able to book, pay for bookings and cancel students bookings                 

                var user = new ApplicationUser();
                user.UserName = "re@re.re";
                user.Email = "re@re.re";
                user.Name = "Default Receptionist";
                user.DrivingLicence = "12345678910";
                user.DateOfBirth = DateTime.Today;
                user.PhoneNumber = "11";
                user.MobilePhone = "22";

                string userPWD = "default";

                var chkUser = UserManager.Create(user, userPWD);

                //Add default Receptionist to Role Receptionist   
                if (chkUser.Succeeded)
                {
                    var result1 = UserManager.AddToRole(user.Id, "Receptionist");

                }
            }
        }
    }
}
