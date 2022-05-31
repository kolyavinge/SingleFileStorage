using System;
using System.Diagnostics;
using System.IO;
using SingleFileStorage;
using SingleFileStorage.Maintenance;

namespace StarterApp;

class Program
{
    const string _storageFileName = "storage";

    static void Main(string[] args)
    {
        var createRecord = 0;
        var createDeleteCreateRecord = 0;
        var openRecord = 0;
        var deleteRecord = 0;
        var renameRecord = 0;
        var recordExist = 0;
        var allRecordNames = 0;
        var sequence = 0;
        var write = 0;
        var bigWrite = 0;
        var read = 0;
        var seek = 0;
        var setLength = 0;
        var setLengthZero = 1;
        var defragment = 0;

        File.Delete(_storageFileName);
        StorageFile.Create(_storageFileName);

        var sw = Stopwatch.StartNew();

        if (createRecord == 1)
        {
            CreateRecord();
        }
        if (createDeleteCreateRecord == 1)
        {
            CreateDeleteCreateRecord();
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
        if (write == 1)
        {
            Write();
        }
        if (bigWrite == 1)
        {
            BigWrite();
        }
        if (read == 1)
        {
            Read();
        }
        if (seek == 1)
        {
            Seek();
        }
        if (setLength == 1)
        {
            SetLength();
        }
        if (setLengthZero == 1)
        {
            SetLengthZero();
        }
        if (defragment == 1)
        {
            Defragment();
        }

        sw.Stop();
        Console.WriteLine($"Total: {sw.Elapsed}");
        Console.WriteLine("done");
        Console.ReadKey();
    }

    private static void CreateRecord()
    {
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
        {
            for (int i = 0; i < 1000; i++)
            {
                storage.CreateRecord(i.ToString());
            }
        }
    }

    private static void OpenRecord()
    {
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
        {
            storage.CreateRecord("record");
        }
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
        {
            for (int i = 0; i < 10000; i++)
            {
                storage.OpenRecord("record");
            }
        }
    }

    private static void DeleteRecord()
    {
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
        {
            for (int i = 0; i < 1000; i++)
            {
                storage.CreateRecord(i.ToString());
            }
        }
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
        {
            for (int i = 0; i < 1000; i++)
            {
                storage.DeleteRecord(i.ToString());
            }
        }
    }

    private static void CreateDeleteCreateRecord()
    {
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
        {
            for (int i = 0; i < 1000; i++)
            {
                storage.CreateRecord(i.ToString());
            }
        }
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
        {
            for (int i = 0; i < 1000; i++)
            {
                storage.DeleteRecord(i.ToString());
            }
        }
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
        {
            for (int i = 0; i < 1000; i++)
            {
                storage.CreateRecord(i.ToString());
            }
        }
    }

    private static void RenameRecord()
    {
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
        {
            for (int i = 0; i < 1000; i++)
            {
                storage.CreateRecord(i.ToString());
            }
        }
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
        {
            for (int i = 0; i < 1000; i++)
            {
                storage.RenameRecord(i.ToString(), (i + 10000).ToString());
            }
        }
    }

    private static void IsRecordExist()
    {
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
        {
            for (int i = 0; i < 1000; i++)
            {
                storage.CreateRecord(i.ToString());
            }
        }
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
        {
            for (int i = 0; i < 1000; i++)
            {
                storage.IsRecordExist(i.ToString());
            }
        }
    }

    private static void GetAllRecordNames()
    {
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
        {
            for (int i = 0; i < 1000; i++)
            {
                storage.CreateRecord(i.ToString());
            }
        }
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
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
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
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

    private static void Write()
    {
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
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

    private static void BigWrite()
    {
        var buff = new byte[1 * 1024 * 1024];
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
        {
            storage.CreateRecord("record");
            using (var record = storage.OpenRecord("record"))
            {
                for (int i = 0; i < 400; i++)
                {
                    record.Write(buff, 0, buff.Length);
                }
            }
        }
    }

    private static void Read()
    {
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
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
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
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
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
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

    private static void SetLengthZero()
    {
        var buffer = new byte[100000000];
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
        {
            storage.CreateRecord("record");
            using (var record = storage.OpenRecord("record"))
            {
                record.Write(buffer, 0, buffer.Length);
                record.SetLength(0);
            }
        }
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
        {
            using (var record = storage.OpenRecord("record"))
            {
                record.Write(buffer, 0, buffer.Length);
            }
        }
    }

    private static void Defragment()
    {
        using (var storage = StorageFile.Open(_storageFileName, Access.Modify))
        {
            storage.CreateRecord("record");
            var buffer = new byte[10 * 1024 * 1024];
            using (var record = storage.OpenRecord("record"))
            {
                record.Write(buffer, 0, buffer.Length);
                record.SetLength(1024 * 1024);
            }
        }

        var defragmentator = DefragmentatorFactory.Make();
        defragmentator.Defragment(_storageFileName);
    }
}
