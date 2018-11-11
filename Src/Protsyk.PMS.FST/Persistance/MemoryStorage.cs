using System.IO;

namespace Protsyk.PMS.FST.Persistance
{
    public class MemoryStorage : StreamStorage<MemoryStream>
    {
        public MemoryStorage()
            : base(new MemoryStream()) { }
    }
}
