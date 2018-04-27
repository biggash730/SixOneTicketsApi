using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PianoBarApi.Models;

namespace PianoBarApi.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<Models.AppDbContext>
    {
        public UserManager<User> UserManager { get; private set; }

        public Configuration()
            : this(new UserManager<User>(new UserStore<User>(new AppDbContext())))
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        public Configuration(UserManager<User> userManager) { UserManager = userManager; }

        protected override void Seed(AppDbContext context)
        {
            #region Roles [Privileges]
            var roles = new List<IdentityRole>
                        {
                            new IdentityRole {Name = Privileges.CanViewDashboard},
                            new IdentityRole {Name = Privileges.CanViewReport},
                            new IdentityRole {Name = Privileges.CanViewSetting},
                            new IdentityRole {Name = Privileges.CanViewAdministration},
                            new IdentityRole {Name = Privileges.CanViewUser},
                            new IdentityRole {Name = Privileges.CanCreateUser},
                            new IdentityRole {Name = Privileges.CanUpdateUser},
                            new IdentityRole {Name = Privileges.CanDeleteUser},
                            new IdentityRole {Name = Privileges.CanViewRole},
                            new IdentityRole {Name = Privileges.CanCreateRole},
                            new IdentityRole {Name = Privileges.CanUpdateRole},
                            new IdentityRole {Name = Privileges.CanDeleteRole},
                            new IdentityRole {Name = Privileges.CanViewAttendants},
                            new IdentityRole {Name = Privileges.CanViewBar},
                            new IdentityRole {Name = Privileges.CanViewTicketing},
                            new IdentityRole {Name = Privileges.CanViewKitchen},
                            new IdentityRole {Name = Privileges.CanAdjustStock},
                            new IdentityRole {Name = Privileges.HasStore},
                            new IdentityRole {Name = Privileges.CanViewCashier},
                            new IdentityRole {Name = Privileges.CanCancelOrder},
                        };

            roles.ForEach(r => context.Roles.AddOrUpdate(q => q.Name, r));
            var a = "";
            roles.ForEach(q => a += q.Name + ",");
            #endregion

            #region App Roles
            var adminProfile = new Profile
            {
                Name = "Administrator",
                Notes = "Administrator Role",
                Privileges = a.Trim(','),
                Locked = true
            };
            #endregion

            #region Users
            var userManager = new UserManager<User>(new UserStore<User>(context))
            {
                UserValidator = new UserValidator<User>(UserManager)
                {
                    AllowOnlyAlphanumericUserNames = false
                }
            };

            //Admin User
            if (UserManager.FindByNameAsync("admin").Result == null)
            {
                var res = userManager.CreateAsync(new User
                {
                    Name = "Administrator",
                    Profile = adminProfile,
                    UserName = "admin",
                    PhoneNumber = "",
                    City = "Accra",
                    DateOfBirth = DateTime.Now.AddYears(-30),
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    TokenVerified = true,
                    Locked = false,
                }, "admin@app");

                if (res.Result.Succeeded)
                {
                    var userId = userManager.FindByNameAsync("admin").Result.Id;
                    roles.ForEach(q => userManager.AddToRole(userId, q.Name));
                }
            }

            #endregion

            #region Update Roles
            roles.ForEach(q => context.Roles.AddOrUpdate(q));
            #endregion

            context.SaveChanges();
            base.Seed(context);
        }
    }
}
