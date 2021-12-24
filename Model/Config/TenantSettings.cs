namespace SaaS.WebApp.Model.Config
{
    public class TenantSettings
    {
        public Configuration Defaults { get; set; }
        public List<Tenant> Tenants { get; set; }

    }
}
