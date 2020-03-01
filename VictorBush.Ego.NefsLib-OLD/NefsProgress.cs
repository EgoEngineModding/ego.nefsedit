using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VictorBush.Ego.NefsLib
{
    /// <summary>
    /// Contains the data used by the NeFS library to report progress for async operations.
    /// </summary>
    public class NefsProgress
    {
        /// <summary>
        /// Status message to display.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Percent complete (in range 0.0 to 1.0).
        /// </summary>
        public float Progress { get; set; }
    }
}
