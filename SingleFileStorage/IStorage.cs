using System;
using System.IO;

namespace SingleFileStorage
{
    public interface IStorage
    {
        void CreateRecord(string recordName);
        Stream OpenRecord(string name, RecordAccess access);
    }
}
