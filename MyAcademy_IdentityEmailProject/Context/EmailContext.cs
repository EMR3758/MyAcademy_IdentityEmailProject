using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyAcademy_IdentityEmailProject.Entities;

namespace MyAcademy_IdentityEmailProject.Context
{
    public class EmailContext : IdentityDbContext<AppUser>
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           optionsBuilder.UseSqlServer("Server=EMIRHAN\\SQLEXPRESS;initial Catalog=IdentityEmailDb;integrated Security=true;trust server certificate=true;");
        }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
