using System;
using System.IO;
using SingleFileStorage.Core;

namespace SingleFileStorage.Test.Tools
{
    internal abstract class BaseTest
    {
        protected Storage _storage;
        protected MemoryFileStream _memoryStream;

        public Stream CreateRecordWithContent(string recordName, byte[] recordContent)
        {
            _storage.CreateRecord(recordName);
            var record = _storage.OpenRecord(recordName, RecordAccess.ReadWrite);
            record.Write(recordContent, 0, recordContent.Length);

            return record;
        }

        public RecordDescription GetRecordDescription(string name)
        {
            var position = _memoryStream.Position;
            _memoryStream.Seek(0, SeekOrigin.Begin);
            var recordDescription = RecordDescription.FindByName(_memoryStream, name);
            _memoryStream.Seek(position, SeekOrigin.Begin);

            return recordDescription;
        }

        public byte[] GetRandomByteArray(int size)
        {
            var result = new byte[size];
            var rand = new Random();
            rand.NextBytes(result);

            return result;
        }
    }
}
