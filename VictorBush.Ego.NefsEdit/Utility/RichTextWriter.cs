using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VictorBush.Ego.NefsEdit.Utility
{
    /// <summary>
    /// Provides ability for a TextWriter to write to a RichTextBox control.
    /// </summary>
    public class RichTextWriter : TextWriter
    {
        private RichTextBox _textbox;

        public RichTextWriter(RichTextBox textbox)
        {
            _textbox = textbox;
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }

        public override void Write(char value)
        {
            write(value.ToString());
        }

        public override void Write(string value)
        {
            write(value);
        }

        /// <summary>
        /// Writes the provided value into the RichTextBox.
        /// </summary>
        /// <param name="value">The string to write.</param>
        private void write(string value)
        {
            if (_textbox.IsHandleCreated)
            {
                // Window handle created, do an invoke. Invoke required to allow 
                //  thread-safe modification of the textbox.
                _textbox.Invoke((MethodInvoker)(() => {
                    _textbox.Text += value;
                    _textbox.Select(_textbox.Text.Length - 1, 0);
                    _textbox.ScrollToCaret();
                }));
            }
            else
            {
                // Window handle not yet created, just modify directly
                _textbox.Text += value;
            }
        }
    }
}
