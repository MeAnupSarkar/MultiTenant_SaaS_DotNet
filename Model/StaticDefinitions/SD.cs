namespace SaaS.WebApp.Model.StaticDefinitions
{
    public static class SD
    {

        public static string MasterDbServer = ".\\SQLEXP2019";
        public static string SharedTenantDb = "TenantSharedCatalogDb";
        public static string MasterDb = "TenantBaseDb";
        public static string MasterDbUser = "";
        public static string MasterDbPass = "";


        public static string getDefaultConnectionString() => $"Server={MasterDbServer};Database={MasterDb};Trusted_Connection=True;MultipleActiveResultSets=true;Integrated Security=True";
        public static string createConnectionString(string server, string db) => $"Server={server};Database={db};Trusted_Connection=True;MultipleActiveResultSets=true;Integrated Security=True";


        //public static string MasterDbServer = "tcp:saasdatabaseserver.database.windows.net,1433";
        //public static string SharedTenantDb = "TenantSharedCatalogDb";
        //public static string MasterDb = "TenantBaseDb";
        //public static string MasterDbUser = "";
        //public static string MasterDbPass = "";


        //public static string getDefaultConnectionString() => $"Server={MasterDbServer};Database={MasterDb};Persist Security Info=False;User ID=developer;Password=Virtual#7;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        //public static string createConnectionString(string server, string db) => $"Server={server};Database={db};Persist Security Info=False;User ID=developer;Password=Virtual#7;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";


    }
}
