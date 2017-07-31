using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VictorBush.Ego.NefsEdit.Utility;
using WeifenLuo.WinFormsUI.Docking;

namespace VictorBush.Ego.NefsEdit.UI
{
    public partial class ConsoleForm : DockContent
    {
        RichTextWriter _writer;

        public ConsoleForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the application's standard output to write to the form's RichTextBox 
        /// control.
        /// </summary>
        public void SetupConsole()
        {
            _writer = new RichTextWriter(richTextBox);
            Console.SetOut(_writer);
        }
    }
}
