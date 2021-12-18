using SaaS.WebApp.Model.Base;

namespace SaaS.WebApp.Model.Config
{
    public class Configuration : BaseEntity
    {
        public string DBProvider { get; set; }
        public string ConnectionString { get; set; }

    }
}