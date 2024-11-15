using DigitalBlog.Models;
using DigitalBlog.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalBlog.Controllers
{
    public class AccountController : Controller
    {
        private readonly DigitalBlogContext _context;
        private readonly IDataProtector _protector;

        public AccountController(DigitalBlogContext context,IDataProtectionProvider provider , DataSecurityKey key)
        {
            _context = context;
            _protector = provider.CreateProtector(key.Datakey);
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
            var u = _context.UserLists.ToList();
            if (u != null)
            {

                var user = u.Where(e => (e.LoginName.ToUpper().Equals
                (edit.LoginName.ToUpper()) || e.EmailAddress.ToUpper().Equals(edit.EmailAddress.ToUpper()))
                    && _protector.Unprotect(e.LoginPassword).Equals(edit.LoginPassword) && e.LoginStatus == true).FirstOrDefault();

                if (user != null)
                {
                    List<Claim> claims = new()
                    {

                        new Claim(ClaimTypes.Name, user.LoginName),
                        new Claim(ClaimTypes.Email, user.EmailAddress),
                        new Claim(ClaimTypes.Role, user.UserRole),
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
            return Json("Hello Dashboard");
        }
    }
}
