using Microsoft.AspNetCore.Identity;

namespace SaaS.WebApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? TenantId { get; set; }
        public string? UserType { get; set; } = "Free";


    }
}