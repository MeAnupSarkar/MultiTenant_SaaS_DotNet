
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Data.Azure;
using Data.Azure.Service;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SaaS.WebApp.Data;
using SaaS.WebApp.Model.Config;
using SaaS.WebApp.Model.Entity.Tables;

namespace Infrastructure.Azure
{
   
    public class ShardingManagementService
    {  
        private static string server = "";
        private static string shardmapmgrdb = "";
        private static string userName = "";
        private static string password = "";
        private static string applicationName = "";

        SqlConnectionStringBuilder connStrBldr;
        private readonly SharedCatalogDbContext _contextSharedCatalogDb;
        private readonly IConfiguration config;

        public ShardingManagementService()  {}

        public ShardingManagementService(IConfiguration con, SharedCatalogDbContext _contextSharedCatalogDb)
        {
            this._contextSharedCatalogDb = _contextSharedCatalogDb;
            this.config=con;

            //Reading connection string from appsettings.json      
            server = config["TenantSettings:Defaults:DB_Server"];
            shardmapmgrdb = config["TenantSettings:Defaults:ShardMapDb"];      
            userName = config["TenantSettings:Defaults:userName"];
            password = config["TenantSettings:Defaults:password"];
            applicationName = config["TenantSettings:Defaults:applicationName"];

            connStrBldr = new SqlConnectionStringBuilder
            {
                UserID = userName,
                Password = password,
                ApplicationName = applicationName
            };
        }
     

        public async Task AddShardWithNewDatabase(int tenantSl, string shardDb,string connectionStr)
        {

            _contextSharedCatalogDb.Database.SetConnectionString(connectionStr);

            //  await _contextSharedCatalogDb.Database.EnsureCreatedAsync();


            // Created Tenant if not exists along with create Product table migration 
            if (_contextSharedCatalogDb.Database.GetMigrations().Count() > 0)
            {
                await _contextSharedCatalogDb.Database.MigrateAsync();
            }

            ShardingService sharding = new ShardingService(server, shardmapmgrdb, connStrBldr.ConnectionString);
            sharding.RegisterNewShard(server, shardDb, connStrBldr.ConnectionString, tenantSl);
 
        }

        public async Task DeleteShardsWithMapping(int tenantSl, string shardDb, string connectionStr)
        {
            ShardingService sharding = new ShardingService(server, shardmapmgrdb, connStrBldr.ConnectionString);
            sharding.DeleteShard(server, shardDb, connStrBldr.ConnectionString, tenantSl);

        }

    }
}
