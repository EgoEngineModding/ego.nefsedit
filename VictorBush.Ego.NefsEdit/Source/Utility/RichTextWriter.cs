// See LICENSE.txt for license information.

using System.IO;
using System.Text;

namespace VictorBush.Ego.NefsEdit.Utility;

/// <summary>
/// Provides ability for a TextWriter to write to a RichTextBox control.
/// </summary>
public class RichTextWriter : TextWriter
{
	private RichTextBox textbox;

	/// <summary>
	/// Initializes a new instance of the <see cref="RichTextWriter"/> class.
	/// </summary>
	/// <param name="textbox">The textbox.</param>
	public RichTextWriter(RichTextBox textbox)
	{
		this.textbox = textbox;
	}

	/// <inheritdoc/>
	public override Encoding Encoding
	{
		get { return Encoding.ASCII; }
	}

	/// <inheritdoc/>
	public override void Write(char value)
	{
		DoWrite(value.ToString());
	}

	/// <inheritdoc/>
	public override void Write(string? value)
	{
		DoWrite(value);
	}

	/// <summary>
	/// Writes the provided value into the RichTextBox.
	/// </summary>
	/// <param name="value">The string to write.</param>
	private void DoWrite(string? value)
	{
		if (this.textbox.IsHandleCreated)
		{
			// Window handle created, do an invoke. Invoke required to allow thread-safe modification of the textbox.
			this.textbox.Invoke((MethodInvoker)(() =>
			{
				this.textbox.Text += value;
				this.textbox.Select(this.textbox.Text.Length - 1, 0);
				this.textbox.ScrollToCaret();
			}));
		}
		else
		{
			// Window handle not yet created, just modify directly
			this.textbox.Text += value;
		}
	}
}
