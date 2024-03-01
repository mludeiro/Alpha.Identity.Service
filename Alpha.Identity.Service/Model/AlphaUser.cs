using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Alpha.Identity.Model;

public class AlphaUser : IdentityUser
{
    public AlphaUser() : base()
    {
    }
    
    public AlphaUser(string userName) : base(userName)
    {
    }

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    public bool IsAdmin { get; set; }
}

