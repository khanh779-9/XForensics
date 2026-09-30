using XForensics.Core.Analyzers.Signatures;

namespace XForensics.Core.Database
{
    class CarvedFile
    {
        FileSignature signature;

        public CarvedFile(FileSignature signature)
        {
            this.signature = signature;
        }
    }
}
