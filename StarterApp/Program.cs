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
            var sequence = 0;
            var hugeWrite = 1;
            var hugeRead = 0;
            var seek = 0;

            var sw = Stopwatch.StartNew();

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

            sw.Stop();
            Console.WriteLine($"Total: {sw.Elapsed}");
            Console.WriteLine("done");
            Console.ReadKey();
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
    }
}
