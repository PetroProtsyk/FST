using System;
using System.IO;

namespace Protsyk.PMS.FST.Persistance
{
    public class FileStorage : StreamStorage<FileStream>
    {
        public FileStorage(string name)
            : base(new FileStream(name, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) { }

        public override void Flush()
        {
            stream.Flush(true);
        }

        public static bool Exists(string name)
        {
            return File.Exists(name);
        }
    }
}
