using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace MongoData
{
    public class MongoTarget
    {
        public virtual IEnumerable<string> Files
        {
            get
            {
                return new[]
                           {
                               "bsondump.exe",
                               "mongo.exe",
                               "mongod.exe",
                               "mongodump.exe",
                               "mongoexport.exe",
                               "mongofiles.exe",
                               "mongoimport.exe",
                               "mongorestore.exe",
                               "mongos.exe",
                               "mongostat.exe"
                           };
            }
        }

        public virtual IEnumerable<string> FilePaths
        {
            get { return Files.Select(f => Path.Combine(Directory, f)); }
        }

        public virtual string Executable
        {
            get { return Path.Combine(Directory, "mongo.exe"); }
        }

        public virtual string Arguments
        {
            get { return string.Format("--host=\"{0}:{1}\" {2} ", Host, Port, DatabaseName); }
        }

        protected virtual string Host
        {
            get { return "localhost"; }
        }

        protected virtual int Port
        {
            get { return 27017; }
        }

        public bool CreateNoWindow
        {
            get { return true; }
        }

        public virtual string ConnectionString
        {
            get { return "mongodb://localhost:{0}" + Port; }
        }

        public virtual string DatabaseName
        {
            get { return "test"; }
        }

        public virtual string Directory
        {
            get
            {
                string dir = ConfigurationManager.AppSettings["MongoBinDirectory"];
                return dir ?? @"C:\mongo\bin";
            }
        }
    }
}
