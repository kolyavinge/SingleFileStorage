using System.IO;
using SingleFileStorage;

namespace StarterApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var sequence = 1;

            if (sequence == 1)
            {
                Sequence();
            }
        }

        private static void Sequence()
        {
            var image1FileContent = File.ReadAllBytes(@"D:\Projects\SingleFileStorage\StarterApp\Sample\image1.jpg");
            var image2FileContent = File.ReadAllBytes(@"D:\Projects\SingleFileStorage\StarterApp\Sample\image2.jpg");
            File.Delete("image12.storage");
            StorageBuilder.Create("image12.storage");
            using (var storage = StorageBuilder.Open("image12.storage", Access.Modify))
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
    }
}
