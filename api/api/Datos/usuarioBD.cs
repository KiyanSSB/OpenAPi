
using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.Datos
{
    public class usuarioContext:DbContext
    {
        public usuarioContext(DbContextOptions<usuarioContext> options):base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<usuario> usuarios { get; set; }
    }
}
