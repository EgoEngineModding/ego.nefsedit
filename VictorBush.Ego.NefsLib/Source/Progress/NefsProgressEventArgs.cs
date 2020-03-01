// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Progress
{
    /// <summary>
    /// Progress report information.
    /// </summary>
    public class NefsProgressEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NefsProgressEventArgs"/> class.
        /// </summary>
        /// <param name="progress">Current progress.</param>
        /// <param name="message">Current message.</param>
        /// <param name="subMessage">Current sub-message.</param>
        public NefsProgressEventArgs(float progress, string message, string subMessage)
        {
            this.Progress = progress;
            this.Message = message;
            this.SubMessage = subMessage;
        }

        /// <summary>
        /// Current status message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Percent complete in range [0.0, 1.0].
        /// </summary>
        public float Progress { get; }

        /// <summary>
        /// Current status sub-message.
        /// </summary>
        public string SubMessage { get; }
    }
}
