using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Compression;

// Подключение библиотекаи Snappy
using Snappy;

namespace ConsoleApplicationArchivy
{
    static class ArchivySnappy
    {
        static public String Compress(string nameFile)
        {
            string pathToFile = Path.GetFullPath(nameFile);
            FileInfo fi = new FileInfo(pathToFile);

            using (FileStream fs = fi.OpenRead())
            {
                using (FileStream outFile1 = File.Create(nameFile + ".sz"))
                {
                    using (SnappyStream compress = new SnappyStream(outFile1, CompressionMode.Compress))
                    {
                        fs.CopyTo(compress);
                        return (outFile1.Name);
                    }
                }
            }
        }

        static public void Decompress(string nameFile)
        {
            string pathToFile = Path.GetFullPath(nameFile);

            FileInfo fi = new FileInfo(pathToFile);

            using (FileStream inFile = fi.OpenRead())
            {
                string curFile = fi.FullName;
                string origName = curFile.Remove(curFile.Length -
                fi.Extension.Length);

                using (FileStream outFile = File.Create(origName))
                {
                    using (SnappyStream decompress = new SnappyStream(inFile, CompressionMode.Decompress))
                    {
                        decompress.CopyTo(outFile);
                    }
                }
            }
        }
    }
}

