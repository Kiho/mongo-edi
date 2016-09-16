using System;
using System.Diagnostics;
using MongoDB.Driver;
using MongoData;

namespace MongoData
{
    public class MongoContext<T>
    {
        private readonly string name;
        private MongoCollection<T> collection;
        private MongoCollection<T> reportCollection;

        public MongoContext()
            : this (typeof(T).Name)
        {         
        }

        public MongoContext(string name)
        {
            this.name = name;
        }


        public MongoCollection<T> Collection { get { return (collection ?? (collection = GetCollection<T>(name))); } }
        public MongoCollection<T> SlaveServerCollection { get { return (reportCollection ?? (reportCollection = GetReportCollection<T>(name))); } }

        #region static method


        internal MongoCollection<U> GetCollection<U>(string collectionName)
        {
            return GetCollection<U>(GetDatabase(), collectionName);
        }

        internal MongoCollection<U> GetCollection<U>(MongoDatabase database, string collectionName)
        {
            return database.GetCollection<U>(collectionName);
        }

        internal MongoCollection GetCollection(string collectionName)
        {
            return GetCollection(GetDatabase(), collectionName);
        }

        internal MongoCollection GetCollection(MongoDatabase database, string collectionName)
        {
            return database.GetCollection(collectionName);
        }

        internal MongoCollection<U> GetReportCollection<U>(string collectionName)
        {
            return GetCollection<U>(GetReportDatabase(), collectionName);
        }
        



        private static MongoDatabase GetDatabase()
        {
            return GetDatabase(SiteConfig.MongoDBUrl, SiteConfig.MongoDBDatabase);
        }

        private static MongoDatabase GetReportDatabase()
        {
            return GetDatabase(SiteConfig.SecondarySlaveDB, SiteConfig.MongoDBDatabase);
        }

        private static MongoDatabase GetDatabase(string connString, string dbName)
        {
            if ( string.IsNullOrWhiteSpace(connString))
                throw new ArgumentNullException("connString");

            if (string.IsNullOrWhiteSpace(dbName))
                throw new ArgumentNullException("dbName");

            var server = MongoServer.Create(connString);
            return server.GetDatabase(dbName);
        }

        #endregion
    }
}
