using Document = Domain.Entities.Document;
namespace Infrustructure;

public class AppDbContext : DbContext
{
    private readonly ITenantResolver _tenantResolver;
    public DbSet<User> Users { get; set; }
    public DbSet<Token> Tokens { get; set; }
    public DbSet<Owner> Owners { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Document> Documents { get; set; }

    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options
      , ITenantResolver tenantResolver) : base(options)
    {
       _tenantResolver = tenantResolver;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>().HasQueryFilter(c => c.OwnerId == _tenantResolver.GetOwnerId());

        modelBuilder.Entity<Department>().HasQueryFilter(c => c.CompanyId == _tenantResolver.GetCompanyId());

        modelBuilder.Entity<Document>().HasQueryFilter(c => c.DepartmentId == _tenantResolver.GetDepartmentId());

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<AppDbContext>()
            .Build();   

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (!string.IsNullOrEmpty(connectionString))
        {
            optionsBuilder.UseNpgsql(connectionString);
        }
        else
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }
    }
}

