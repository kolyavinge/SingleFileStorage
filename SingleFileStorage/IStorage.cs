using System;
using System.Collections.Generic;
using System.IO;

namespace SingleFileStorage
{
    public interface IStorage : IDisposable
    {
        Access AccessMode { get; }

        void CreateRecord(string recordName);

        Stream OpenRecord(string recordName);

        bool IsRecordExist(string recordName);

        void RenameRecord(string oldRecordName, string newRecordName);

        void DeleteRecord(string recordName);

        List<string> GetAllRecordNames();
    }
}
