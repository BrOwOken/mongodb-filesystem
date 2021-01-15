using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoFS.Data
{
    public class FileSystemItem
    {
        [BsonId]
        public Guid id { get; set; }
        public FileSystemType Type { get; set; }
        public string name { get; set; }
        public long size { get; set; }
    }
}
