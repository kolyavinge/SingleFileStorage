using System;
using System.IO;

namespace SingleFileStorage
{
    public interface IStorage
    {
        Stream OpenRecord(string name, RecordAccess access);
    }
}
