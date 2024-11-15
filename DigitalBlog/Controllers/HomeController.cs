using DigitalBlog.Models;
using DigitalBlog.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace DigitalBlog.Controllers
{
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

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            return View();
        }

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
                else {
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
                _context.UserLists.Add(u);
                _context.SaveChanges();
                return Json(u);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);


            }
        }

  }

}
