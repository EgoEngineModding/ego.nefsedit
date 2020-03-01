using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VictorBush.Ego.NefsEdit.Source
{
    /// <summary>
    /// Provides user interface dialogs and other services.
    /// </summary>
    internal interface IUiService
    {
        /// <summary>
        /// Shows a message box.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title of the message box.</param>
        /// <param name="buttons">Buttons to show.</param>
        /// <param name="icon">Icon to display.</param>
        /// <returns>The dialog result.</returns>
        DialogResult ShowMessageBox(
            string message, 
            string title = null, 
            MessageBoxButtons buttons = MessageBoxButtons.OK, 
            MessageBoxIcon icon = MessageBoxIcon.None);
    }
}
