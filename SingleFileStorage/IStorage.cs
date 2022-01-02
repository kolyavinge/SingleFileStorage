using System;
using System.IO;

namespace SingleFileStorage
{
    public interface IStorage : IDisposable
    {
        void CreateRecord(string recordName);

        Stream OpenRecord(string recordName);
    }
}
