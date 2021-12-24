using SaaS.WebApp.Model.Base;

namespace SaaS.WebApp.Model.Config
{
    public class Tenant : BaseEntity
    {
        public string Name { get; set; }

        public string TID { get; set; }    

        public string Database { get; set; }

        public string DatabaseProvider { get; set; }

        public string ConnectionString { get; set; }

        public bool Status { get; set; }

        public int TenantSl { get; set; }



    }
}