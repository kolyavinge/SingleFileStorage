using System.IO;

namespace SingleFileStorage.Maintenance;

internal interface IFileSystem
{
    void CreateStorageFile(string fullPath);
    IStorage OpenStorageFile(string fullPath, Access access);
    void RenameFile(string fullPath, string renamedFilePath);
    void DeleteFile(string fullPath);
}

class FileSystem : IFileSystem
{
    public void CreateStorageFile(string fullPath)
    {
        StorageFile.Create(fullPath);
    }

    public IStorage OpenStorageFile(string fullPath, Access access)
    {
        return StorageFile.Open(fullPath, access);
    }

    public void RenameFile(string fullPath, string renamedFilePath)
    {
        File.Move(fullPath, renamedFilePath);
    }

    public void DeleteFile(string fullPath)
    {
        File.Delete(fullPath);
    }
}
