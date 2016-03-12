using System;
using System.IO;
using System.IO.Compression;

namespace Common.Compression
{
    public static class Compressor
    {
        public static byte[] CompressByte(byte[] data)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(data, 0, data.Length);
                }
                return memory.ToArray();
            }
        }
    }
}
