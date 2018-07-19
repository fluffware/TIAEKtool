using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAtool
{
    class TempFile
    {
        /* Open a temporary file for writing. The file name is assembled from the base, a random number and the extension */
        static public Stream Open(string basename, string ext, out FileInfo file)
        {
            Random rnd = new Random();

            Stream stream;
            int tries = 0;
            while (true)
            {
                try
                {
                    string path = Path.Combine(Path.GetTempPath(), basename + "_" + rnd.Next(1, 32767) + "." + ext);
                    stream = new FileStream(path, FileMode.CreateNew);
                    file = new FileInfo(path);
                    break;
                }
                catch (IOException e)
                {
                    if (++tries >= 10)
                    {
                        throw new IOException("Too many tries when finding a unique filename", e);
                    }
                }
            }

            return stream;
        }
        static public FileInfo File(string basename, string ext)
        {
            FileInfo file = null;
            Random rnd = new Random();
            int tries = 0;
            while (true)
            {
                try
                {
                    string path = Path.Combine(Path.GetTempPath(), basename + "_" + rnd.Next(1, 32767) + "." + ext);
                    file = new FileInfo(path);
                    if (file.Exists) {
                        if (++tries >= 10)
                        {
                            throw new IOException("Too many tries when finding a unique filename");
                        }
                        continue;
                    }

                    break;
                }
                catch (IOException e)
                {
                    if (++tries >= 10)
                    {
                        throw new IOException("Too many tries when finding a unique filename", e);
                    }
                }
            }
            return file;
        }
    }
}
