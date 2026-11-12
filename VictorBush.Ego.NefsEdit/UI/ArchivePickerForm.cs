// See LICENSE.txt for license information.

using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace VictorBush.Ego.NefsEdit.UI;

/// <summary>
/// Lets the user pick which NeFS archive to open when a file contains several concatenated archives
/// (or when the first archive does not start at offset 0).
/// </summary>
internal sealed class ArchivePickerForm : Form
{
	private readonly ListView list;
	private readonly Button okButton;
	private readonly Button cancelButton;

	public ArchivePickerForm(string filePath, IReadOnlyList<ArchiveEntryInfo> entries)
	{
		Text = $"Escolher arquivo NeFS - {Path.GetFileName(filePath)}";
		StartPosition = FormStartPosition.CenterScreen;
		FormBorderStyle = FormBorderStyle.Sizable;
		MinimizeBox = false;
		MaximizeBox = true;
		Width = 760;
		Height = 460;
		MinimumSize = new Size(560, 320);

		var header = new Label
		{
			Dock = DockStyle.Top,
			Height = 46,
			Padding = new Padding(10, 8, 10, 8),
			Text = "Este arquivo nao comeca com um cabecalho NeFS (ou contem varios arquivos " +
				"concatenados)." + Environment.NewLine +
				"Escolha qual deles abrir:",
		};

		this.list = new ListView
		{
			Dock = DockStyle.Fill,
			View = View.Details,
			FullRowSelect = true,
			MultiSelect = false,
			HideSelection = false,
			GridLines = true,
		};

		this.list.Columns.Add("#", 40, HorizontalAlignment.Right);
		this.list.Columns.Add("Offset", 140, HorizontalAlignment.Right);
		this.list.Columns.Add("0x", 110, HorizontalAlignment.Right);
		this.list.Columns.Add("Entradas", 80, HorizontalAlignment.Right);
		this.list.Columns.Add("Versao", 90, HorizontalAlignment.Left);
		this.list.Columns.Add("Nome / descricao", 260, HorizontalAlignment.Left);

		for (var i = 0; i < entries.Count; ++i)
		{
			var entry = entries[i];
			var item = new ListViewItem(i.ToString(CultureInfo.InvariantCulture));
			item.SubItems.Add(entry.Offset.ToString("N0", CultureInfo.InvariantCulture));
			item.SubItems.Add(entry.Offset.ToString("X", CultureInfo.InvariantCulture));
			item.SubItems.Add(entry.NumEntries.ToString("N0", CultureInfo.InvariantCulture));
			item.SubItems.Add(entry.Version);
			item.SubItems.Add(entry.Name);
			item.Tag = entry.Offset;
			this.list.Items.Add(item);
		}

		this.list.DoubleClick += (_, _) => Accept();

		if (this.list.Items.Count > 0)
		{
			this.list.Items[0].Selected = true;
			this.list.Select();
		}

		var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 48 };
		this.okButton = new Button { Text = "Abrir", DialogResult = DialogResult.OK, Width = 110, Height = 30 };
		this.cancelButton = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Width = 110, Height = 30 };
		this.okButton.Click += (_, _) => Accept();
		this.okButton.Left = buttonPanel.Width - 250;
		this.okButton.Top = 9;
		this.okButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		this.cancelButton.Left = buttonPanel.Width - 130;
		this.cancelButton.Top = 9;
		this.cancelButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		buttonPanel.Controls.Add(this.okButton);
		buttonPanel.Controls.Add(this.cancelButton);

		Controls.Add(this.list);
		Controls.Add(header);
		Controls.Add(buttonPanel);
		AcceptButton = this.okButton;
		CancelButton = this.cancelButton;
	}

	/// <summary>
	/// Offset of the archive the user picked.
	/// </summary>
	public long SelectedOffset { get; private set; }

	private void Accept()
	{
		if (this.list.SelectedItems.Count == 0)
		{
			return;
		}

		SelectedOffset = (long)this.list.SelectedItems[0].Tag!;
		DialogResult = DialogResult.OK;
		Close();
	}
}

/// <summary>
/// Description of one NeFS archive found inside a file.
/// </summary>
internal sealed record ArchiveEntryInfo(long Offset, string Version, uint NumEntries, string Name);
