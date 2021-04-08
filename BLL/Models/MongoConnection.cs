using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Models
{
    public class MongoConnection
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
        public string Database { get; set; } = "local";
    }
}
