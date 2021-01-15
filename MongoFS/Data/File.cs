using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoFS.Data
{
    public class File: FileSystemItem
    {
        public string data { get; set; }
        public string fileType { get; set; }
        public DateTime created { get; set; }
        public DateTime lastEdit { get; set; }
        public Guid parentId { get; set; }
        public FileSystemType parentType { get; set; }
        public File(string name, string data, string fileType, Guid parentId, FileSystemType parentType)
        {
            this.parentId = parentId;
            this.parentType = parentType;
            var r = new Random();
            created = DateTime.Now;
            id = Guid.NewGuid();
            this.name = name;
            this.data = data;
            this.fileType = fileType;
            this.size = r.Next(1, 9999);
            Type = FileSystemType.File;
        }
    }
}
