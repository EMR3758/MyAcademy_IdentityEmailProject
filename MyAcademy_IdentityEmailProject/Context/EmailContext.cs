using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyAcademy_IdentityEmailProject.Entities;

namespace MyAcademy_IdentityEmailProject.Context
{
    public class EmailContext : IdentityDbContext<AppUser>
    {
        public EmailContext(DbContextOptions<EmailContext> options) : base(options)
        {
        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<MessageFavorite> MessageFavorites { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<MessageFavorite>()
                .HasIndex(x => new { x.MessageId, x.UserId })
                .IsUnique();
        }

    }
}
