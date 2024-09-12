using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using web_bite_server.Models;

namespace web_bite_server.Data
{
    public class ApplicationDBContext : IdentityDbContext<AppUser>
    {
        public ApplicationDBContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }

        public DbSet<CardGameConnection> CardGameConnection { get; set; }
        public DbSet<CardGameCard> CardGameCard { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>().HasOne(u => u.CardGameConnection).WithOne(t => t.AppUser).HasForeignKey<CardGameConnection>(fk => fk.AppUserId);
            builder.Entity<CardGameConnection>().HasOne(u => u.AppUser).WithOne(t => t.CardGameConnection).HasForeignKey<AppUser>(fk => fk.CardGameConnectionId);

            builder.Entity<CardGameConnection>()
                .HasMany(e => e.CardGameHand)
                .WithMany(e => e.CardGameConnection)
                .UsingEntity("CardGamePlayerHand");

            builder.Entity<CardGameConnection>()
                .HasMany(e => e.CardGamePlayed)
                .WithMany(e => e.CardGameConnection)
                .UsingEntity("CardGamePlayerPlayedCards");

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