using Alpha.Identity.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Alpha.Identity.Data;

public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext<AlphaUser>(options)
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}
