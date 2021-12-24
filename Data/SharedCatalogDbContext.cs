using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Microsoft.EntityFrameworkCore;
using SaaS.WebApp.Infrastruture.Interfaces;
using SaaS.WebApp.Model.Entity.Tables;
using System.Data.Common;
using System.Data.SqlClient;

namespace SaaS.WebApp.Data
{
    public   class SharedCatalogDbContext : DbContext
    {

        public string TenantId { get; set; }
        private readonly ITenantService _tenantService;

 
   

        public SharedCatalogDbContext(DbContextOptions<SharedCatalogDbContext> options, ITenantService tenantService) : base(options)
        {
            
            _tenantService = tenantService;
            TenantId = _tenantService.GetTenant()?.TID;
        }

        public DbSet<Product> Products { get; set; }



        
       



        
      

        // Only static methods are allowed in calls into base class c'tors
        private static DbConnection CreateDDRConnection(ShardMap shardMap, string shardingKey, string connectionStr)
        {
                     
            // Ask shard map to broker a validated connection for the given key
            SqlConnection conn = shardMap.OpenConnectionForKey<string>(shardingKey, connectionStr, ConnectionOptions.Validate);
            return conn;
        }

      

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>().HasQueryFilter(a => a.TenantId == TenantId);

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var tenantConnectionString = _tenantService.GetConnectionString();

            if (!string.IsNullOrEmpty(tenantConnectionString))
            {
                var DBProvider = _tenantService.GetDatabaseProvider();

                if (DBProvider.ToLower() == "mssql")
                {
                    optionsBuilder.UseSqlServer(_tenantService.GetConnectionString());
                }
            }
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<ICompulsaryTenantProperty>().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                    case EntityState.Modified:
                        entry.Entity.TenantId = TenantId;
                        break;
                }
            }
            var result = await base.SaveChangesAsync(cancellationToken);
            return result;
        }




    }
}
