using DigitalBlog.Models;
using DigitalBlog.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalBlog.Controllers
{
    public class BlogController : Controller
    {
      
        private readonly DigitalBlogContext _context;
        private readonly IDataProtector _protector;
        private readonly IWebHostEnvironment _env;

        public BlogController(DigitalBlogContext context,DataSecurityKey key, IDataProtectionProvider provider, IWebHostEnvironment env)
        {
            _context = context;
            _protector = provider.CreateProtector(key.DataKey);
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var blogList = await _context.Blogs.Include(u=>u.User).ToListAsync();
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
                PublishedBy = e.User.FullName

            }).ToList();
            return View(blogList);
        }

        [HttpGet]
        public IActionResult AddBlog()
        {
            return View();
        }

        [HttpPost]

        public IActionResult AddBlog(BlogEdit edit)
        {
            //auto increment the Bid
            long BId = _context.Blogs.Any()? _context.Blogs.Max(m=>m.Bid) + 1 : 1;
            string fileName = Guid.NewGuid().ToString()+Path.GetExtension(edit.BlogFile!.FileName);
            string filePath = Path.Combine(_env.WebRootPath, "Images/Blogs", fileName);

            if (edit.BlogFile != null)
            {
                edit.BlogImage = fileName;


            }
            Blog b = new()
            {
                Bid = BId,
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

            if (edit.BlogFile != null)
            { 
                using(FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    edit.BlogFile.CopyTo(stream);
                }
            }
            //return Json(edit);
            return View(edit);
        }
    }
}
