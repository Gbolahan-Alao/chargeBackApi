using Microsoft.AspNetCore.Identity;

namespace ChargeBackAuthApi.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string?  Name { get; set; }
    }
}
