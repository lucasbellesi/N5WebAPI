using Microsoft.EntityFrameworkCore;

public class PermissionDbContext : DbContext
{
    public DbSet<Permission> Permissions { get; set; }

    public PermissionDbContext(DbContextOptions<PermissionDbContext> options)
        : base(options)
    {
    }

    // Aquí puedes agregar más configuraciones específicas para la base de datos si es necesario
}
