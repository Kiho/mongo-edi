using System.Configuration;

namespace MongoData
{
    public static class SiteConfig
    {
        static SiteConfig()
        {
            if (!string.IsNullOrWhiteSpace(MongoDBServer)) return; 

            MongoDBServer = GetConnectionString("MongoDB_Server");
            if (!string.IsNullOrWhiteSpace(MongoDBServer))
                MongoDBUrl = MongoDBServer.Replace("server=", "mongodb://");
            MongoDBDatabase = GetConnectionString("MongoDB_Database");

            SecondarySlaveDB = GetConnectionString("SecondarySlave");
            if (!string.IsNullOrWhiteSpace(SecondarySlaveDB))
                SecondarySlaveDB = SecondarySlaveDB.Replace("server=", "mongodb://");
        }

        private static string GetConnectionString(string settingName)
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings[settingName];
            if (connectionStringSettings == null)
                return string.Empty;

            return connectionStringSettings.ConnectionString ?? string.Empty;
        }

        public static string MongoDBServer { get; set; }

        public static string MongoDBUrl { get; set; }

        public static string MongoDBDatabase { get; set; }

        public static string SecondarySlaveDB { get; set; }
    }
}
