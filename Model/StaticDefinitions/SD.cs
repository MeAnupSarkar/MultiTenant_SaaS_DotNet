namespace SaaS.WebApp.Model.StaticDefinitions
{
    public static class SD
    {
         
        public static string MasterDbServer = ".\\SQLEXP2019";
        public static string MasterDb = "MasterDb";
        public static string MasterDbUser = "";
        public static string MasterDbPass = "";


        public static string  getDefaultConnectionString() => $"Server={MasterDbServer};Database={MasterDb};Trusted_Connection=True;MultipleActiveResultSets=true;Integrated Security=True";
        public static string createConnectionString(string server,string db) => $"Server={server};Database={db};Trusted_Connection=True;MultipleActiveResultSets=true;Integrated Security=True";


    }
}
