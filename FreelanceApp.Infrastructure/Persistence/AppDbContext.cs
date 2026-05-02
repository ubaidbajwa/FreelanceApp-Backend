using FreelanceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Freelancer.Infrastructure.Presistence;

public class AppDbContext : DbContext
{
   public AppDbContext (DbContextOptions<AppDbContext> options) : base(options)
    {

    }
    public DbSet<User> Users => Set<User>();
}