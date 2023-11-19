// See LICENSE.txt for license information.

using VictorBush.Ego.NefsEdit.Utility;
using WeifenLuo.WinFormsUI.Docking;

namespace VictorBush.Ego.NefsEdit.UI;

/// <summary>
/// Console output form.
/// </summary>
public partial class ConsoleForm : DockContent
{
	private RichTextWriter? writer;

	/// <summary>
	/// Initializes a new instance of the <see cref="ConsoleForm"/> class.
	/// </summary>
	public ConsoleForm()
	{
		InitializeComponent();
	}

	/// <summary>
	/// Sets the application's standard output to write to the form's RichTextBox control.
	/// </summary>
	public void SetupConsole()
	{
		this.writer = new RichTextWriter(this.richTextBox);
		Console.SetOut(this.writer);
	}
}
