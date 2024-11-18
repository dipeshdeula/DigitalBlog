using System.ComponentModel.DataAnnotations;

namespace DigitalBlog.Models
{
    public class ChangePsw
    {
        [DataType(DataType.Password)]
        [Required(ErrorMessage ="Please, Enter your current password!")]
        [Display(Name ="Current Password")]
        public string CurrentPassword { get; set; } = null!;

        [DataType(DataType.Password)]
        [Required(ErrorMessage ="Please, Enter your New Password!")]
        [Display(Name ="New Password")]
        public string NewPassword { get; set; } = null!;


        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Please, Enter your confirm password!")]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "Confirm Password Does not match")]
        public string ConfirmPassword { get; set; } = null!;

        public int UserId { get; set; }
    }
}
