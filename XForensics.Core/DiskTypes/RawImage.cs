using System.IO;

namespace XForensics.Core.DiskTypes
{
    public class RawImage : DriveReader
    {
        // TODO: replace with FileStream to be able to use "using"
        public RawImage(string fileName)
            : base(new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
            base.Initialize();
        }
    }
}
