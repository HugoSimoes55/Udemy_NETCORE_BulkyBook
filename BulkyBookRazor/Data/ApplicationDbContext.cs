using BulkyBookRazor.Model;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookRazor.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public DbSet<Category> Category { get; set; }
}
