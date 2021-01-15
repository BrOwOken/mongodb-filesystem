using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoFS.Data
{
    public class Drive: FileSystemItem
    {
        public List<Guid> files { get; set; }
        public List<Guid> folders { get; set; }
        public Drive(string name)
        {
            id = Guid.NewGuid();
            this.name = name;
            files = new List<Guid>();
            folders = new List<Guid>();
            size = 0;
            Type = FileSystemType.Drive;
        }
    }
}
