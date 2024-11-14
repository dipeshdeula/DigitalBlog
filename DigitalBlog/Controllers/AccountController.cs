using DigitalBlog.Models;
using Microsoft.AspNetCore.Mvc;

namespace DigitalBlog.Controllers
{
    public class AccountController : Controller
    {
        private readonly DigitalBlogContext _context;

        public AccountController(DigitalBlogContext context)
        {
            _context = context;
        }
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Login(UserListEdit edit)
        {
            var u = _context.UserLists.ToList();
            if (u != null)
            {

                var user = u.Where(e => e.LoginName.ToUpper().Equals
                (edit.LoginName.ToUpper()) || e.EmailAddress.ToUpper().Equals(edit.EmailAddress.ToUpper()))
                    && e.LoginPassword.Equals(edit.LoginPassword) && e.LoginStatus == true).FirstOrDefault();
            }
            else
            {
                ModelState.AddModelError("LoginName", "Invalid Login");
                return View(edit);
            }
            return Json(edit);
        }
    }
}
