using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.Startup.Models
{
    public class LoginInputModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }
}
