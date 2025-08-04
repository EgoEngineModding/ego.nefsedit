namespace VictorBush.Ego.NefsEdit.UI
{
    partial class OpenFileForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpenFileForm));
			this.cancelButton = new Button();
			this.openButton = new Button();
			this.tablessControl1 = new TablessControl();
			this.nefsTabPage = new TabPage();
			this.fileLabel = new Label();
			this.nefsFileButton = new Button();
			this.nefsFileTextBox = new TextBox();
			this.headlessTabPage = new TabPage();
			this.gameDatRefreshButton = new Button();
			this.gameDatFilesLabel = new Label();
			this.gameDatFilesListBox = new ListBox();
			this.gameExeFileButton = new Button();
			this.headlessGameExeFileTextBox = new TextBox();
			this.gameExeFileLabel = new Label();
			this.headlessDataDirButton = new Button();
			this.headlessDataDirTextBox = new TextBox();
			this.headlessDataDirLabel = new Label();
			this.headlessCustomTabPage = new TabPage();
			this.label6 = new Label();
			this.splitSecondarySizeTextBox = new TextBox();
			this.label7 = new Label();
			this.splitSecondaryOffsetTextBox = new TextBox();
			this.label5 = new Label();
			this.splitPrimarySizeTextBox = new TextBox();
			this.primaryOffsetLabel = new Label();
			this.splitPrimaryOffsetTextBox = new TextBox();
			this.label4 = new Label();
			this.splitHeaderFileBrowseButton = new Button();
			this.splitHeaderFileTextBox = new TextBox();
			this.label3 = new Label();
			this.splitDataFileBrowseButton = new Button();
			this.splitDataFileTextBox = new TextBox();
			this.recentTabPage = new TabPage();
			this.recentListBox = new ListBox();
			this.label2 = new Label();
			this.modeListBox = new ListBox();
			this.tablessControl1.SuspendLayout();
			this.nefsTabPage.SuspendLayout();
			this.headlessTabPage.SuspendLayout();
			this.headlessCustomTabPage.SuspendLayout();
			this.recentTabPage.SuspendLayout();
			SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.cancelButton.DialogResult = DialogResult.Cancel;
			this.cancelButton.Location = new Point(445, 330);
			this.cancelButton.Margin = new Padding(4, 4, 4, 4);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new Size(88, 28);
			this.cancelButton.TabIndex = 24;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// openButton
			// 
			this.openButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.openButton.Location = new Point(351, 330);
			this.openButton.Margin = new Padding(4, 4, 4, 4);
			this.openButton.Name = "openButton";
			this.openButton.Size = new Size(88, 28);
			this.openButton.TabIndex = 23;
			this.openButton.Text = "Open";
			this.openButton.UseVisualStyleBackColor = true;
			this.openButton.Click += OpenButton_Click;
			// 
			// tablessControl1
			// 
			this.tablessControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.tablessControl1.Appearance = TabAppearance.FlatButtons;
			this.tablessControl1.Controls.Add(this.nefsTabPage);
			this.tablessControl1.Controls.Add(this.headlessTabPage);
			this.tablessControl1.Controls.Add(this.headlessCustomTabPage);
			this.tablessControl1.Controls.Add(this.recentTabPage);
			this.tablessControl1.Location = new Point(164, 11);
			this.tablessControl1.Margin = new Padding(4, 4, 4, 4);
			this.tablessControl1.Name = "tablessControl1";
			this.tablessControl1.SelectedIndex = 0;
			this.tablessControl1.Size = new Size(374, 310);
			this.tablessControl1.TabIndex = 18;
			// 
			// nefsTabPage
			// 
			this.nefsTabPage.Controls.Add(this.fileLabel);
			this.nefsTabPage.Controls.Add(this.nefsFileButton);
			this.nefsTabPage.Controls.Add(this.nefsFileTextBox);
			this.nefsTabPage.Location = new Point(4, 28);
			this.nefsTabPage.Margin = new Padding(4, 4, 4, 4);
			this.nefsTabPage.Name = "nefsTabPage";
			this.nefsTabPage.Padding = new Padding(4, 4, 4, 4);
			this.nefsTabPage.Size = new Size(366, 278);
			this.nefsTabPage.TabIndex = 0;
			this.nefsTabPage.Text = "NeFS";
			this.nefsTabPage.UseVisualStyleBackColor = true;
			// 
			// fileLabel
			// 
			this.fileLabel.AutoSize = true;
			this.fileLabel.Location = new Point(7, 10);
			this.fileLabel.Margin = new Padding(4, 0, 4, 0);
			this.fileLabel.Name = "fileLabel";
			this.fileLabel.Size = new Size(26, 16);
			this.fileLabel.TabIndex = 0;
			this.fileLabel.Text = "File";
			// 
			// nefsFileButton
			// 
			this.nefsFileButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.nefsFileButton.Location = new Point(325, 27);
			this.nefsFileButton.Margin = new Padding(4, 4, 4, 4);
			this.nefsFileButton.Name = "nefsFileButton";
			this.nefsFileButton.Size = new Size(31, 28);
			this.nefsFileButton.TabIndex = 1;
			this.nefsFileButton.Text = "...";
			this.nefsFileButton.UseVisualStyleBackColor = true;
			this.nefsFileButton.Click += NefsFileButton_Click;
			// 
			// nefsFileTextBox
			// 
			this.nefsFileTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.nefsFileTextBox.Location = new Point(10, 30);
			this.nefsFileTextBox.Margin = new Padding(4, 4, 4, 4);
			this.nefsFileTextBox.Name = "nefsFileTextBox";
			this.nefsFileTextBox.Size = new Size(306, 23);
			this.nefsFileTextBox.TabIndex = 0;
			// 
			// headlessTabPage
			// 
			this.headlessTabPage.Controls.Add(this.gameDatRefreshButton);
			this.headlessTabPage.Controls.Add(this.gameDatFilesLabel);
			this.headlessTabPage.Controls.Add(this.gameDatFilesListBox);
			this.headlessTabPage.Controls.Add(this.gameExeFileButton);
			this.headlessTabPage.Controls.Add(this.headlessGameExeFileTextBox);
			this.headlessTabPage.Controls.Add(this.gameExeFileLabel);
			this.headlessTabPage.Controls.Add(this.headlessDataDirButton);
			this.headlessTabPage.Controls.Add(this.headlessDataDirTextBox);
			this.headlessTabPage.Controls.Add(this.headlessDataDirLabel);
			this.headlessTabPage.Location = new Point(4, 28);
			this.headlessTabPage.Margin = new Padding(4, 4, 4, 4);
			this.headlessTabPage.Name = "headlessTabPage";
			this.headlessTabPage.Padding = new Padding(4, 4, 4, 4);
			this.headlessTabPage.Size = new Size(366, 278);
			this.headlessTabPage.TabIndex = 1;
			this.headlessTabPage.Text = "Headless";
			this.headlessTabPage.UseVisualStyleBackColor = true;
			// 
			// gameDatRefreshButton
			// 
			this.gameDatRefreshButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			this.gameDatRefreshButton.Location = new Point(11, 244);
			this.gameDatRefreshButton.Margin = new Padding(4, 4, 4, 4);
			this.gameDatRefreshButton.Name = "gameDatRefreshButton";
			this.gameDatRefreshButton.Size = new Size(88, 28);
			this.gameDatRefreshButton.TabIndex = 25;
			this.gameDatRefreshButton.Text = "Search";
			this.gameDatRefreshButton.UseVisualStyleBackColor = true;
			this.gameDatRefreshButton.Click += GameDatRefreshButton_Click;
			// 
			// gameDatFilesLabel
			// 
			this.gameDatFilesLabel.AutoSize = true;
			this.gameDatFilesLabel.Location = new Point(8, 118);
			this.gameDatFilesLabel.Margin = new Padding(4, 0, 4, 0);
			this.gameDatFilesLabel.Name = "gameDatFilesLabel";
			this.gameDatFilesLabel.Size = new Size(62, 16);
			this.gameDatFilesLabel.TabIndex = 16;
			this.gameDatFilesLabel.Text = "Data Files";
			// 
			// gameDatFilesListBox
			// 
			this.gameDatFilesListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.gameDatFilesListBox.FormattingEnabled = true;
			this.gameDatFilesListBox.Location = new Point(11, 141);
			this.gameDatFilesListBox.Margin = new Padding(4, 4, 4, 4);
			this.gameDatFilesListBox.Name = "gameDatFilesListBox";
			this.gameDatFilesListBox.Size = new Size(343, 100);
			this.gameDatFilesListBox.TabIndex = 16;
			// 
			// gameExeFileButton
			// 
			this.gameExeFileButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.gameExeFileButton.Location = new Point(326, 27);
			this.gameExeFileButton.Margin = new Padding(4, 4, 4, 4);
			this.gameExeFileButton.Name = "gameExeFileButton";
			this.gameExeFileButton.Size = new Size(31, 28);
			this.gameExeFileButton.TabIndex = 13;
			this.gameExeFileButton.Text = "...";
			this.gameExeFileButton.UseVisualStyleBackColor = true;
			this.gameExeFileButton.Click += GameExeFileButton_Click;
			// 
			// headlessGameExeFileTextBox
			// 
			this.headlessGameExeFileTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.headlessGameExeFileTextBox.Location = new Point(11, 30);
			this.headlessGameExeFileTextBox.Margin = new Padding(4, 4, 4, 4);
			this.headlessGameExeFileTextBox.Name = "headlessGameExeFileTextBox";
			this.headlessGameExeFileTextBox.Size = new Size(306, 23);
			this.headlessGameExeFileTextBox.TabIndex = 12;
			// 
			// gameExeFileLabel
			// 
			this.gameExeFileLabel.AutoSize = true;
			this.gameExeFileLabel.Location = new Point(8, 10);
			this.gameExeFileLabel.Margin = new Padding(4, 0, 4, 0);
			this.gameExeFileLabel.Name = "gameExeFileLabel";
			this.gameExeFileLabel.Size = new Size(105, 16);
			this.gameExeFileLabel.TabIndex = 12;
			this.gameExeFileLabel.Text = "Game Executable";
			// 
			// headlessDataDirButton
			// 
			this.headlessDataDirButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.headlessDataDirButton.Location = new Point(326, 82);
			this.headlessDataDirButton.Margin = new Padding(4, 4, 4, 4);
			this.headlessDataDirButton.Name = "headlessDataDirButton";
			this.headlessDataDirButton.Size = new Size(31, 28);
			this.headlessDataDirButton.TabIndex = 15;
			this.headlessDataDirButton.Text = "...";
			this.headlessDataDirButton.UseVisualStyleBackColor = true;
			this.headlessDataDirButton.Click += HeadlessDataDirButton_Click;
			// 
			// headlessDataDirTextBox
			// 
			this.headlessDataDirTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.headlessDataDirTextBox.Location = new Point(11, 85);
			this.headlessDataDirTextBox.Margin = new Padding(4, 4, 4, 4);
			this.headlessDataDirTextBox.Name = "headlessDataDirTextBox";
			this.headlessDataDirTextBox.Size = new Size(306, 23);
			this.headlessDataDirTextBox.TabIndex = 14;
			// 
			// headlessDataDirLabel
			// 
			this.headlessDataDirLabel.AutoSize = true;
			this.headlessDataDirLabel.Location = new Point(8, 64);
			this.headlessDataDirLabel.Margin = new Padding(4, 0, 4, 0);
			this.headlessDataDirLabel.Name = "headlessDataDirLabel";
			this.headlessDataDirLabel.Size = new Size(91, 16);
			this.headlessDataDirLabel.TabIndex = 14;
			this.headlessDataDirLabel.Text = "Data Directory";
			// 
			// headlessCustomTabPage
			// 
			this.headlessCustomTabPage.Controls.Add(this.label6);
			this.headlessCustomTabPage.Controls.Add(this.splitSecondarySizeTextBox);
			this.headlessCustomTabPage.Controls.Add(this.label7);
			this.headlessCustomTabPage.Controls.Add(this.splitSecondaryOffsetTextBox);
			this.headlessCustomTabPage.Controls.Add(this.label5);
			this.headlessCustomTabPage.Controls.Add(this.splitPrimarySizeTextBox);
			this.headlessCustomTabPage.Controls.Add(this.primaryOffsetLabel);
			this.headlessCustomTabPage.Controls.Add(this.splitPrimaryOffsetTextBox);
			this.headlessCustomTabPage.Controls.Add(this.label4);
			this.headlessCustomTabPage.Controls.Add(this.splitHeaderFileBrowseButton);
			this.headlessCustomTabPage.Controls.Add(this.splitHeaderFileTextBox);
			this.headlessCustomTabPage.Controls.Add(this.label3);
			this.headlessCustomTabPage.Controls.Add(this.splitDataFileBrowseButton);
			this.headlessCustomTabPage.Controls.Add(this.splitDataFileTextBox);
			this.headlessCustomTabPage.Location = new Point(4, 28);
			this.headlessCustomTabPage.Name = "headlessCustomTabPage";
			this.headlessCustomTabPage.Padding = new Padding(3, 3, 3, 3);
			this.headlessCustomTabPage.Size = new Size(366, 278);
			this.headlessCustomTabPage.TabIndex = 4;
			this.headlessCustomTabPage.Text = "Headless (Custom)";
			this.headlessCustomTabPage.UseVisualStyleBackColor = true;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new Point(164, 179);
			this.label6.Margin = new Padding(4, 0, 4, 0);
			this.label6.Name = "label6";
			this.label6.Size = new Size(89, 16);
			this.label6.TabIndex = 32;
			this.label6.Text = "Secondary Size";
			// 
			// splitSecondarySizeTextBox
			// 
			this.splitSecondarySizeTextBox.Location = new Point(167, 200);
			this.splitSecondarySizeTextBox.Margin = new Padding(4, 4, 4, 4);
			this.splitSecondarySizeTextBox.Name = "splitSecondarySizeTextBox";
			this.splitSecondarySizeTextBox.Size = new Size(99, 23);
			this.splitSecondarySizeTextBox.TabIndex = 33;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new Point(10, 179);
			this.label7.Margin = new Padding(4, 0, 4, 0);
			this.label7.Name = "label7";
			this.label7.Size = new Size(136, 16);
			this.label7.TabIndex = 30;
			this.label7.Text = "Secondary Offset (Hex)";
			// 
			// splitSecondaryOffsetTextBox
			// 
			this.splitSecondaryOffsetTextBox.Location = new Point(13, 200);
			this.splitSecondaryOffsetTextBox.Margin = new Padding(4, 4, 4, 4);
			this.splitSecondaryOffsetTextBox.Name = "splitSecondaryOffsetTextBox";
			this.splitSecondaryOffsetTextBox.Size = new Size(99, 23);
			this.splitSecondaryOffsetTextBox.TabIndex = 31;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new Point(163, 122);
			this.label5.Margin = new Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new Size(74, 16);
			this.label5.TabIndex = 28;
			this.label5.Text = "Primary Size";
			// 
			// splitPrimarySizeTextBox
			// 
			this.splitPrimarySizeTextBox.Location = new Point(166, 143);
			this.splitPrimarySizeTextBox.Margin = new Padding(4, 4, 4, 4);
			this.splitPrimarySizeTextBox.Name = "splitPrimarySizeTextBox";
			this.splitPrimarySizeTextBox.Size = new Size(99, 23);
			this.splitPrimarySizeTextBox.TabIndex = 29;
			// 
			// primaryOffsetLabel
			// 
			this.primaryOffsetLabel.AutoSize = true;
			this.primaryOffsetLabel.Location = new Point(9, 122);
			this.primaryOffsetLabel.Margin = new Padding(4, 0, 4, 0);
			this.primaryOffsetLabel.Name = "primaryOffsetLabel";
			this.primaryOffsetLabel.Size = new Size(121, 16);
			this.primaryOffsetLabel.TabIndex = 26;
			this.primaryOffsetLabel.Text = "Primary Offset (Hex)";
			// 
			// splitPrimaryOffsetTextBox
			// 
			this.splitPrimaryOffsetTextBox.Location = new Point(12, 143);
			this.splitPrimaryOffsetTextBox.Margin = new Padding(4, 4, 4, 4);
			this.splitPrimaryOffsetTextBox.Name = "splitPrimaryOffsetTextBox";
			this.splitPrimaryOffsetTextBox.Size = new Size(99, 23);
			this.splitPrimaryOffsetTextBox.TabIndex = 27;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new Point(8, 66);
			this.label4.Margin = new Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new Size(199, 16);
			this.label4.TabIndex = 5;
			this.label4.Text = "Header File (i.e., game executable)";
			// 
			// splitHeaderFileBrowseButton
			// 
			this.splitHeaderFileBrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.splitHeaderFileBrowseButton.Location = new Point(326, 83);
			this.splitHeaderFileBrowseButton.Margin = new Padding(4, 4, 4, 4);
			this.splitHeaderFileBrowseButton.Name = "splitHeaderFileBrowseButton";
			this.splitHeaderFileBrowseButton.Size = new Size(31, 28);
			this.splitHeaderFileBrowseButton.TabIndex = 7;
			this.splitHeaderFileBrowseButton.Text = "...";
			this.splitHeaderFileBrowseButton.UseVisualStyleBackColor = true;
			this.splitHeaderFileBrowseButton.Click += splitHeaderFileBrowseButton_Click;
			// 
			// splitHeaderFileTextBox
			// 
			this.splitHeaderFileTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.splitHeaderFileTextBox.Location = new Point(11, 86);
			this.splitHeaderFileTextBox.Margin = new Padding(4, 4, 4, 4);
			this.splitHeaderFileTextBox.Name = "splitHeaderFileTextBox";
			this.splitHeaderFileTextBox.Size = new Size(306, 23);
			this.splitHeaderFileTextBox.TabIndex = 6;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new Point(7, 10);
			this.label3.Margin = new Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new Size(228, 16);
			this.label3.TabIndex = 2;
			this.label3.Text = "Data File (i.e., game.dat, game.bin, etc.)";
			// 
			// splitDataFileBrowseButton
			// 
			this.splitDataFileBrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.splitDataFileBrowseButton.Location = new Point(325, 27);
			this.splitDataFileBrowseButton.Margin = new Padding(4, 4, 4, 4);
			this.splitDataFileBrowseButton.Name = "splitDataFileBrowseButton";
			this.splitDataFileBrowseButton.Size = new Size(31, 28);
			this.splitDataFileBrowseButton.TabIndex = 4;
			this.splitDataFileBrowseButton.Text = "...";
			this.splitDataFileBrowseButton.UseVisualStyleBackColor = true;
			this.splitDataFileBrowseButton.Click += splitDataFileBrowseButton_Click;
			// 
			// splitDataFileTextBox
			// 
			this.splitDataFileTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.splitDataFileTextBox.Location = new Point(10, 30);
			this.splitDataFileTextBox.Margin = new Padding(4, 4, 4, 4);
			this.splitDataFileTextBox.Name = "splitDataFileTextBox";
			this.splitDataFileTextBox.Size = new Size(306, 23);
			this.splitDataFileTextBox.TabIndex = 3;
			// 
			// recentTabPage
			// 
			this.recentTabPage.Controls.Add(this.recentListBox);
			this.recentTabPage.Location = new Point(4, 28);
			this.recentTabPage.Margin = new Padding(3, 2, 3, 2);
			this.recentTabPage.Name = "recentTabPage";
			this.recentTabPage.Padding = new Padding(3, 2, 3, 2);
			this.recentTabPage.Size = new Size(366, 278);
			this.recentTabPage.TabIndex = 3;
			this.recentTabPage.Text = "Recent";
			this.recentTabPage.UseVisualStyleBackColor = true;
			// 
			// recentListBox
			// 
			this.recentListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.recentListBox.FormattingEnabled = true;
			this.recentListBox.HorizontalScrollbar = true;
			this.recentListBox.Location = new Point(4, 5);
			this.recentListBox.Margin = new Padding(3, 2, 3, 2);
			this.recentListBox.Name = "recentListBox";
			this.recentListBox.Size = new Size(360, 260);
			this.recentListBox.TabIndex = 26;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new Point(8, 12);
			this.label2.Margin = new Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new Size(269, 20);
			this.label2.TabIndex = 8;
			this.label2.Text = "Data File (i.e., game.dat, game.bin, etc.)";
			// 
			// modeListBox
			// 
			this.modeListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			this.modeListBox.FormattingEnabled = true;
			this.modeListBox.Location = new Point(10, 10);
			this.modeListBox.Margin = new Padding(3, 2, 3, 2);
			this.modeListBox.Name = "modeListBox";
			this.modeListBox.Size = new Size(150, 308);
			this.modeListBox.TabIndex = 25;
			this.modeListBox.SelectedIndexChanged += ModeListBox_SelectedIndexChanged;
			// 
			// OpenFileForm
			// 
			AcceptButton = this.openButton;
			AutoScaleDimensions = new SizeF(7F, 16F);
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = this.cancelButton;
			ClientSize = new Size(547, 373);
			Controls.Add(this.modeListBox);
			Controls.Add(this.tablessControl1);
			Controls.Add(this.openButton);
			Controls.Add(this.cancelButton);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			Icon = (Icon)resources.GetObject("$this.Icon");
			Margin = new Padding(4, 4, 4, 4);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "OpenFileForm";
			SizeGripStyle = SizeGripStyle.Hide;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Open";
			FormClosing += OpenFileForm_FormClosing;
			Load += OpenFileForm_Load;
			this.tablessControl1.ResumeLayout(false);
			this.nefsTabPage.ResumeLayout(false);
			this.nefsTabPage.PerformLayout();
			this.headlessTabPage.ResumeLayout(false);
			this.headlessTabPage.PerformLayout();
			this.headlessCustomTabPage.ResumeLayout(false);
			this.headlessCustomTabPage.PerformLayout();
			this.recentTabPage.ResumeLayout(false);
			ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TextBox nefsFileTextBox;
        private System.Windows.Forms.Label fileLabel;
        private System.Windows.Forms.Button nefsFileButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button openButton;
        private TablessControl tablessControl1;
        private System.Windows.Forms.TabPage nefsTabPage;
        private System.Windows.Forms.TabPage headlessTabPage;
        private System.Windows.Forms.Button gameExeFileButton;
        private System.Windows.Forms.TextBox headlessGameExeFileTextBox;
        private System.Windows.Forms.Label gameExeFileLabel;
        private System.Windows.Forms.Button headlessDataDirButton;
        private System.Windows.Forms.TextBox headlessDataDirTextBox;
        private System.Windows.Forms.Label headlessDataDirLabel;
        private System.Windows.Forms.Label gameDatFilesLabel;
        private System.Windows.Forms.ListBox gameDatFilesListBox;
        private System.Windows.Forms.Button gameDatRefreshButton;
        private System.Windows.Forms.ListBox modeListBox;
        private System.Windows.Forms.TabPage recentTabPage;
        private System.Windows.Forms.ListBox recentListBox;
        private System.Windows.Forms.TabPage headlessCustomTabPage;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox splitSecondarySizeTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox splitSecondaryOffsetTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox splitPrimarySizeTextBox;
        private System.Windows.Forms.Label primaryOffsetLabel;
        private System.Windows.Forms.TextBox splitPrimaryOffsetTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button splitHeaderFileBrowseButton;
        private System.Windows.Forms.TextBox splitHeaderFileTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button splitDataFileBrowseButton;
        private System.Windows.Forms.TextBox splitDataFileTextBox;
        private System.Windows.Forms.Label label2;
    }
}
