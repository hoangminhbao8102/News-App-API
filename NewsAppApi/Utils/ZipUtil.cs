using System.IO.Compression;

namespace NewsAppApi.Utils
{
    public static class ZipUtil
    {
        // entries: filename -> content bytes
        public static byte[] CreateZip(Dictionary<string, byte[]> entries)
        {
            using var ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var kv in entries)
                {
                    var entry = archive.CreateEntry(kv.Key, CompressionLevel.Optimal);
                    using var s = entry.Open();
                    s.Write(kv.Value, 0, kv.Value.Length);
                }
            }
            return ms.ToArray();
        }
    }
}
