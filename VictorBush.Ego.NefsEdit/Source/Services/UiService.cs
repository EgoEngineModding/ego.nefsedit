// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Services
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// UI service implementation.
    /// </summary>
    internal class UiService : IUiService
    {
        /// <inheritdoc/>
        public DialogResult ShowMessageBox(String message, String title = null, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            return MessageBox.Show(message, title, buttons, icon);
        }
    }
}
