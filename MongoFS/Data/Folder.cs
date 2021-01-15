using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoFS.Data
{
    public class Folder: FileSystemItem
    {
        public DateTime created { get; set; }
        public DateTime lastEdit { get; set; }
        public List<Guid> files { get; set; }
        public List<Guid> folders { get; set; }
        public Guid parentId { get; set; }
        public FileSystemType parentType { get; set; }
        public Folder(string name, Guid parentId, FileSystemType parentType)
        {
            this.parentId = parentId;
            this.parentType = parentType;
            id = Guid.NewGuid();
            created = DateTime.Now;
            this.name = name;
            this.size = 0;
            Type = FileSystemType.Folder;
            files = new List<Guid>();
            folders = new List<Guid>();
        }
    }
}
