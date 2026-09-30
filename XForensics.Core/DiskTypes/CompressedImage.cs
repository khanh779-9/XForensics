using System.IO;

namespace XForensics.Core.DiskTypes
{
    public class CompressedImage : DriveReader
    {
        public CompressedImage(string fileName)
            : base(new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
            // Verify IMGC header

            // Pre-load blocks

            //base.Initialize();
        }
    }
}
