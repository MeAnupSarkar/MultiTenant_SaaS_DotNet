using Infrastructure.Azure;
using Microsoft.EntityFrameworkCore;
using SaaS.WebApp.Data;
using SaaS.WebApp.Model.Config;
using SaaS.WebApp.Model.Entity.Tables;
using SaaS.WebApp.Model.StaticDefinitions;
using SaaS.WebApp.Models;
using System.Diagnostics;

namespace SaaS.WebApp.Services
{
    public class AzureElasticScaleScalingOutDbService
    {
        private readonly ApplicationDbContext _context;
        private readonly SharedCatalogDbContext _currentContext;

        private readonly IConfiguration config;

        private static string masterDbServer = "";
        private static string userName = "";
        private static string password = "";
        private static string tenantSharedCatalogDb = "";


        public AzureElasticScaleScalingOutDbService(ApplicationDbContext context, SharedCatalogDbContext contextSharedDb, IConfiguration config)
        {
            _context = context;
            _currentContext = contextSharedDb;
            this.config = config;


            masterDbServer = config["TenantSettings:Defaults:DB_Server"];
            tenantSharedCatalogDb = config["TenantSettings:Defaults:TenantSharedCatalogDb"];
            userName = config["TenantSettings:Defaults:userName"];
            password = config["TenantSettings:Defaults:password"];

            
        }

        internal   async  Task  CreateUserDedicatedOrSharedTenantDb(ApplicationUser user, string code)
        {
            // Premium User
            if (user.UserType == "Paid")
            {
                Tenant tenant = new Tenant();
                tenant.Database = user.TenantId;
                tenant.Name = user.TenantId;
                tenant.TID = user.TenantId;
                tenant.DatabaseProvider = "mssql";
                tenant.ConnectionString = SD.createConnectionString(masterDbServer, tenant.Database,userName,password);
                tenant.Status = true;
                tenant.TenantSl= _context.Tenants.Count() + 1;

                await _context.Tenants.AddAsync(tenant);
                await _context.SaveChangesAsync();

                var azureShardingManager = new ShardingManagementService(config, _currentContext);

                var shardDb = user.TenantId;

                await azureShardingManager.AddShardWithNewDatabase(tenant.TenantSl, shardDb, tenant.ConnectionString);

                //_currentContext.Database.SetConnectionString(tenant.ConnectionString);

                //if (_currentContext.Database.GetMigrations().Count() > 0)
                //{
                //    _currentContext.Database.Migrate();
                //}


                // Free Trial User
            }
            else
            {
                Tenant tenant = new();
                tenant.Database = SD.SharedTenantDb;
                tenant.Name = user.TenantId;
                tenant.TID = user.TenantId;
                tenant.DatabaseProvider = "mssql";
                tenant.ConnectionString = SD.createConnectionString(masterDbServer, tenantSharedCatalogDb,userName,password);
                tenant.Status = true;
                tenant.TenantSl = _context.Tenants.Count() + 1;

                await _context.Tenants.AddAsync(tenant);
                await _context.SaveChangesAsync();


                var azureShardingManager = new ShardingManagementService(config, _currentContext);

                var shardDb = SD.SharedTenantDb;

                await azureShardingManager.AddShardWithNewDatabase(tenant.TenantSl, shardDb, tenant.ConnectionString);

            }
        }

        internal async Task ChangeUserDedicatedOrSharedTenantDb(ApplicationUser user )
        {
            List<Product> existingData = null;

            switch (user.UserType)
            {
                // From Paid to Free
                case "Free":

                    try
                    {
                        var t = _context.Tenants.Where(s => s.TID == user.TenantId).SingleOrDefault();

                        if (t is not null)
                        {
                            _currentContext.Database.SetConnectionString(t.ConnectionString);

                            //Fetching Existing Data
                            existingData = _currentContext.Products.Where(s => s.TenantId == user.TenantId).ToList();

                            //Removing Existing Tenant Data
                            _currentContext.RemoveRange(existingData);
                            _currentContext.SaveChanges();

                            t.Database = tenantSharedCatalogDb;
                            t.ConnectionString = SD.createConnectionString(masterDbServer, tenantSharedCatalogDb, userName, password);
                            _context.Update(t);
                            _context.SaveChanges();


                            //_currentContext.ChangeTracker.Entries().ToList().ForEach(e => e.State = EntityState.Detached);

                            //_currentContext.Database.EnsureDeleted();  

                            //_currentContext.SaveChanges();

                            //_currentContext.Database.SetConnectionString(t.ConnectionString);

                            //if (existingData.Count > 0)
                            //{

                            //    existingData.ForEach(x =>
                            //    {
                            //        x.Id = 0;
                            //    });

                            //    // Move Data
                            //    _currentContext.AddRange(existingData);
                            //    _currentContext.SaveChanges();
                            //}

                            
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("****************************");
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine("****************************");

                    }

                    break;

                // From Free to Paid
                case "Paid":

                    try
                    {
                        var tenant = _context.Tenants.Where(s => s.TID == user.TenantId).SingleOrDefault();

                        if (tenant is not null)
                        {
                            var str = tenant.ConnectionString;

                            tenant.Database = user.TenantId;
                            tenant.ConnectionString = SD.createConnectionString(masterDbServer, tenant.Database, userName, password);
                            _context.Update(tenant);
                            _context.SaveChanges();


                            _currentContext.Database.SetConnectionString(str);

                            if (_currentContext.Products.Any(s => s.TenantId == user.TenantId))
                            {
                                existingData = _currentContext.Products.Where(s => s.TenantId == user.TenantId).ToList();

                                //Removing Existing Tenant Data
                                _currentContext.RemoveRange(existingData);
                                _currentContext.SaveChanges();

                            }


                            var azureShardingManager = new ShardingManagementService(config,_currentContext);

                            var shardDb = user.TenantId;

                           await azureShardingManager.AddShardWithNewDatabase(tenant.TenantSl, shardDb, tenant.ConnectionString);



                           // _currentContext.Database.SetConnectionString(tenant.ConnectionString);

                            //if (_currentContext.Database.GetMigrations().Count() > 0)
                            //{
                            //    _currentContext.Database.Migrate();

                            //    if (existingData.Count > 0)
                            //    {
                            //        existingData.ForEach(x =>
                            //        {
                            //            x.Id = 0;
                            //        });


                            //        // Move Data
                            //        _currentContext.AddRange(existingData);
                            //        _currentContext.SaveChanges();
                            //    }


                            //}

                            
                        }

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("****************************");
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine("****************************");
                    }



                    break;
            }

        }
    }
}
