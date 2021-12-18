using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaaS.WebApp.Model.Entity.Tables;
using SaaS.WebApp.Infrastruture.Interfaces;
using SaaS.WebApp.Models;
using SaaS.WebApp.Model.Config;

namespace SaaS.WebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
 


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            

        }
        public DbSet<Tenant> Tenants { get; set; }







        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


         //   builder.Entity<Product>().HasQueryFilter(a => a.TenantId == TenantId);


        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    var tenantConnectionString = _tenantService.GetConnectionString();


        //    if (!string.IsNullOrEmpty(tenantConnectionString))
        //    {
        //        var DBProvider = _tenantService.GetDatabaseProvider();

        //        if (DBProvider.ToLower() == "mssql")
        //        {
        //            optionsBuilder.UseSqlServer(_tenantService.GetConnectionString());
        //        }
        //    }
        //}
        //public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        //{
        //    foreach (var entry in ChangeTracker.Entries<ICompulsaryTenantProperty>().ToList())
        //    {
        //        switch (entry.State)
        //        {
        //            case EntityState.Added:
        //            case EntityState.Modified:
        //                entry.Entity.TenantId = TenantId;
        //                break;
        //        }
        //    }
        //    var result = await base.SaveChangesAsync(cancellationToken);
        //    return result;
        //}





    }


}