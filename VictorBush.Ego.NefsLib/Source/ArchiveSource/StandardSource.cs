// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.ArchiveSource
{
    using System;
    using System.IO;


    public sealed class StandardSource : NefsArchiveSource
    {
        internal StandardSource(string filePath)
            : base(filePath)
        {
        }
    }
}
