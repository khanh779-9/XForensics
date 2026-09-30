using XForensics.Core.FileSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace XForensics.Core.Database
{
    public class AddPartitionEventArgs
    {
        public Volume Volume;

        public AddPartitionEventArgs(Volume volume)
        {
            this.Volume = volume;
        }
    }
}
