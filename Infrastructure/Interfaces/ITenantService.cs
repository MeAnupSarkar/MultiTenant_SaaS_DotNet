using SaaS.WebApp.Model.Config;

namespace SaaS.WebApp.Infrastruture.Interfaces
{
    public interface ITenantService
    {
        public string GetDatabaseProvider();
        public string GetConnectionString();
        public Tenant GetTenant();
    }
}
