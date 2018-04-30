using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using SixOneTikitsApi.AxHelpers;
using SixOneTikitsApi.DataAccess.Repositories;
using SixOneTikitsApi.Extensions;
using SixOneTikitsApi.Models;
using WebGrease.Css.Extensions;

namespace SixOneTikitsApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/account")]
    public class AccountController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;

        private readonly UserRepository _userRepo = new UserRepository();


        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }
        public AccountController()
            : this(new UserManager<User>(new UserStore<User>(new AppDbContext())))
        {
        }

        public AccountController(UserManager<User> userManager)
        {
            UserManager = userManager;
            UserManager.UserValidator = new UserValidator<User>(UserManager)
            {
                AllowOnlyAlphanumericUserNames =
                    false
            };
        }

        public AccountController(UserManager<User> userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public UserManager<User> UserManager { get; private set; }



        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

       

        

        // GET security/signin
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultObj> Login(LoginModel model)
        {
            try
            {
                if (!ModelState.IsValid) throw new Exception("Please check the login details");

                var user = await UserManager.FindAsync(model.UserName, model.Password);

                if (user == null) throw new Exception("Invalid Username or Password");

                if (!user.TokenVerified) throw new Exception("Your account is not verified");

                var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
                authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = model.RememberMe }, identity);

                var ticket = new AuthenticationTicket(identity, new AuthenticationProperties());
                var token = Startup.OAuthOptions.AccessTokenFormat.Protect(ticket);
                

                var data = new
                {
                    user.Id,
                    Username = user.UserName,
                    user.Name,
                    user.PhoneNumber,
                    user.DateOfBirth,
                    user.City,
                    Role = new
                    {
                        user.Profile.Id,
                        user.Profile.Name,
                        Privileges = user.Profile.Privileges.Split(',')
                    },
                    Token = token
                };

                return WebHelpers.BuildResponse(data, "Login Successfull", true, 0);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        // POST api/Account/Logout
        [HttpGet]
        [Route("Logout")]
        public ResultObj Logout()
        {
            var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return WebHelpers.BuildResponse(new { }, "User Logged Out", true, 0);
        }

        [Route("GetUsers")]
        public ResultObj GetUsers()
        {
            try
            {
                var db = new AppDbContext();
                var data = db.Users.ToList()
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.DateOfBirth,
                        x.PhoneNumber,
                        Username = x.UserName,
                        x.IdNumber,
                        x.IdExpiryDate,
                        x.ResidentialAddress,
                        x.City,
                        x.Email,
                        x.CreatedAt,
                        RoleId = x.ProfileId,
                        Role = new { x.Profile.Id, x.Profile.Name }
                    }).ToList();
                return WebHelpers.BuildResponse(data, "", true, data.Count);
            }
            catch (Exception exception)
            {
                return WebHelpers.ProcessException(exception);
            }
        }
        [HttpGet]
        [Route("getuserdetails")]
        public ResultObj GetUserDetails(string id)
        {
            try
            {
                var db = new AppDbContext();
                var data = db.Users.Where(x => x.Id == id).ToList()
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.DateOfBirth,
                        x.PhoneNumber,
                        Username = x.UserName,
                        x.IdNumber,
                        x.IdExpiryDate,
                        x.ResidentialAddress,
                        x.City,
                        x.Email,
                        x.CreatedAt,
                    }).FirstOrDefault();
                return WebHelpers.BuildResponse(data, "Successful", true, 1);
            }
            catch (Exception exception)
            {
                return WebHelpers.ProcessException(exception);
            }
        }

        //[Authorize]
        [HttpGet]
        [Route("GetRoles")]
        public ResultObj GetRoles()
        {
            ResultObj results;
            try
            {
                var data = new AppDbContext().Roles.Select(x => x.Name).ToList();
                results = WebHelpers.BuildResponse(data, "", true, data.Count());
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [Route("customersignup")]
        [AllowAnonymous]
        public async Task<ResultObj> CustomerSignUp(User model)
        {
            try
            {
                var db = new AppDbContext();
                if (!ModelState.IsValid) return WebHelpers.ProcessException(ModelState.Values);
                var role = new ProfileRepository().GetProfileByName("Customer");

                if (role == null) throw new Exception("Please check the profile Id");

                var token = MessageHelpers.GenerateRandomNumber(6);

                var user = new User
                {
                    UserName = model.PhoneNumber,
                    PhoneNumber = model.PhoneNumber,
                    ProfileId = role.Id,
                    Name = model.Name,
                    City = model.City,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow,
                    Token = token
                };

                var identityResult = await UserManager.CreateAsync(user, model.Password);
                if (!identityResult.Succeeded) return WebHelpers.ProcessException(identityResult);


                //Add Roles in selected Role to user
                if (!string.IsNullOrEmpty(role.Privileges))
                {
                    role.Privileges.Split(',').ForEach(r => UserManager.AddToRole(user.Id, r.Trim()));
                }

                //send the token
                var msg = new Message
                {
                    Text =
                        $"Hello {model.Name}, Thank you for signing up to Piano Bar. Your verification code is {token}. Enjoy!!",
                    Subject = "Verification Token",
                    Recipient = user.PhoneNumber,
                    TimeStamp = DateTime.Now
                };
                db.Messages.Add(msg);
                db.SaveChanges();

                return WebHelpers.BuildResponse(null, "Registration Successful, Please verify your account.", true, 1);

            }
            catch (Exception ex)
            {
                return WebHelpers.ProcessException(ex);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("resetpassword")]
        public ResultObj ResetPassword(ResetPasswordModel model)
        {
            try
            {
                if (!ModelState.IsValid) return WebHelpers.ProcessException(ModelState.Values);

                var user = UserManager.FindByName(model.UserName);
                if (user == null) throw new Exception("Please check the username.");


                //reset old passwords
                var res = UserManager.RemovePassword(user.Id);
                if (!res.Succeeded) return WebHelpers.ProcessException(res);
                var result = UserManager.AddPassword(user.Id, model.NewPassword);
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First());
                }
                return !result.Succeeded
                    ? WebHelpers.ProcessException(result)
                    : WebHelpers.BuildResponse(model, "Password reset was sucessful.", true, 1);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("resetrequest")]
        public ResultObj ResetRequest(string phoneNumber)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var existing = db.Users.FirstOrDefault(x => x.UserName == phoneNumber && !x.Hidden && !x.Locked);
                    if (existing == null) throw new Exception("Sorry phone number is not valid. Enjoy!!");
                    //deactivate all other requests
                    var resReqs = db.ResetRequests.Where(x => x.PhoneNumber == phoneNumber && x.IsActive).ToList();
                    foreach (var r in resReqs)
                    {
                        r.IsActive = false;
                    }
                    db.SaveChanges();
                    var newRecord = new ResetRequest
                    {
                        PhoneNumber = phoneNumber,
                        Token = MessageHelpers.GenerateRandomNumber(6),
                        Date = DateTime.Now,
                        Ip = Request.Headers.Referrer.AbsoluteUri,
                        IsActive = true
                    };
                    db.ResetRequests.Add(newRecord);
                    db.SaveChanges();

                    // create a password reset entry
                    var msg = new Message
                    {
                        Text = $"You have requested to reset your Piano Bar Password. Your reset token is {newRecord.Token}. Please ignore this message if you did not request a password reset.",
                        Subject = "Password Reset",
                        Recipient = newRecord.PhoneNumber,
                        TimeStamp = DateTime.Now
                    };
                    db.Messages.Add(msg);
                    db.SaveChanges();

                    return WebHelpers.BuildResponse(null, "Password reset token has been sent to your phone.", true, 1);
                }
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("verify")]
        public ResultObj VerifyAccount(VerifyModel rm)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var user = db.Users.FirstOrDefault(x => x.UserName == rm.PhoneNumber);
                    if (user == null) throw new Exception("Please check your phone number");
                    if (user.TokenVerified) throw new Exception("Your account has already been verified. Please login with your phone number and password.");
                    if (user.Token != rm.Code) throw new Exception("Please check your token");

                    user.TokenVerified = true;
                    user.ModifiedAt = DateTime.Now;
                    db.SaveChanges();
                    return WebHelpers.BuildResponse(null, "Account has been verified successfully", true, 1);
                }
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("resendcode")]
        public ResultObj ResendCode(VerifyModel rm)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var user = db.Users.FirstOrDefault(x => x.UserName == rm.PhoneNumber);
                    if (user == null) throw new Exception("Please check your phone number");
                    //resend the token
                    var msg = new Message
                    {
                        Text =
                            $"Hello {user.Name}, Your verification code is {user.Token}. Enjoy!!",
                        Subject = "Verification Token",
                        Recipient = user.PhoneNumber,
                        TimeStamp = DateTime.Now
                    };
                    db.Messages.Add(msg);
                    db.SaveChanges();
                    return WebHelpers.BuildResponse(null, "Code Resent Successfully", true, 1);
                }
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        // POST api/Account/Register
        [Route("CreateUser")]
        public async Task<ResultObj> CreateUser(User model)
        {
            try
            {
                var db = new AppDbContext();
                if (!ModelState.IsValid) return WebHelpers.ProcessException(ModelState.Values);
                var role = new ProfileRepository().Get(model.RoleId);

                var token = MessageHelpers.GenerateRandomNumber(6);

                var user = new User
                {
                    UserName = model.PhoneNumber,
                    PhoneNumber = model.PhoneNumber,
                    ProfileId = role.Id,
                    Name = model.Name,
                    City = model.City,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow,
                    Email = model.Email,
                    DateOfBirth = model.DateOfBirth,
                    Token = token,
                    TokenVerified = true
                };

                var identityResult = await UserManager.CreateAsync(user, model.Password);
                if (!identityResult.Succeeded) return WebHelpers.ProcessException(identityResult);

                //Add Roles in selected Role to user
                if (!string.IsNullOrEmpty(role.Privileges))
                {
                    role.Privileges.Split(',').ForEach(r => UserManager.AddToRole(user.Id, r.Trim()));
                }

                //send the token
                var msg = new Message
                {
                    Text = 
                        $"Hello {model.Name}, Thank you for joining Piano Bar. Your verification code is {token}. Enjoy!!",
                    Subject = "Verification Token",
                    Recipient = user.PhoneNumber,
                    TimeStamp = DateTime.Now
                };
                db.Messages.Add(msg);
                db.SaveChanges();

                return WebHelpers.BuildResponse(user, "User Created Successfully", true, 1);

            }
            catch (Exception ex)
            {
                return WebHelpers.ProcessException(ex);
            }
        }

        [HttpPut]
        public ResultObj UpdateUser(User model)
        {
            try
            {
                if (!ModelState.IsValid) return WebHelpers.ProcessException(ModelState.Values);

                var user = _userRepo.Get(model.UserName);
                var role = new ProfileRepository().Get(model.ProfileId);

                if (user == null) return WebHelpers.BuildResponse(null, "Updating user not found. Please update an existing user", false, 0);
                var oldRoles = user.Profile.Privileges.Split(',');

                user.ProfileId = role.Id;
                user.Name = model.Name;
                user.DateOfBirth = model.DateOfBirth;
                user.IdExpiryDate = model.IdExpiryDate;
                user.IdNumber = model.IdNumber;
                user.City = model.City;
                user.ResidentialAddress = model.ResidentialAddress;
                user.PhoneNumber = model.PhoneNumber;
                user.UserName = model.PhoneNumber;
                user.Email = model.Email;
                user.ModifiedAt = DateTime.UtcNow;
                
                _userRepo.Update(user);

                //Remove old reles
                oldRoles.ForEach(x => UserManager.RemoveFromRole(user.Id, x));

                //Add Roles in selected Role to user
                if (!string.IsNullOrEmpty(role.Privileges))
                {
                    role.Privileges.Split(',').ForEach(r => UserManager.AddToRole(user.Id, r.Trim()));
                }

                //Change Password
                if (!string.IsNullOrEmpty(model.Password))
                {
                    UserManager.RemovePassword(user.Id);
                    UserManager.AddPassword(user.Id, model.Password);
                }

                return WebHelpers.BuildResponse(user.Id, "User Updated Successfully", true, 1);

            }
            catch (Exception ex)
            {
                return WebHelpers.ProcessException(ex);
            }
        }

        [HttpPost]
        [Route("updateuserprofile")]
        public ResultObj UpdateUserProfile(User model)
        {
            ResultObj results;
            try
            {
                var usr = User.Identity.AsAppUser().Result;
                var db = new AppDbContext();

                var user = db.Users.First(x => x.Id == usr.Id);
                user.Name = model.Name;
                user.DateOfBirth = model.DateOfBirth;
                user.IdExpiryDate = model.IdExpiryDate;
                user.IdNumber = model.IdNumber;
                user.City = model.City;
                user.ResidentialAddress = model.ResidentialAddress;
                user.PhoneNumber = model.PhoneNumber;
                user.UserName = model.PhoneNumber;
                user.Email = model.Email;
                user.ModifiedAt = DateTime.UtcNow;
                db.SaveChanges();
                results = WebHelpers.BuildResponse(null, "Profile Updated Successfully.", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }

        private static bool IsValidProfile(Profile profile)
        {
            return !string.IsNullOrEmpty(profile.Privileges) && !string.IsNullOrEmpty(profile.Name);
        }


        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("DeleteUser")]
        public ResultObj DeleteUser(string id)
        {
            ResultObj results;
            try
            {
                _userRepo.Delete(id);
                results = WebHelpers.BuildResponse(id, "User Deleted Successfully.", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }

        


        // POST api/Account/ChangePassword
        [Authorize]
        [Route("ChangePassword")]
        public async Task<ResultObj> ChangePassword(ChangePasswordBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid) return WebHelpers.ProcessException(ModelState.Values);

                var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(),
                    model.OldPassword, model.NewPassword);

                return !result.Succeeded ? WebHelpers.ProcessException(result)
                    : WebHelpers.BuildResponse(model, "Password changed sucessfully.", true, 1);
            }
            catch (Exception exception)
            {
                return WebHelpers.ProcessException(exception);
            }

        }

        

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //UserManager.Dispose();
            }

            base.Dispose(disposing);
        }

        
    }
}
