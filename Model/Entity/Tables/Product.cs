using SaaS.WebApp.Infrastruture.Interfaces;
using SaaS.WebApp.Model.Base;


namespace SaaS.WebApp.Model.Entity.Tables
{
    public class Product : BaseEntity, ICompulsaryTenantProperty
    {


        public Product()
        {

        }

        public Product(string name, string description, int rate)
        {
            Name = name;
            Description = description;
            Rate = rate;
        }

        public Product(string name, string description, int rate, string tenantId)
        {
            Name = name;
            Description = description;
            Rate = rate;
            TenantId = tenantId;
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public int Rate { get; private set; }
        public string TenantId { get; set; }

    }
}
