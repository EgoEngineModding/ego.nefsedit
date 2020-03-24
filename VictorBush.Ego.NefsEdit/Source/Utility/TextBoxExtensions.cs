// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Utility
{
    using System.Windows.Forms;

    /// <summary>
    /// Extension methods for text boxes.
    /// </summary>
    public static class TextBoxExtensions
    {
        /// <summary>
        /// Scrolls text box to end to show the end of the text string.
        /// </summary>
        /// <param name="textBox">The text box to scroll.</param>
        public static void ScrollToEnd(this TextBox textBox)
        {
            textBox.SelectionStart = textBox.Text.Length;
            textBox.SelectionLength = 0;
        }
    }
}
