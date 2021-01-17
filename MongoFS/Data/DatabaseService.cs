using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;

namespace MongoFS.Data
{
    public class DatabaseService
    {
        private MongoClient dbClient;
        private IMongoDatabase db;
        private IMongoCollection<Drive> driveCollection;
        private IMongoCollection<File> fileCollection;
        private IMongoCollection<Folder> folderCollection;
        public DatabaseService()
        {
            dbClient = new MongoClient("mongodb+srv://filesystem:GDwdds84lJX6NHjI@mongofreecluster.kfoft.mongodb.net/fileSystemDb?retryWrites=true&w=majority");
            //dbClient = new MongoClient("mongodb://localhost:27017/fileSystemDb");
            db = dbClient.GetDatabase("fileSystemDb");
            driveCollection = db.GetCollection<Drive>("drives");
            fileCollection = db.GetCollection<File>("files");
            folderCollection = db.GetCollection<Folder>("folders");
        }
        public void AddFolder(Guid parentId, FileSystemType parentType, string name)
        {
            if(parentType == FileSystemType.Drive)
            {
                //create new folder and add to folder collection. Add id to list of guid in parent.
                var filter = Builders<Drive>.Filter.Eq("id", parentId);
                var drive = driveCollection.Find(filter).Single();
                var newFolder = new Folder(name, parentId, parentType);
                folderCollection.InsertOne(newFolder);
                drive.folders.Add(newFolder.id);
                var update = Builders<Drive>.Update.Set("folders", drive.folders);
                driveCollection.UpdateOne(filter, update);
            }
            else if(parentType == FileSystemType.Folder)
            {
                var filter = Builders<Folder>.Filter.Eq("id", parentId);
                var folder = folderCollection.Find(filter).Single();
                var newFolder = new Folder(name, parentId, parentType);
                folderCollection.InsertOne(newFolder);
                folder.folders.Add(newFolder.id);
                var update = Builders<Folder>.Update.Set("folders", folder.folders);
                folderCollection.UpdateOne(filter, update);
            }
        }
        public void AddFile(Guid parentId, FileSystemType parentType, string name, string data, string type)
        {
            var newFile = new File(name, data, type, parentId, parentType);
            fileCollection.InsertOne(newFile);
            if (parentType == FileSystemType.Drive)
            {
                var filter = Builders<Drive>.Filter.Eq("id", parentId);
                var drive = driveCollection.Find(filter).Single();

                drive.files.Add(newFile.id);
                drive.size += newFile.size;
                
                var update = Builders<Drive>.Update.Set("files", drive.files);
                var sizeUpdate = Builders<Drive>.Update.Set("size", drive.size);

                driveCollection.UpdateOne(filter, update);
                driveCollection.UpdateOne(filter, sizeUpdate);
            }
            else if(parentType == FileSystemType.Folder)
            {
                var filter = Builders<Folder>.Filter.Eq("id", parentId);
                var folder = folderCollection.Find(filter).Single();
                
                folder.files.Add(newFile.id);
                FolderSizeCalculate(newFile, parentId, parentType, false);

                var update = Builders<Folder>.Update.Set("files", folder.files);
                folderCollection.UpdateOne(filter, update);
            }
        }
        public void FolderSizeCalculate(File file, Guid parentId, FileSystemType parentType, bool delete)
        {
            if(parentType == FileSystemType.Folder)
            {
                var filter = Builders<Folder>.Filter.Eq("id", parentId);
                var parentFolder = folderCollection.Find(filter).Single();

                if(!delete)
                    parentFolder.size += file.size;
                else
                    parentFolder.size -= file.size;

                var update = Builders<Folder>.Update.Set("size", parentFolder.size);
                folderCollection.UpdateOne(filter, update);

                if(!delete)
                    FolderSizeCalculate(file, parentFolder.parentId, parentFolder.parentType, false);
                else
                    FolderSizeCalculate(file, parentFolder.parentId, parentFolder.parentType, true);
            }
            else
            {
                var filter = Builders<Drive>.Filter.Eq("id", parentId);
                var parentDrive = driveCollection.Find(filter).Single();

                if (!delete)
                    parentDrive.size += file.size;
                else
                    parentDrive.size -= file.size;

                var update = Builders<Drive>.Update.Set("size", parentDrive.size);
                driveCollection.UpdateOne(filter, update);
            }
        }
        public void AddDrive(string name)
        {
            driveCollection.InsertOne(new Drive(name));
        }
        public List<Drive> GetDrives()
        {
            return driveCollection.AsQueryable().ToList();
        }
        public List<FileSystemItem> GetItems(Guid targetId, FileSystemType type)
        {
            List<FileSystemItem> items = new List<FileSystemItem>();
            if (type == FileSystemType.Drive)
            {
                var filter = Builders<Drive>.Filter.Eq("id", targetId);
                var drive = driveCollection.Find(filter).Single();
                foreach (var folder in drive.folders)
                {
                    var folderFilter = Builders<Folder>.Filter.Eq("id", folder);
                    items.Add(folderCollection.Find(folderFilter).Single());
                }
                foreach (var file in drive.files)
                {
                    var fileFilter = Builders<File>.Filter.Eq("id", file);
                    items.Add(fileCollection.Find(fileFilter).Single());
                }
            }
            else if(type == FileSystemType.Folder)
            {
                var filter = Builders<Folder>.Filter.Eq("id", targetId);
                var parentFolder = folderCollection.Find(filter).Single();
                foreach (var folder in parentFolder.folders)
                {
                    var folderFilter = Builders<Folder>.Filter.Eq("id", folder);
                    items.Add(folderCollection.Find(folderFilter).Single());
                }
                foreach (var file in parentFolder.files)
                {
                    var fileFilter = Builders<File>.Filter.Eq("id", file);
                    items.Add(fileCollection.Find(fileFilter).Single());
                }
                
            }
            return items;
        }
        public string GetName(Guid id, FileSystemType type)
        {
            switch (type)
            {
                case FileSystemType.Drive:
                    var filter = Builders<Drive>.Filter.Eq("id", id);
                    var drive = driveCollection.Find(filter).Single();
                    return drive.name;
                case FileSystemType.File:
                    var fileFilter = Builders<File>.Filter.Eq("id", id);
                    var file = fileCollection.Find(fileFilter).Single();
                    return file.name;
                case FileSystemType.Folder:
                    var folderFilter = Builders<Folder>.Filter.Eq("id", id);
                    var folder = folderCollection.Find(folderFilter).Single();
                    return folder.name;
            }
            return "";
            
        }
        public void Delete(Guid id, FileSystemType type)
        {
            if(type == FileSystemType.File)
            {
                var filter = Builders<File>.Filter.Eq("id", id);
                var deletedFile = fileCollection.Find(filter).Single();
                if(deletedFile.parentType == FileSystemType.Drive)
                {
                    var parentDriveFilter = Builders<Drive>.Filter.Eq("id", deletedFile.parentId);
                    var parentDrive = driveCollection.Find(parentDriveFilter).Single();

                    parentDrive.files.Remove(id);
                    parentDrive.size -= deletedFile.size;

                    var parentDriveUpdate = Builders<Drive>.Update.Set("files", parentDrive.files);
                    var parentDriveSizeUpdate = Builders<Drive>.Update.Set("size", parentDrive.size);

                    driveCollection.UpdateOne(parentDriveFilter, parentDriveUpdate);
                    driveCollection.UpdateOne(parentDriveFilter, parentDriveSizeUpdate);
                }
                else if(deletedFile.parentType == FileSystemType.Folder)
                {
                    var parentFolderFilter = Builders<Folder>.Filter.Eq("id", deletedFile.parentId);
                    var parentFolder = folderCollection.Find(parentFolderFilter).Single();

                    FolderSizeCalculate(deletedFile, deletedFile.parentId, FileSystemType.Folder, true);

                    if (parentFolder.files.Remove(id))
                    {
                        var parentFolderUpdate = Builders<Folder>.Update.Set("files", parentFolder.files);
                        folderCollection.UpdateOne(parentFolderFilter, parentFolderUpdate);
                    }
                }
                fileCollection.DeleteOne(filter);
            }
            else if(type == FileSystemType.Folder)
            {
                var filter = Builders<Folder>.Filter.Eq("id", id);
                var deletedFolder = folderCollection.Find(filter).Single();

                if(deletedFolder.parentType == FileSystemType.Drive)
                {
                    var parentDriveFilter = Builders<Drive>.Filter.Eq("id", deletedFolder.parentId);
                    var parentDrive = driveCollection.Find(parentDriveFilter).Single();
                    parentDrive.folders.Remove(id);
                    var parentDriveUpdate = Builders<Drive>.Update.Set("folders", parentDrive.folders);
                    driveCollection.UpdateOne(parentDriveFilter, parentDriveUpdate);
                }
                else if(deletedFolder.parentType == FileSystemType.Folder)
                {
                    var parentFolderFilter = Builders<Folder>.Filter.Eq("id", deletedFolder.parentId);
                    var parentFolder = folderCollection.Find(parentFolderFilter).Single();
                    if (parentFolder.folders.Remove(id))
                    {
                        var parentFolderUpdate = Builders<Folder>.Update.Set("folders", parentFolder.folders);
                        folderCollection.UpdateOne(parentFolderFilter, parentFolderUpdate);
                    }
                }
                foreach (var file in deletedFolder.files)
                {
                    Delete(file, FileSystemType.File);
                }
                foreach(var folder in deletedFolder.folders)
                {
                    Delete(folder, FileSystemType.Folder);
                }

                folderCollection.DeleteOne(filter);
            }
            else if(type == FileSystemType.Drive)
            {
                var deleteFilter = Builders<Drive>.Filter.Eq("id", id);
                var deletedDrive = driveCollection.Find(deleteFilter).Single();
                foreach (var file in deletedDrive.files)
                {
                    var fileFilter = Builders<File>.Filter.Eq("id", file);
                    fileCollection.DeleteOne(fileFilter);
                }
                foreach (var folder in deletedDrive.folders)
                {
                    Delete(folder, FileSystemType.Folder);
                }
                driveCollection.DeleteOne(deleteFilter);
            }
        }
    }
}
