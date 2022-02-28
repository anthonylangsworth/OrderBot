using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddnMessageProcessor
{
    internal class EddnMessageDecompressor
    {
        public string Decompress(byte[] compressed)
        {
            return Encoding.UTF8.GetString(ZlibStream.UncompressBuffer(compressed));
        }
    }
}
