using Microsoft.AspNetCore.Identity;

namespace ContactApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }


}
