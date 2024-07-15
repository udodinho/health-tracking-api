using HealthTracker.Entities.DbSet;
using HealtTracker.Entities.DbSet;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealtTracker.DataService.Data
{
    public class AppDbContext : IdentityDbContext
    {
        static AppDbContext()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<HealthData> HealthData { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }
    }
}