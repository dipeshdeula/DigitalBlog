using DigitalBlog.Models;
using DigitalBlog.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;

namespace DigitalBlog.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly DigitalBlogContext _context;
        private readonly IDataProtector _protector;

        public AccountController(ILogger<AccountController> logger, DigitalBlogContext context,  DataSecurityKey key, IDataProtectionProvider provider)
        {
            _logger = logger;
            _context = context;
            _protector = provider.CreateProtector(key.DataKey);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(UserListEdit edit)
        {
         
            var u = await _context.UserLists.ToListAsync();
           
            if (u != null)
            {

               /* var user = u.Where(e => (e.LoginName.ToUpper().Equals
                (edit.LoginName.ToUpper()) || e.EmailAddress.ToUpper().Equals(edit.EmailAddress.ToUpper()))
                    && _protector.Unprotect(e.LoginPassword).Equals(edit.LoginPassword) && e.LoginStatus == true).FirstOrDefault();*/

               var user = u.Where(e=>e.LoginName.Equals(edit.LoginName) && _protector.Unprotect(e.LoginPassword).Equals(edit.LoginPassword) && e.LoginStatus == true).FirstOrDefault();



                if (user != null)
                {
                    List<Claim> claims = new()
                    {
                        new Claim(ClaimTypes.Name,user.UserId.ToString()),
                        /*new Claim(ClaimTypes.Name, user.LoginName),*/
                        new Claim(ClaimTypes.Role, user.UserRole),
                         new Claim("Email", user.EmailAddress),
                        new Claim("FullName", user.FullName),
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
                        new AuthenticationProperties { IsPersistent = edit.RememberMe });
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid UserName or Password");
                    return View(edit);
                }
            }
            else
            {
                ModelState.AddModelError("", "this user doesn't exist");
                return View(edit);
            }

        }

        [Authorize]
        public IActionResult Dashboard()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index","Home");

            }
            else if (User.IsInRole("Editor"))
            {
                return RedirectToAction("Index","Home");

            }
            else
            {
                return RedirectToAction("Privacy","Home");
            }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Static");
        
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {

            return View();
            
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePsw p)
        {
            try {
                var user = await _context.UserLists.Where(c => c.UserId == Convert.ToInt16(User.Identity!.Name)).FirstOrDefaultAsync();

                if (user != null)
                {
                    if (_protector.Unprotect(user.LoginPassword) == p.CurrentPassword)
                    {
                        if (p.NewPassword == p.ConfirmPassword)
                        {
                            user.LoginPassword = _protector.Protect(p.NewPassword);
                            _context.Update(user);
                            await _context.SaveChangesAsync();
                            return Json("Success");

                        }
                        else
                        {
                            ModelState.AddModelError("", "Confirm password does not matched.");
                            return View(p);

                        }

                    }
                    else
                    {
                        ModelState.AddModelError("", "Please, Re-Check your current password.");
                        return View(p);
                    }
                }
                else {
                    return View();
                }

            }
            catch (Exception ex)
            {
                return View(ex);

            }
        }

        [AllowAnonymous]
        [HttpGet]

        public IActionResult ForgotPassword()
        {


            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(UserListEdit edit)
        {
            try
            {
                var user = _context.UserLists.Where(u => u.EmailAddress == edit.EmailAddress).FirstOrDefault();
                if (user == null)
                {
                    ModelState.AddModelError("", "This email address does not exist.");
                    return View(edit);

                }

                Random r = new Random();
                HttpContext.Session.SetString("otp", r.Next(100000, 999999).ToString());
                HttpContext.Session.SetString("id", _protector.Protect(user.UserId.ToString()));
                var otp = HttpContext.Session.GetString("otp");

                SmtpClient smtpClient = new()
                {

                    Host = "smtp.gmail.com",
                    Port = 587,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("deuladipesh94@gmail.com", "yugt ynov piuo psnx"), //gmail_id, app Password
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                MailMessage mailMessage = new()
                {
                    From = new MailAddress("deuladipesh94@gmail.com"),
                    Subject = "forgot password",
                    Body = $"<a style='background-color:blue; color:white; padding:5px;' href='https://localhost:7299/Account/ResetPassword/{_protector.Protect(otp!)}'>Reset Password</a>" +
                    $"<p>otp:{otp}</p>",

                    IsBodyHtml = true,
                };
                mailMessage.To.Add(edit.EmailAddress);
                smtpClient.Send(mailMessage);
                return RedirectToAction("verifyOtp");
            }
            catch (SmtpException smtpEx)

            {
                ModelState.AddModelError("", "There was an error sending the Email. Please try again later.");
                _logger.LogError(smtpEx, "SMTP error in ForgotPassword");
                return View(edit);


            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                _logger.LogError(ex, "Error in ForgotPassword");
                return View(edit);

            }
           
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult VerifyOtp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOtp(UserListEdit edit)
        {
            try
            {
                var otp = HttpContext.Session.GetString("otp");
                int o= Convert.ToInt32(otp);
                if (edit.Otp == o)
                {
                   
                   return RedirectToAction("ResetPassword", new { id = _protector.Protect(otp!) });
                }
                else {
                    return View(edit);
                }

            }
            catch 
            {
                return View();
            
            }
            
        }

        [HttpGet]
        public IActionResult ResetPassword(string id)
        {
            try
            {
                var otp = _protector.Unprotect(id);
                var o = HttpContext.Session.GetString("otp");

                if (o == otp)
                {
                    var uid = HttpContext.Session.GetString("id");
                    if (uid == null)
                    {

                        return Json("null Id");
                    }

                    int userid = Convert.ToInt32(_protector.Unprotect(uid));
                    ChangePsw p = new() { UserId = userid };
                    return View(p);

                }
                else
                {
                    return RedirectToAction("ForgotPassword");

                }

            }
            catch
            {
                return RedirectToAction("ForgotPassword");

            }

        }

        [HttpPost]
        public IActionResult ResetPassword(ChangePsw p)
        {
            var user = _context.UserLists.Where(u => u.UserId == p.UserId).FirstOrDefault();
            if (user == null)
            {
                return Json("null");
            }

            if (p.NewPassword == p.ConfirmPassword)
            {
                user.LoginPassword = _protector.Protect(p.NewPassword);
                _context.Update(user);
                return RedirectToAction("Login", "Account");
            }

            else {
                ModelState.AddModelError("", "Confirm Password");
                return View(p);
            }
        }

    }
}
