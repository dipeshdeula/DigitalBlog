using DigitalBlog.Models;
using DigitalBlog.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalBlog.Controllers
{
    public class BlogController : Controller
    {
        private readonly ILogger<BlogController> _logger;
        private readonly DigitalBlogContext _context;
        private readonly IDataProtector _protector;
        private readonly IWebHostEnvironment _env;

        public BlogController(ILogger<BlogController> logger,DigitalBlogContext context,DataSecurityKey key, IDataProtectionProvider provider, IWebHostEnvironment env)
        {
            _logger = logger;
            _context = context;
            _protector = provider.CreateProtector(key.DataKey);
            _env = env;
        }

       [HttpGet]
        public async Task<IActionResult> Index()
        {
            
            
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetBlogList(BlogEdit edit)
        {
          
            var blogList = await _context.Blogs.Include(u=>u.User).Where(b=>b.Bstatus==edit.Bstatus).ToListAsync();

            var bgList = blogList.Select(e => new BlogEdit
            {
                Bid = e.Bid,
                Title = e.Title,
                Bdescription = e.Bdescription,
                BlogImage = e.BlogImage,
                BlogPostDate = e.BlogPostDate,
                UserId = e.UserId,
                Bstatus = e.Bstatus,
                Amount = e.Amount,
                PublishedBy = e.User.FullName,
                BlogEncId = _protector.Protect(e.Bid.ToString())


            }).ToList();
            return Json(bgList);
           // return PartialView("_GetBlogList",bgList);
        }
        [Authorize(Roles ="Admin,Editor")]
        [HttpGet]
        public IActionResult AddBlog()
        {
            return View();
        }


        [Authorize(Roles = "Admin,Editor")]
        [HttpPost]

        public IActionResult AddBlog(BlogEdit edit)
        {
            //auto increment the Bid
            long BId = _context.Blogs.Any()? _context.Blogs.Max(m=>m.Bid) + 1 : 1;
            edit.Bid = BId;
            string fileName = Guid.NewGuid().ToString()+Path.GetExtension(edit.BlogFile!.FileName);
            string filePath = Path.Combine(_env.WebRootPath, "Images/Blogs", fileName);

            if (edit.BlogFile != null)
            {

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    edit.BlogFile.CopyTo(stream);
                }

                edit.BlogImage = fileName;

            }
            Blog b = new()
            {
                Bid = edit.Bid,
                Title = edit.Title,
                Bdescription = edit.Bdescription,
                BlogImage = edit.BlogImage,
                BlogPostDate = DateOnly.FromDateTime(DateTime.Today),
                UserId = Convert.ToInt16(User.Identity!.Name),
                Bstatus = edit.Bstatus,
                Amount = edit.Amount
            };

            _context.Blogs.Add(b);
            _context.SaveChanges();

            //return Json(edit);
            return View(edit);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        { 
            if(id==0)
            {
                return NotFound();
            }

            var blogs = _context.Blogs.Where(b=>b.Bid==id).Include(b=> b.User).First();
            if (blogs != null)
            {
                BlogEdit e = new()
                {
                    Bid = blogs.Bid,
                    Title = blogs.Title,
                    Amount = blogs.Amount,
                    Bdescription = blogs.Bdescription,
                    BlogImage = blogs.BlogImage,
                    BlogPostDate = blogs.BlogPostDate,
                    Bstatus = blogs.Bstatus,
                    UserId = blogs.UserId,
                    PublishedBy = blogs.User.FullName

                };
                return View(e);
            }
            else {
                return Content("try again");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]

        public IActionResult Edit(BlogEdit edit)
        {
            if (edit.BlogFile != null)
            {
                string filename = Guid.NewGuid().ToString() + Path.GetExtension(edit.BlogFile.FileName);
                string path = Path.Combine(_env.WebRootPath, "Images/Blogs", filename);
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    edit.BlogFile.CopyTo(stream);
                }
                edit.BlogImage = filename;
            }
            Blog b = new()
            {
                Bid = edit.Bid,
                Amount = edit.Amount,
                Bdescription = edit.Bdescription,
                BlogImage = edit.BlogImage,
                BlogPostDate = DateOnly.FromDateTime(DateTime.Today),
                Bstatus = edit.Bstatus,
                Title = edit.Title,
                UserId = Convert.ToInt16(User.Identity!.Name)
            };

            _context.Blogs.Update(b);
            _context.SaveChanges();
            return Content("success");

            try
            {

            }
            catch (Exception ex)
            {
                return Json(ex);
            }
            return View(edit);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var blogs = _context.Blogs.Where(x => x.Bid == id).First();
            if (blogs != null)
            {
                _context.Blogs.Remove(blogs);
                _context.SaveChanges();
                return Content("success");
            }
            else
            {
                return Content("try again");
            }
        }

        [Authorize]
        public IActionResult Success(string q, string oid, string amt, string refId)
        {
            var bid = _protector.Unprotect(oid);
            Blog? sub = _context.Blogs.Where(x => x.Bid == Convert.ToInt32(bid)).FirstOrDefault();
            if (sub != null)
            {
                var Subid = (_context.BlogSubscriptions.Any()) ? _context.BlogSubscriptions.Max(x => x.SubId) + 1 : 1;
                BlogSubscription s = new()
                {
                    SubId = Subid,
                    Bid = sub.Bid,
                    SubAmount = Convert.ToDecimal(amt),
                    UserId = Convert.ToInt16(User.Identity!.Name)
                };
                _context.Add(s);
                _context.SaveChanges();
                string msg = "Payment Successful. Rs." + amt;
                return View((object)msg);
            }
            return View();
        }
        public IActionResult Failure()
        {
            return View();
        }
    }
}
