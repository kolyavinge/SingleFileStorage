using System;
using System.Diagnostics;
using System.IO;
using SingleFileStorage;

namespace StarterApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var createRecord = 0;
            var openRecord = 1;
            var deleteRecord = 0;
            var renameRecord = 0;
            var recordExist = 0;
            var allRecordNames = 0;
            var sequence = 0;
            var hugeWrite = 0;
            var hugeRead = 0;
            var seek = 0;
            var setLength = 0;

            var sw = Stopwatch.StartNew();

            if (createRecord == 1)
            {
                CreateRecord();
            }
            if (openRecord == 1)
            {
                OpenRecord();
            }
            if (deleteRecord == 1)
            {
                DeleteRecord();
            }
            if (renameRecord == 1)
            {
                RenameRecord();
            }
            if (recordExist == 1)
            {
                IsRecordExist();
            }
            if (allRecordNames == 1)
            {
                GetAllRecordNames();
            }
            if (sequence == 1)
            {
                Sequence();
            }
            if (hugeWrite == 1)
            {
                HugeWrite();
            }
            if (hugeRead == 1)
            {
                HugeRead();
            }
            if (seek == 1)
            {
                Seek();
            }
            if (setLength == 1)
            {
                SetLength();
            }

            sw.Stop();
            Console.WriteLine($"Total: {sw.Elapsed}");
            Console.WriteLine("done");
            Console.ReadKey();
        }

        private static void CreateRecord()
        {
            File.Delete("create.storage");
            StorageFile.Create("create.storage");
            using (var storage = StorageFile.Open("create.storage", Access.Modify))
            {
                for (int i = 0; i < 1000; i++)
                {
                    storage.CreateRecord(i.ToString());
                }
            }
        }

        private static void OpenRecord()
        {
            File.Delete("open.storage");
            StorageFile.Create("open.storage");
            using (var storage = StorageFile.Open("open.storage", Access.Modify))
            {
                storage.CreateRecord("record");
            }
            using (var storage = StorageFile.Open("open.storage", Access.Modify))
            {
                for (int i = 0; i < 10000; i++)
                {
                    storage.OpenRecord("record");
                }
            }
        }

        private static void DeleteRecord()
        {
            File.Delete("delete.storage");
            StorageFile.Create("delete.storage");
            using (var storage = StorageFile.Open("delete.storage", Access.Modify))
            {
                for (int i = 0; i < 1000; i++)
                {
                    storage.CreateRecord(i.ToString());
                }
            }
            using (var storage = StorageFile.Open("delete.storage", Access.Modify))
            {
                for (int i = 0; i < 1000; i++)
                {
                    storage.DeleteRecord(i.ToString());
                }
            }
        }

        private static void RenameRecord()
        {
            File.Delete("rename.storage");
            StorageFile.Create("rename.storage");
            using (var storage = StorageFile.Open("rename.storage", Access.Modify))
            {
                for (int i = 0; i < 1000; i++)
                {
                    storage.CreateRecord(i.ToString());
                }
            }
            using (var storage = StorageFile.Open("rename.storage", Access.Modify))
            {
                for (int i = 0; i < 1000; i++)
                {
                    storage.RenameRecord(i.ToString(), (i + 10000).ToString());
                }
            }
        }

        private static void IsRecordExist()
        {
            File.Delete("exist.storage");
            StorageFile.Create("exist.storage");
            using (var storage = StorageFile.Open("exist.storage", Access.Modify))
            {
                for (int i = 0; i < 1000; i++)
                {
                    storage.CreateRecord(i.ToString());
                }
            }
            using (var storage = StorageFile.Open("exist.storage", Access.Modify))
            {
                for (int i = 0; i < 1000; i++)
                {
                    storage.IsRecordExist(i.ToString());
                }
            }
        }

        private static void GetAllRecordNames()
        {
            File.Delete("allRecordNames.storage");
            StorageFile.Create("allRecordNames.storage");
            using (var storage = StorageFile.Open("allRecordNames.storage", Access.Modify))
            {
                for (int i = 0; i < 1000; i++)
                {
                    storage.CreateRecord(i.ToString());
                }
            }
            using (var storage = StorageFile.Open("allRecordNames.storage", Access.Modify))
            {
                for (int i = 0; i < 1000; i++)
                {
                    storage.GetAllRecordNames();
                }
            }
        }

        private static void Sequence()
        {
            var image1FileContent = File.ReadAllBytes(@"D:\Projects\SingleFileStorage\StarterApp\Sample\image1.jpg");
            var image2FileContent = File.ReadAllBytes(@"D:\Projects\SingleFileStorage\StarterApp\Sample\image2.jpg");
            File.Delete("image12.storage");
            StorageFile.Create("image12.storage");
            using (var storage = StorageFile.Open("image12.storage", Access.Modify))
            {
                // image1.jpg write
                storage.CreateRecord("image1.jpg");
                using (var stream = storage.OpenRecord("image1.jpg"))
                {
                    stream.Write(image1FileContent, 0, image1FileContent.Length);
                }
                // image2.jpg write
                storage.CreateRecord("image2.jpg");
                using (var stream = storage.OpenRecord("image2.jpg"))
                {
                    stream.Write(image2FileContent, 0, image2FileContent.Length);
                }
                // image1.jpg read
                var image1RecordReadContent = new byte[image1FileContent.Length];
                using (var stream = storage.OpenRecord("image1.jpg"))
                {
                    stream.Read(image1RecordReadContent, 0, image1RecordReadContent.Length);
                }
                // image2.jpg read
                var image2RecordReadContent = new byte[image2FileContent.Length];
                using (var stream = storage.OpenRecord("image2.jpg"))
                {
                    stream.Read(image2RecordReadContent, 0, image2RecordReadContent.Length);
                }
                File.Delete("image1_from_storage.jpg");
                File.WriteAllBytes("image1_from_storage.jpg", image1RecordReadContent);
                File.Delete("image2_from_storage.jpg");
                File.WriteAllBytes("image2_from_storage.jpg", image2RecordReadContent);
            }
        }

        private static void HugeWrite()
        {
            File.Delete("hugeWrite.storage");
            StorageFile.Create("hugeWrite.storage");
            using (var storage = StorageFile.Open("hugeWrite.storage", Access.Modify))
            {
                storage.CreateRecord("record");
                using (var record = storage.OpenRecord("record"))
                {
                    for (int i = 0; i < 10000000; i++)
                    {
                        record.WriteByte(255);
                    }
                }
            }
        }

        private static void HugeRead()
        {
            File.Delete("hugeRead.storage");
            StorageFile.Create("hugeRead.storage");
            using (var storage = StorageFile.Open("hugeRead.storage", Access.Modify))
            {
                storage.CreateRecord("record");
                var buffer = new byte[10000000];
                using (var record = storage.OpenRecord("record"))
                {
                    record.Write(buffer, 0, buffer.Length);
                }
                using (var record = storage.OpenRecord("record"))
                {
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        record.ReadByte();
                    }
                }
            }
        }

        private static void Seek()
        {
            File.Delete("seek.storage");
            StorageFile.Create("seek.storage");
            using (var storage = StorageFile.Open("seek.storage", Access.Modify))
            {
                storage.CreateRecord("record");
                var buffer = new byte[5 * 100000];
                using (var record = storage.OpenRecord("record"))
                {
                    record.Write(buffer, 0, buffer.Length);
                }
                using (var record = storage.OpenRecord("record"))
                {
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        record.Seek(i, SeekOrigin.Begin);
                    }
                }
            }
        }

        private static void SetLength()
        {
            File.Delete("setLength.storage");
            StorageFile.Create("setLength.storage");
            using (var storage = StorageFile.Open("setLength.storage", Access.Modify))
            {
                storage.CreateRecord("record");
                var buffer = new byte[100000000];
                using (var record = storage.OpenRecord("record"))
                {
                    record.Write(buffer, 0, buffer.Length);
                }
                using (var record = storage.OpenRecord("record"))
                {
                    for (int i = 0; i < 100000000; i++)
                    {
                        record.SetLength(buffer.Length - 0);
                    }
                }
            }
        }
    }
}
