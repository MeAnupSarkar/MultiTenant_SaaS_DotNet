using SaaS.WebApp.Model.Base;

namespace SaaS.WebApp.Model.Config
{
    public class Configuration : BaseEntity
    {
        public string DBProvider { get; set; }
        public string ConnectionString { get; set; }

        public string TenantSharedCatalogDbConnection { get; set; }

        public string DefaultConnection2 { get; set; }
        public string TenantSharedCatalogDbConnection2 { get; set; }

        public string ShardMapManagerDbConnection { get; set; }
        public string DB_Server { get; set; }
        public string ShardMapDb { get; set; }
        public string userName { get; set; }

        public string password { get; set; }

        public string applicationName { get; set; }

    }
}