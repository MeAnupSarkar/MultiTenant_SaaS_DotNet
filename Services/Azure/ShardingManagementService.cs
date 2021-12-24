
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

////////////////////////////////////////////////////////////////////////////////////////
// This sample follows the CodeFirstNewDatabase Blogging tutorial for EF.
// It illustrates the adjustments that need to be made to use EF in combination
// with the Entity Framework to scale out your data tier across many databases and
// benefit from Elastic Database Tools capabilities for Data Dependent Routing and 
// Shard Map Management.
//
// In particular, this sample shows how to configure multi-tenant shards, using 
// Row-Level Security (RLS) to ensure that tenants can only access their own rows.
////////////////////////////////////////////////////////////////////////////////////////

namespace Infrastructure.Azure
{
    // This sample requires three pre-created empty SQL Server databases. 
    // The first database serves as the shard map manager database to store the Elastic Database shard map.
    // The remaining two databases serve as multi-tenant shards to hold the data for the sample.
    public class ShardingManagementService
    {
        // You need to adjust the following settings to your database server and database names in Azure Db
        private static string server = "";
        private static string shardmapmgrdb = "";
        private static string shard1 = "ShardDb1";
        private static string shard2 = "ShardDb2";
        private static string userName = "";
        private static string password = "";
        private static string applicationName = "";

        //private static string server = "saasdatabaseserver2.database.windows.net";
        //private static string shardmapmgrdb = "SaaSWebApp_ShardMapManager";
        //private static string shard1 = "ShardDb1";
        //private static string shard2 = "ShardDb2";
        //private static string userName = "developer";
        //private static string password = "Virtual#7";
        //private static string applicationName = "SaaSWebAppv1.0";

        // Four tenants
        private static string tenantId1 = "1";
        private static string tenantId2 = "2";
        private static string tenantId3 = "3";
        private static string tenantId4 = "4";

        private string ConnectionString;
 

        private readonly SharedCatalogDbContext _contextSharedCatalogDb;

        private readonly IConfiguration config;

        SqlConnectionStringBuilder connStrBldr;

        public ShardingManagementService()
        {

        }

