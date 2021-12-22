using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SaaS.WebApp.Data;
using SaaS.WebApp.Model.Config;
using System.Diagnostics;

namespace SaaS.WebApp.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static  async Task<IServiceCollection>  AddAndMigrateTenantDatabasesAsync(this IServiceCollection services, IConfiguration config)
        {
            var options = services.GetOptions<TenantSettings>(nameof(TenantSettings));
            var defaultConnectionString = options.Defaults?.ConnectionString;
            var defaultDbProvider = options.Defaults?.DBProvider;

            if (defaultDbProvider.ToLower() == "mssql")
            {
                services.AddDbContext<SharedCatalogDbContext>(m => m.UseSqlServer(e => e.MigrationsAssembly(typeof(SharedCatalogDbContext).Assembly.FullName)));
            }
         
            var masterDbContext = services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

            //var tenants = options.Tenants;
            var tenants = await  masterDbContext.Tenants.ToListAsync();

            foreach (var tenant in tenants)
            {
                string connectionString = string.IsNullOrEmpty(tenant.ConnectionString) ? defaultConnectionString : tenant.ConnectionString;
          

                using var scope = services.BuildServiceProvider().CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<SharedCatalogDbContext>();

                dbContext.Database.SetConnectionString(connectionString);
 
                if (dbContext.Database.GetMigrations().Count() > 0)
                {
                      dbContext.Database.Migrate();
                }
 

               
            }
            return services;
        }


        public static void MigrateAndGenerateDatabase(this IServiceCollection services,String connectionString)
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SharedCatalogDbContext>();
            dbContext.Database.SetConnectionString(connectionString);

            if (dbContext.Database.GetMigrations().Count() > 0)
            {
                dbContext.Database.Migrate();
            }
        }

      
        public static T GetOptions<T>(this IServiceCollection services, string sectionName) where T : new()
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var section = configuration.GetSection(sectionName);
            var options = new T();
            section.Bind(options);
            return options;
        }
    }
}
