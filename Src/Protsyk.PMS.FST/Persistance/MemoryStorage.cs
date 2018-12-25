using System.IO;

namespace Protsyk.PMS.FST.Persistance
{
    public class MemoryStorage : StreamStorage<MemoryStream>
    {
        public MemoryStorage()
            : base(new MemoryStream()) { }

        public MemoryStorage(byte[] data)
            : base(new MemoryStream(data)) { }

    }
}