        public ShardingManagementService(IConfiguration con, SharedCatalogDbContext _contextSharedCatalogDb)
        {
            //this._context = _context;
            this._contextSharedCatalogDb = _contextSharedCatalogDb;
           // this.ConnectionString = ConnectionString;
            this.config=con;

          
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
        public  void init()
        {
  
            ShardingService sharding = new ShardingService(server, shardmapmgrdb, connStrBldr.ConnectionString);
            //sharding.RegisterNewShard(server, shard1, connStrBldr.ConnectionString, tenantId1);
            //sharding.RegisterNewShard(server, shard2, connStrBldr.ConnectionString, tenantId2);
            //sharding.RegisterNewShard(server, shard1, connStrBldr.ConnectionString, tenantId3);
            //sharding.RegisterNewShard(server, shard2, connStrBldr.ConnectionString, tenantId4);

    
            var tenants = new string[] { tenantId1, tenantId2, tenantId3, tenantId4 };



            //_contextSharedCatalogDb.Database.SetConnectionString(connStrBldr.ConnectionString);

            //_contextSharedCatalogDb.Database.EnsureCreated();


            //if (_contextSharedCatalogDb.Database.GetMigrations().Count() > 0)
            //{
            //    _contextSharedCatalogDb.Database.Migrate();
            //}

            SqlConnection conn = sharding.ShardMap.OpenConnectionForKey<string>(tenantId4.ToString(), connStrBldr.ConnectionString, ConnectionOptions.Validate);

            foreach (var tenantId in tenants)
            {
                
                 

                SqlDatabaseUtils.SqlRetryPolicy.ExecuteAction(() =>
                {
                    var product = new Product("Test Product", "none", 1000, tenantId.ToString());
                    _contextSharedCatalogDb.Add(product);
                    _contextSharedCatalogDb.SaveChanges();

                    //using (var db = new ElasticScaleContext<int>(sharding.ShardMap, tenantId, connStrBldr.ConnectionString))
                    //{
                    //    var blog = new Blog { Name = name, TenantId = tenantId }; // must specify TenantId unless using default constraints to auto-populate
                    //    db.Blogs.Add(blog);
                    //    db.SaveChanges();

                    //    // If Row-Level Security is enabled, tenants will only display their own blogs
                    //    // Otherwise, tenants will see blogs for all tenants on the shard db
                    //    var query = from b in db.Blogs
                    //                orderby b.Name
                    //                select b;

                    //    Console.WriteLine("All blogs for TenantId {0}:", tenantId);
                    //    foreach (var item in query)
                    //    {
                    //        Console.WriteLine(item.Name);
                    //    }
                    //}
                });
            }

            // Example query via ADO.NET SqlClient
            // If Row-Level Security is enabled, only Tenant 4's blogs will be listed
            SqlDatabaseUtils.SqlRetryPolicy.ExecuteAction(() =>
            {
                // Note: We are using a wrapper function OpenDDRConnection that automatically set SESSION_CONTEXT with the specified TenantId. 
                // This is a best practice to ensure that SESSION_CONTEXT is always set before executing a query.
                //using (SqlConnection conn = ElasticScaleContext<int>.OpenDDRConnection(sharding.ShardMap, tenantId4, connStrBldr.ConnectionString))
                //{
                //    SqlCommand cmd = conn.CreateCommand();
                //    cmd.CommandText = @"SELECT * FROM Blogs";

                //    Console.WriteLine("\n--\n\nAll blogs for TenantId {0} (using ADO.NET SqlClient):", tenantId4);
                //    SqlDataReader reader = cmd.ExecuteReader();
                //    while (reader.Read())
                //    {
                //        Console.WriteLine("{0}", reader["Name"]);
                //    }
                //}
            });

            // Because of the RLS block predicate, attempting to insert a row for the wrong tenant will throw an error.
            Console.WriteLine("\n--\n\nTrying to create a new Blog for TenantId {0} while connected as TenantId {1}: ", tenantId2, tenantId3);
            SqlDatabaseUtils.SqlRetryPolicy.ExecuteAction(() =>
            {
                //using (var db = new ElasticScaleContext<int>(sharding.ShardMap, tenantId3, connStrBldr.ConnectionString))
                //{
                //    // Verify that block predicate prevents Tenant 3 from inserting rows for Tenant 2
                //    try
                //    {
                //        var bad_blog = new Blog { Name = "BAD BLOG", TenantId = tenantId2 };
                //        db.Blogs.Add(bad_blog);
                //        db.SaveChanges();
                //        Console.WriteLine("No error thrown - make sure your security policy has a block predicate on this table in each shard database.");
                //    }
                //    catch (DbUpdateException)
                //    {
                //        Console.WriteLine("Can't insert blog for incorrect tenant.");
                //    }
                //}
            });
        }


        public void AddShard(int tenantSl,string shardDb)
        {          
            ShardingService sharding = new ShardingService(server, shardmapmgrdb, connStrBldr.ConnectionString);
            sharding.RegisterNewShard(server, shardDb, connStrBldr.ConnectionString, tenantSl);
       
            //int[] tenants = new int[] { tenantId1, tenantId2, tenantId3, tenantId4 };

            //_contextSharedCatalogDb.Database.SetConnectionString(connStrBldr.ConnectionString);

            //_contextSharedCatalogDb.Database.EnsureCreated();


            //if (_contextSharedCatalogDb.Database.GetMigrations().Count() > 0)
            //{
            //    _contextSharedCatalogDb.Database.Migrate();
            //}

             
        }

        public async Task AddShardWithNewDatabase(int tenantSl, string shardDb,string connectionStr)
        {

            _contextSharedCatalogDb.Database.SetConnectionString(connectionStr);

           //  await _contextSharedCatalogDb.Database.EnsureCreatedAsync();


            if (_contextSharedCatalogDb.Database.GetMigrations().Count() > 0)
            {
                await _contextSharedCatalogDb.Database.MigrateAsync();
            }

            // _contextSharedCatalogDb.SaveChanges();

            //try
            //{
                ShardingService sharding = new ShardingService(server, shardmapmgrdb, connStrBldr.ConnectionString);
                sharding.RegisterNewShard(server, shardDb, connStrBldr.ConnectionString, tenantSl);

            //}
            //catch (Exception ex)
            //{
            //    _contextSharedCatalogDb.Database.EnsureDeleted();
            //    Debug.WriteLine(ex.Message);
            //}

             


        }

    }
}
