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
            this.cancelButton = new System.Windows.Forms.Button();
            this.openButton = new System.Windows.Forms.Button();
            this.tablessControl1 = new VictorBush.Ego.NefsEdit.UI.TablessControl();
            this.nefsTabPage = new System.Windows.Forms.TabPage();
            this.advancedGroupBox = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.headerPart6OffsetTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.headerPart6OffsetLabel = new System.Windows.Forms.Label();
            this.dataFileTextBox = new System.Windows.Forms.TextBox();
            this.headerOffsetTextBox = new System.Windows.Forms.TextBox();
            this.dataFileBrowseButton = new System.Windows.Forms.Button();
            this.advancedCheckBox = new System.Windows.Forms.CheckBox();
            this.fileLabel = new System.Windows.Forms.Label();
            this.nefsFileButton = new System.Windows.Forms.Button();
            this.nefsFileTextBox = new System.Windows.Forms.TextBox();
            this.recentTabPage = new System.Windows.Forms.TabPage();
            this.recentListBox = new System.Windows.Forms.ListBox();
            this.gameDatTabPage = new System.Windows.Forms.TabPage();
            this.gameDatDirButton = new System.Windows.Forms.Button();
            this.gameDatDirTextBox = new System.Windows.Forms.TextBox();
            this.gameDatDirLabel = new System.Windows.Forms.Label();
            this.gameDatRefreshButton = new System.Windows.Forms.Button();
            this.gameDatFilesLabel = new System.Windows.Forms.Label();
            this.gameDatFilesListBox = new System.Windows.Forms.ListBox();
            this.gameExeFileButton = new System.Windows.Forms.Button();
            this.gameExeFileTextBox = new System.Windows.Forms.TextBox();
            this.gameExeFileLabel = new System.Windows.Forms.Label();
            this.modeListBox = new System.Windows.Forms.ListBox();
            this.tablessControl1.SuspendLayout();
            this.nefsTabPage.SuspendLayout();
            this.advancedGroupBox.SuspendLayout();
            this.recentTabPage.SuspendLayout();
            this.gameDatTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(382, 268);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 24;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // openButton
            // 
            this.openButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.openButton.Location = new System.Drawing.Point(301, 268);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(75, 23);
            this.openButton.TabIndex = 23;
            this.openButton.Text = "Open";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.OpenButton_Click);
            // 
            // tablessControl1
            // 
            this.tablessControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tablessControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tablessControl1.Controls.Add(this.nefsTabPage);
            this.tablessControl1.Controls.Add(this.recentTabPage);
            this.tablessControl1.Controls.Add(this.gameDatTabPage);
            this.tablessControl1.Location = new System.Drawing.Point(140, 9);
            this.tablessControl1.Name = "tablessControl1";
            this.tablessControl1.SelectedIndex = 0;
            this.tablessControl1.Size = new System.Drawing.Size(321, 252);
            this.tablessControl1.TabIndex = 18;
            // 
            // nefsTabPage
            // 
            this.nefsTabPage.Controls.Add(this.advancedGroupBox);
            this.nefsTabPage.Controls.Add(this.advancedCheckBox);
            this.nefsTabPage.Controls.Add(this.fileLabel);
            this.nefsTabPage.Controls.Add(this.nefsFileButton);
            this.nefsTabPage.Controls.Add(this.nefsFileTextBox);
            this.nefsTabPage.Location = new System.Drawing.Point(4, 25);
            this.nefsTabPage.Name = "nefsTabPage";
            this.nefsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.nefsTabPage.Size = new System.Drawing.Size(313, 223);
            this.nefsTabPage.TabIndex = 0;
            this.nefsTabPage.Text = "NeFS";
            this.nefsTabPage.UseVisualStyleBackColor = true;
            // 
            // advancedGroupBox
            // 
            this.advancedGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.advancedGroupBox.Controls.Add(this.label1);
            this.advancedGroupBox.Controls.Add(this.headerPart6OffsetTextBox);
            this.advancedGroupBox.Controls.Add(this.label2);
            this.advancedGroupBox.Controls.Add(this.headerPart6OffsetLabel);
            this.advancedGroupBox.Controls.Add(this.dataFileTextBox);
            this.advancedGroupBox.Controls.Add(this.headerOffsetTextBox);
            this.advancedGroupBox.Controls.Add(this.dataFileBrowseButton);
            this.advancedGroupBox.Location = new System.Drawing.Point(5, 69);
            this.advancedGroupBox.Margin = new System.Windows.Forms.Padding(2);
            this.advancedGroupBox.Name = "advancedGroupBox";
            this.advancedGroupBox.Padding = new System.Windows.Forms.Padding(2);
            this.advancedGroupBox.Size = new System.Drawing.Size(306, 155);
            this.advancedGroupBox.TabIndex = 30;
            this.advancedGroupBox.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 24;
            this.label1.Text = "Header Offset";
            // 
            // headerPart6OffsetTextBox
            // 
            this.headerPart6OffsetTextBox.Location = new System.Drawing.Point(7, 68);
            this.headerPart6OffsetTextBox.Name = "headerPart6OffsetTextBox";
            this.headerPart6OffsetTextBox.Size = new System.Drawing.Size(86, 20);
            this.headerPart6OffsetTextBox.TabIndex = 29;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 23;
            this.label2.Text = "Data File";
            // 
            // headerPart6OffsetLabel
            // 
            this.headerPart6OffsetLabel.AutoSize = true;
            this.headerPart6OffsetLabel.Location = new System.Drawing.Point(3, 52);
            this.headerPart6OffsetLabel.Name = "headerPart6OffsetLabel";
            this.headerPart6OffsetLabel.Size = new System.Drawing.Size(104, 13);
            this.headerPart6OffsetLabel.TabIndex = 28;
            this.headerPart6OffsetLabel.Text = "Header Part 6 Offset";
            // 
            // dataFileTextBox
            // 
            this.dataFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataFileTextBox.Location = new System.Drawing.Point(7, 107);
            this.dataFileTextBox.Name = "dataFileTextBox";
            this.dataFileTextBox.Size = new System.Drawing.Size(259, 20);
            this.dataFileTextBox.TabIndex = 26;
            // 
            // headerOffsetTextBox
            // 
            this.headerOffsetTextBox.Location = new System.Drawing.Point(6, 29);
            this.headerOffsetTextBox.Name = "headerOffsetTextBox";
            this.headerOffsetTextBox.Size = new System.Drawing.Size(86, 20);
            this.headerOffsetTextBox.TabIndex = 25;
            // 
            // dataFileBrowseButton
            // 
            this.dataFileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dataFileBrowseButton.Location = new System.Drawing.Point(272, 105);
            this.dataFileBrowseButton.Name = "dataFileBrowseButton";
            this.dataFileBrowseButton.Size = new System.Drawing.Size(26, 23);
            this.dataFileBrowseButton.TabIndex = 27;
            this.dataFileBrowseButton.Text = "...";
            this.dataFileBrowseButton.UseVisualStyleBackColor = true;
            this.dataFileBrowseButton.Click += new System.EventHandler(this.DataFileBrowseButton_Click);
            // 
            // advancedCheckBox
            // 
            this.advancedCheckBox.AutoSize = true;
            this.advancedCheckBox.Location = new System.Drawing.Point(10, 54);
            this.advancedCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.advancedCheckBox.Name = "advancedCheckBox";
            this.advancedCheckBox.Size = new System.Drawing.Size(75, 17);
            this.advancedCheckBox.TabIndex = 2;
            this.advancedCheckBox.Text = "Advanced";
            this.advancedCheckBox.UseVisualStyleBackColor = true;
            this.advancedCheckBox.CheckedChanged += new System.EventHandler(this.AdvancedCheckBox_CheckedChanged);
            // 
            // fileLabel
            // 
            this.fileLabel.AutoSize = true;
            this.fileLabel.Location = new System.Drawing.Point(6, 8);
            this.fileLabel.Name = "fileLabel";
            this.fileLabel.Size = new System.Drawing.Size(23, 13);
            this.fileLabel.TabIndex = 0;
            this.fileLabel.Text = "File";
            // 
            // nefsFileButton
            // 
            this.nefsFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nefsFileButton.Location = new System.Drawing.Point(278, 22);
            this.nefsFileButton.Name = "nefsFileButton";
            this.nefsFileButton.Size = new System.Drawing.Size(26, 23);
            this.nefsFileButton.TabIndex = 1;
            this.nefsFileButton.Text = "...";
            this.nefsFileButton.UseVisualStyleBackColor = true;
            this.nefsFileButton.Click += new System.EventHandler(this.NefsFileButton_Click);
            // 
            // nefsFileTextBox
            // 
            this.nefsFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nefsFileTextBox.Location = new System.Drawing.Point(9, 24);
            this.nefsFileTextBox.Name = "nefsFileTextBox";
            this.nefsFileTextBox.Size = new System.Drawing.Size(263, 20);
            this.nefsFileTextBox.TabIndex = 0;
            // 
            // recentTabPage
            // 
            this.recentTabPage.Controls.Add(this.recentListBox);
            this.recentTabPage.Location = new System.Drawing.Point(4, 25);
            this.recentTabPage.Margin = new System.Windows.Forms.Padding(2);
            this.recentTabPage.Name = "recentTabPage";
            this.recentTabPage.Padding = new System.Windows.Forms.Padding(2);
            this.recentTabPage.Size = new System.Drawing.Size(313, 223);
            this.recentTabPage.TabIndex = 3;
            this.recentTabPage.Text = "Recent";
            this.recentTabPage.UseVisualStyleBackColor = true;
            // 
            // recentListBox
            // 
            this.recentListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.recentListBox.FormattingEnabled = true;
            this.recentListBox.Location = new System.Drawing.Point(4, 4);
            this.recentListBox.Margin = new System.Windows.Forms.Padding(2);
            this.recentListBox.Name = "recentListBox";
            this.recentListBox.Size = new System.Drawing.Size(309, 212);
            this.recentListBox.TabIndex = 26;
            // 
            // gameDatTabPage
            // 
            this.gameDatTabPage.Controls.Add(this.gameDatDirButton);
            this.gameDatTabPage.Controls.Add(this.gameDatDirTextBox);
            this.gameDatTabPage.Controls.Add(this.gameDatDirLabel);
            this.gameDatTabPage.Controls.Add(this.gameDatRefreshButton);
            this.gameDatTabPage.Controls.Add(this.gameDatFilesLabel);
            this.gameDatTabPage.Controls.Add(this.gameDatFilesListBox);
            this.gameDatTabPage.Controls.Add(this.gameExeFileButton);
            this.gameDatTabPage.Controls.Add(this.gameExeFileTextBox);
            this.gameDatTabPage.Controls.Add(this.gameExeFileLabel);
            this.gameDatTabPage.Location = new System.Drawing.Point(4, 25);
            this.gameDatTabPage.Name = "gameDatTabPage";
            this.gameDatTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.gameDatTabPage.Size = new System.Drawing.Size(313, 223);
            this.gameDatTabPage.TabIndex = 1;
            this.gameDatTabPage.Text = "game*.dat";
            this.gameDatTabPage.UseVisualStyleBackColor = true;
            // 
            // gameDatDirButton
            // 
            this.gameDatDirButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gameDatDirButton.Location = new System.Drawing.Point(281, 66);
            this.gameDatDirButton.Name = "gameDatDirButton";
            this.gameDatDirButton.Size = new System.Drawing.Size(26, 23);
            this.gameDatDirButton.TabIndex = 28;
            this.gameDatDirButton.Text = "...";
            this.gameDatDirButton.UseVisualStyleBackColor = true;
            this.gameDatDirButton.Click += new System.EventHandler(this.GameDatDirButton_Click);
            // 
            // gameDatDirTextBox
            // 
            this.gameDatDirTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gameDatDirTextBox.Location = new System.Drawing.Point(11, 68);
            this.gameDatDirTextBox.Name = "gameDatDirTextBox";
            this.gameDatDirTextBox.Size = new System.Drawing.Size(263, 20);
            this.gameDatDirTextBox.TabIndex = 26;
            // 
            // gameDatDirLabel
            // 
            this.gameDatDirLabel.AutoSize = true;
            this.gameDatDirLabel.Location = new System.Drawing.Point(8, 52);
            this.gameDatDirLabel.Name = "gameDatDirLabel";
            this.gameDatDirLabel.Size = new System.Drawing.Size(100, 13);
            this.gameDatDirLabel.TabIndex = 27;
            this.gameDatDirLabel.Text = "game*.dat Directory";
            // 
            // gameDatRefreshButton
            // 
            this.gameDatRefreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.gameDatRefreshButton.Location = new System.Drawing.Point(10, 200);
            this.gameDatRefreshButton.Name = "gameDatRefreshButton";
            this.gameDatRefreshButton.Size = new System.Drawing.Size(75, 23);
            this.gameDatRefreshButton.TabIndex = 25;
            this.gameDatRefreshButton.Text = "Search";
            this.gameDatRefreshButton.UseVisualStyleBackColor = true;
            this.gameDatRefreshButton.Click += new System.EventHandler(this.GameDatRefreshButton_Click);
            // 
            // gameDatFilesLabel
            // 
            this.gameDatFilesLabel.AutoSize = true;
            this.gameDatFilesLabel.Location = new System.Drawing.Point(8, 96);
            this.gameDatFilesLabel.Name = "gameDatFilesLabel";
            this.gameDatFilesLabel.Size = new System.Drawing.Size(79, 13);
            this.gameDatFilesLabel.TabIndex = 15;
            this.gameDatFilesLabel.Text = "Game Dat Files";
            // 
            // gameDatFilesListBox
            // 
            this.gameDatFilesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gameDatFilesListBox.FormattingEnabled = true;
            this.gameDatFilesListBox.Location = new System.Drawing.Point(10, 112);
            this.gameDatFilesListBox.Name = "gameDatFilesListBox";
            this.gameDatFilesListBox.Size = new System.Drawing.Size(294, 82);
            this.gameDatFilesListBox.TabIndex = 14;
            // 
            // gameExeFileButton
            // 
            this.gameExeFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gameExeFileButton.Location = new System.Drawing.Point(279, 22);
            this.gameExeFileButton.Name = "gameExeFileButton";
            this.gameExeFileButton.Size = new System.Drawing.Size(26, 23);
            this.gameExeFileButton.TabIndex = 13;
            this.gameExeFileButton.Text = "...";
            this.gameExeFileButton.UseVisualStyleBackColor = true;
            this.gameExeFileButton.Click += new System.EventHandler(this.GameExeFileButton_Click);
            // 
            // gameExeFileTextBox
            // 
            this.gameExeFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gameExeFileTextBox.Location = new System.Drawing.Point(10, 24);
            this.gameExeFileTextBox.Name = "gameExeFileTextBox";
            this.gameExeFileTextBox.Size = new System.Drawing.Size(263, 20);
            this.gameExeFileTextBox.TabIndex = 12;
            // 
            // gameExeFileLabel
            // 
            this.gameExeFileLabel.AutoSize = true;
            this.gameExeFileLabel.Location = new System.Drawing.Point(7, 8);
            this.gameExeFileLabel.Name = "gameExeFileLabel";
            this.gameExeFileLabel.Size = new System.Drawing.Size(91, 13);
            this.gameExeFileLabel.TabIndex = 12;
            this.gameExeFileLabel.Text = "Game Executable";
            // 
            // modeListBox
            // 
            this.modeListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.modeListBox.FormattingEnabled = true;
            this.modeListBox.Location = new System.Drawing.Point(8, 8);
            this.modeListBox.Margin = new System.Windows.Forms.Padding(2);
            this.modeListBox.Name = "modeListBox";
            this.modeListBox.Size = new System.Drawing.Size(129, 251);
            this.modeListBox.TabIndex = 25;
            this.modeListBox.SelectedIndexChanged += new System.EventHandler(this.ModeListBox_SelectedIndexChanged);
            // 
            // OpenFileForm
            // 
            this.AcceptButton = this.openButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(469, 303);
            this.Controls.Add(this.modeListBox);
            this.Controls.Add(this.tablessControl1);
            this.Controls.Add(this.openButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OpenFileForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Open";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OpenFileForm_FormClosing);
            this.Load += new System.EventHandler(this.OpenFileForm_Load);
            this.tablessControl1.ResumeLayout(false);
            this.nefsTabPage.ResumeLayout(false);
            this.nefsTabPage.PerformLayout();
            this.advancedGroupBox.ResumeLayout(false);
            this.advancedGroupBox.PerformLayout();
            this.recentTabPage.ResumeLayout(false);
            this.gameDatTabPage.ResumeLayout(false);
            this.gameDatTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox nefsFileTextBox;
        private System.Windows.Forms.Label fileLabel;
        private System.Windows.Forms.Button nefsFileButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button openButton;
        private TablessControl tablessControl1;
        private System.Windows.Forms.TabPage nefsTabPage;
        private System.Windows.Forms.TabPage gameDatTabPage;
        private System.Windows.Forms.Button gameExeFileButton;
        private System.Windows.Forms.TextBox gameExeFileTextBox;
        private System.Windows.Forms.Label gameExeFileLabel;
        private System.Windows.Forms.Label gameDatFilesLabel;
        private System.Windows.Forms.ListBox gameDatFilesListBox;
        private System.Windows.Forms.Button gameDatRefreshButton;
        private System.Windows.Forms.Button gameDatDirButton;
        private System.Windows.Forms.TextBox gameDatDirTextBox;
        private System.Windows.Forms.Label gameDatDirLabel;
        private System.Windows.Forms.ListBox modeListBox;
        private System.Windows.Forms.TabPage recentTabPage;
        private System.Windows.Forms.ListBox recentListBox;
        private System.Windows.Forms.GroupBox advancedGroupBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox headerPart6OffsetTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label headerPart6OffsetLabel;
        private System.Windows.Forms.TextBox dataFileTextBox;
        private System.Windows.Forms.TextBox headerOffsetTextBox;
        private System.Windows.Forms.Button dataFileBrowseButton;
        private System.Windows.Forms.CheckBox advancedCheckBox;
    }
}
