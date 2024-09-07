using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using web_bite_server.Models;

namespace web_bite_server.Data
{
    public class ApplicationDBContext : IdentityDbContext<AppUser>
    {
        public ApplicationDBContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }

        public DbSet<GameConnection> GameConnection { get; set; }
        public DbSet<GameCard> GameCard { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>().HasOne(u => u.GameConnection).WithOne(t => t.AppUser).HasForeignKey<GameConnection>(fk => fk.AppUserId);
            builder.Entity<GameConnection>().HasOne(u => u.AppUser).WithOne(t => t.GameConnection).HasForeignKey<AppUser>(fk => fk.GameConnectionId);

            List<IdentityRole> roles =
            [
                new IdentityRole
                {
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Name = "User",
                    NormalizedName = "USER"
                }
            ];
            builder.Entity<IdentityRole>().HasData(roles);
        }
    }
}