// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Services
{
    using System;
    using log4net;
    using VictorBush.Ego.NefsEdit.Utility;

    /// <summary>
    /// Settings service implementation.
    /// </summary>
    internal class SettingsService : ISettingsService
    {
        private static readonly ILog Log = LogHelper.GetLogger();

        /// <inheritdoc/>
        public string QuickExtractDir { get; }

        /// <inheritdoc/>
        public bool ChooseQuickExtractDir()
        {
            throw new NotImplementedException();
        }
    }
}
