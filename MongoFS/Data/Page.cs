using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoFS.Data
{
    public class Page
    {
        public Guid Id { get; set; }
        public FileSystemType Type { get; set; }
        public Page(Guid id, FileSystemType type)
        {
            Id = id;
            Type = type;
        }
    }
}
