// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Utility
{
    using System;

    /// <summary>
    /// Settings.
    /// </summary>
    [Serializable]
    public class Settings
    {
        /// <summary>
        /// The directory for DiRT 4.
        /// </summary>
        public string Dirt4Dir { get; set; }

        /// <summary>
        /// The directory for DiRT Rally 2.
        /// </summary>
        public string DirtRally2Dir { get; set; }

        /// <summary>
        /// Quick extract.
        /// </summary>
        public string QuickExtractDir { get; set; }
    }
}
