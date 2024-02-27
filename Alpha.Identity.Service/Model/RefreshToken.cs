using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Alpha.Identity.Model;
    
[Index(nameof(Token))]
public class RefreshToken
{
    [Key]
    public int Id { get; set; }

    public required string Token { get; set; }
    public required string JwtId { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime DateExpire { get; set; }

    public required string UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public IdentityUser? User { get; set; }
}