using DigitalBlog.Models;
using DigitalBlog.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalBlog.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DigitalBlogContext _context;
        private readonly IDataProtector _protector;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, DataSecurityKey key, IDataProtectionProvider provider, IWebHostEnvironment env, DigitalBlogContext context)
        {
            _logger = logger;
            _protector = provider.CreateProtector(key.Datakey);
            _env = env;
            _context = context;
        }

        [Authorize(Roles = "Admin,Editor")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "User")]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AddUser()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult AddUser(UserListEdit edit)
        {

            try
            {
                short maxId;
                if (_context.UserLists.Any())
                {
                    maxId = Convert.ToInt16(_context.UserLists.Max(u => u.UserId) + 1);

                }
                else
                {
                    maxId = 1;
                }
                edit.UserId = maxId;
                if (edit.UserFile != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(edit.UserFile.FileName);
                    string imgPath = Path.Combine(_env.WebRootPath, "UserProfile", fileName);

                    using (FileStream stream = new FileStream(imgPath, FileMode.Create))
                    {
                        edit.UserFile.CopyTo(stream);

                    }
                    edit.UserProfile = fileName;
                }
                UserList u = new()
                {
                    UserId = edit.UserId,
                    UserRole = "User",
                    LoginName = edit.LoginName,
                    LoginPassword = _protector.Protect(edit.LoginPassword),
                    EmailAddress = edit.EmailAddress,
                    FullName = edit.FullName,
                    LoginStatus = true,
                    Phone = edit.Phone,
                    UserProfile = edit.UserProfile,

                };

               // return Json(u);
                _context.UserLists.Add(u);
                _context.SaveChanges();
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                //return View(edit);
                return Json(ex.Message);


            }
        }

        [HttpGet]
        public IActionResult ProfileImg()
        {
            //var user = _context.UserLists.Where(u => u.LoginName == Convert.ToString(User.Identity!.Name)).FirstOrDefault();
            var user = _context.UserLists.Where(u => u.UserId == Convert.ToInt16(User.Identity!.Name)).FirstOrDefault();
            ViewData["img"] = user!.UserProfile;
            return PartialView("_ProfileImg");

        }
        [HttpGet]
        public IActionResult ProfileUpdate()
        {
            var user = _context.UserLists.Where(u => u.UserId == Convert.ToInt16(User.Identity!.Name)).FirstOrDefault();
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                UserListEdit edit = new()
                {
                    UserId = user.UserId,
                    LoginName = user.LoginName,
                    LoginPassword = _protector.Protect(user.LoginPassword),
                    EmailAddress = user.EmailAddress,
                    FullName = user.FullName,
                    Phone = user.Phone,
                    UserProfile = user.UserProfile,
                    UserRole = user.UserRole,
                    LoginStatus = user.LoginStatus,
                };
                return View(edit);
            }

        }

        [HttpPost]
        public IActionResult ProfileUpdate(UserListEdit edit)
        {
            try
            {
                /*var user = _context.UserLists.Where(x => x.UserId == Convert.ToInt16(edit.UserId)).FirstOrDefault();
                if (user == null)
                    return NotFound();*/

                if (edit.UserFile != null)
                {
                    string oldFilePath = Path.Combine(_env.WebRootPath, "UserProfile", edit.UserProfile);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);

                    }
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(edit.UserFile.FileName);
                    string path = Path.Combine(_env.WebRootPath, "UserProfile", fileName);
                    using (FileStream str = new FileStream(path, FileMode.Create))
                    {
                        edit.UserFile.CopyTo(str);
                    }

                    edit.UserProfile = fileName;
                }

                UserList u = new()
                {
                    UserId = edit.UserId,
                    UserProfile = edit.UserProfile,
                    EmailAddress = edit.EmailAddress,
                    LoginName = edit.LoginName,
                    LoginPassword = edit.LoginPassword,
                    FullName = edit.FullName,
                    LoginStatus = edit.LoginStatus,
                    Phone = edit.Phone,
                    UserRole = User.Claims.FirstOrDefault(c=>c.Type==ClaimTypes.Role).Value
                };

                _context.UserLists.Update(u);
                _context.SaveChanges();
               // return Json(u);
                return RedirectToAction("ProfileUpdate");


            }
            catch
            {
                return BadRequest();
            }
        }
    }

}
