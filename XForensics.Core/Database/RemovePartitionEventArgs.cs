namespace XForensics.Core.Database
{
    public class RemovePartitionEventArgs
    {
        private int index;

        public RemovePartitionEventArgs(int index)
        {
            this.index = index;
        }

        public int Index => index;
    }
}