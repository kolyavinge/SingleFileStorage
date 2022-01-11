namespace SingleFileStorage.Maintenance
{
    public interface IDefragmentator
    {
        void Defragment(string storageFilePath);
    }

    internal class Defragmentator : IDefragmentator
    {
        private readonly IFileSystem _fileSystem;

        public Defragmentator(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void Defragment(string storageFilePath)
        {
            var defragmentStorageFilePath = GetDefragmentedFilePath(storageFilePath);
            _fileSystem.CreateStorageFile(defragmentStorageFilePath);
            var buffer = new byte[10 * 1024 * 1024];
            using (var currentStorage = _fileSystem.OpenStorageFile(storageFilePath, Access.Read))
            using (var defragmentStorage = _fileSystem.OpenStorageFile(defragmentStorageFilePath, Access.Modify))
            {
                foreach (var recordName in currentStorage.GetAllRecordNames())
                {
                    defragmentStorage.CreateRecord(recordName);
                    using (var currentRecord = currentStorage.OpenRecord(recordName))
                    using (var defragmentRecord = defragmentStorage.OpenRecord(recordName))
                    {
                        int count;
                        while ((count = currentRecord.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            defragmentRecord.Write(buffer, 0, count);
                        }
                    }
                }
            }
            _fileSystem.DeleteFile(storageFilePath);
            _fileSystem.RenameFile(defragmentStorageFilePath, storageFilePath);
        }

        private string GetDefragmentedFilePath(string filePath) => filePath + ".defrag";
    }
}
