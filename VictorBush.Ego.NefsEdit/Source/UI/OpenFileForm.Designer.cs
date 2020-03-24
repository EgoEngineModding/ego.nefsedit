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
            this.typeComboBox = new System.Windows.Forms.ComboBox();
            this.typeLabel = new System.Windows.Forms.Label();
            this.tablessControl1 = new VictorBush.Ego.NefsEdit.UI.TablessControl();
            this.nefsTabPage = new System.Windows.Forms.TabPage();
            this.fileLabel = new System.Windows.Forms.Label();
            this.nefsFileButton = new System.Windows.Forms.Button();
            this.nefsFileTextBox = new System.Windows.Forms.TextBox();
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
            this.customTabPage = new System.Windows.Forms.TabPage();
            this.customHeaderOffsetTextBox = new System.Windows.Forms.TextBox();
            this.customHeaderFileButton = new System.Windows.Forms.Button();
            this.customHeaderFileTextBox = new System.Windows.Forms.TextBox();
            this.customHeaderFileLabel = new System.Windows.Forms.Label();
            this.customDataFileButton = new System.Windows.Forms.Button();
            this.customHeaderOffsetLabel = new System.Windows.Forms.Label();
            this.customDataFileTextBox = new System.Windows.Forms.TextBox();
            this.customDataFileLabel = new System.Windows.Forms.Label();
            this.tablessControl1.SuspendLayout();
            this.nefsTabPage.SuspendLayout();
            this.gameDatTabPage.SuspendLayout();
            this.customTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(215, 360);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 24;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // openButton
            // 
            this.openButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.openButton.Location = new System.Drawing.Point(134, 360);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(75, 23);
            this.openButton.TabIndex = 23;
            this.openButton.Text = "Open";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.OpenButton_Click);
            // 
            // typeComboBox
            // 
            this.typeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.typeComboBox.FormattingEnabled = true;
            this.typeComboBox.Location = new System.Drawing.Point(49, 6);
            this.typeComboBox.Name = "typeComboBox";
            this.typeComboBox.Size = new System.Drawing.Size(150, 21);
            this.typeComboBox.TabIndex = 6;
            this.typeComboBox.SelectedIndexChanged += new System.EventHandler(this.TypeComboBox_SelectedIndexChanged);
            // 
            // typeLabel
            // 
            this.typeLabel.AutoSize = true;
            this.typeLabel.Location = new System.Drawing.Point(12, 9);
            this.typeLabel.Name = "typeLabel";
            this.typeLabel.Size = new System.Drawing.Size(31, 13);
            this.typeLabel.TabIndex = 7;
            this.typeLabel.Text = "Type";
            // 
            // tablessControl1
            // 
            this.tablessControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tablessControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tablessControl1.Controls.Add(this.nefsTabPage);
            this.tablessControl1.Controls.Add(this.gameDatTabPage);
            this.tablessControl1.Controls.Add(this.customTabPage);
            this.tablessControl1.Location = new System.Drawing.Point(1, 33);
            this.tablessControl1.Name = "tablessControl1";
            this.tablessControl1.SelectedIndex = 0;
            this.tablessControl1.Size = new System.Drawing.Size(301, 321);
            this.tablessControl1.TabIndex = 18;
            // 
            // nefsTabPage
            // 
            this.nefsTabPage.Controls.Add(this.fileLabel);
            this.nefsTabPage.Controls.Add(this.nefsFileButton);
            this.nefsTabPage.Controls.Add(this.nefsFileTextBox);
            this.nefsTabPage.Location = new System.Drawing.Point(4, 25);
            this.nefsTabPage.Name = "nefsTabPage";
            this.nefsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.nefsTabPage.Size = new System.Drawing.Size(293, 292);
            this.nefsTabPage.TabIndex = 0;
            this.nefsTabPage.Text = "NeFS";
            this.nefsTabPage.UseVisualStyleBackColor = true;
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
            this.nefsFileButton.Location = new System.Drawing.Point(259, 22);
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
            this.nefsFileTextBox.Size = new System.Drawing.Size(244, 20);
            this.nefsFileTextBox.TabIndex = 0;
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
            this.gameDatTabPage.Size = new System.Drawing.Size(293, 292);
            this.gameDatTabPage.TabIndex = 1;
            this.gameDatTabPage.Text = "game*.dat";
            this.gameDatTabPage.UseVisualStyleBackColor = true;
            // 
            // gameDatDirButton
            // 
            this.gameDatDirButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gameDatDirButton.Location = new System.Drawing.Point(261, 66);
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
            this.gameDatDirTextBox.Size = new System.Drawing.Size(244, 20);
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
            this.gameDatRefreshButton.Location = new System.Drawing.Point(10, 263);
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
            this.gameDatFilesListBox.Size = new System.Drawing.Size(275, 147);
            this.gameDatFilesListBox.TabIndex = 14;
            // 
            // gameExeFileButton
            // 
            this.gameExeFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gameExeFileButton.Location = new System.Drawing.Point(260, 22);
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
            this.gameExeFileTextBox.Size = new System.Drawing.Size(244, 20);
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
            // customTabPage
            // 
            this.customTabPage.Controls.Add(this.customHeaderOffsetTextBox);
            this.customTabPage.Controls.Add(this.customHeaderFileButton);
            this.customTabPage.Controls.Add(this.customHeaderFileTextBox);
            this.customTabPage.Controls.Add(this.customHeaderFileLabel);
            this.customTabPage.Controls.Add(this.customDataFileButton);
            this.customTabPage.Controls.Add(this.customHeaderOffsetLabel);
            this.customTabPage.Controls.Add(this.customDataFileTextBox);
            this.customTabPage.Controls.Add(this.customDataFileLabel);
            this.customTabPage.Location = new System.Drawing.Point(4, 25);
            this.customTabPage.Name = "customTabPage";
            this.customTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.customTabPage.Size = new System.Drawing.Size(293, 292);
            this.customTabPage.TabIndex = 2;
            this.customTabPage.Text = "Custom";
            this.customTabPage.UseVisualStyleBackColor = true;
            // 
            // customHeaderOffsetTextBox
            // 
            this.customHeaderOffsetTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.customHeaderOffsetTextBox.Location = new System.Drawing.Point(9, 68);
            this.customHeaderOffsetTextBox.Name = "customHeaderOffsetTextBox";
            this.customHeaderOffsetTextBox.Size = new System.Drawing.Size(71, 20);
            this.customHeaderOffsetTextBox.TabIndex = 20;
            // 
            // customHeaderFileButton
            // 
            this.customHeaderFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.customHeaderFileButton.Location = new System.Drawing.Point(229, 22);
            this.customHeaderFileButton.Name = "customHeaderFileButton";
            this.customHeaderFileButton.Size = new System.Drawing.Size(26, 23);
            this.customHeaderFileButton.TabIndex = 19;
            this.customHeaderFileButton.Text = "...";
            this.customHeaderFileButton.UseVisualStyleBackColor = true;
            this.customHeaderFileButton.Click += new System.EventHandler(this.CustomHeaderFileButton_Click);
            // 
            // customHeaderFileTextBox
            // 
            this.customHeaderFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.customHeaderFileTextBox.Location = new System.Drawing.Point(9, 24);
            this.customHeaderFileTextBox.Name = "customHeaderFileTextBox";
            this.customHeaderFileTextBox.Size = new System.Drawing.Size(214, 20);
            this.customHeaderFileTextBox.TabIndex = 18;
            // 
            // customHeaderFileLabel
            // 
            this.customHeaderFileLabel.AutoSize = true;
            this.customHeaderFileLabel.Location = new System.Drawing.Point(6, 8);
            this.customHeaderFileLabel.Name = "customHeaderFileLabel";
            this.customHeaderFileLabel.Size = new System.Drawing.Size(61, 13);
            this.customHeaderFileLabel.TabIndex = 17;
            this.customHeaderFileLabel.Text = "Header File";
            // 
            // customDataFileButton
            // 
            this.customDataFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.customDataFileButton.Location = new System.Drawing.Point(229, 110);
            this.customDataFileButton.Name = "customDataFileButton";
            this.customDataFileButton.Size = new System.Drawing.Size(26, 23);
            this.customDataFileButton.TabIndex = 22;
            this.customDataFileButton.Text = "...";
            this.customDataFileButton.UseVisualStyleBackColor = true;
            this.customDataFileButton.Click += new System.EventHandler(this.CustomDataFileButton_Click);
            // 
            // customHeaderOffsetLabel
            // 
            this.customHeaderOffsetLabel.AutoSize = true;
            this.customHeaderOffsetLabel.Location = new System.Drawing.Point(6, 52);
            this.customHeaderOffsetLabel.Name = "customHeaderOffsetLabel";
            this.customHeaderOffsetLabel.Size = new System.Drawing.Size(73, 13);
            this.customHeaderOffsetLabel.TabIndex = 16;
            this.customHeaderOffsetLabel.Text = "Header Offset";
            // 
            // customDataFileTextBox
            // 
            this.customDataFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.customDataFileTextBox.Location = new System.Drawing.Point(9, 112);
            this.customDataFileTextBox.Name = "customDataFileTextBox";
            this.customDataFileTextBox.Size = new System.Drawing.Size(214, 20);
            this.customDataFileTextBox.TabIndex = 21;
            // 
            // customDataFileLabel
            // 
            this.customDataFileLabel.AutoSize = true;
            this.customDataFileLabel.Location = new System.Drawing.Point(6, 96);
            this.customDataFileLabel.Name = "customDataFileLabel";
            this.customDataFileLabel.Size = new System.Drawing.Size(49, 13);
            this.customDataFileLabel.TabIndex = 12;
            this.customDataFileLabel.Text = "Data File";
            // 
            // OpenFileForm
            // 
            this.AcceptButton = this.openButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(302, 395);
            this.Controls.Add(this.tablessControl1);
            this.Controls.Add(this.typeLabel);
            this.Controls.Add(this.typeComboBox);
            this.Controls.Add(this.openButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OpenFileForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Open";
            this.Load += new System.EventHandler(this.OpenFileForm_Load);
            this.tablessControl1.ResumeLayout(false);
            this.nefsTabPage.ResumeLayout(false);
            this.nefsTabPage.PerformLayout();
            this.gameDatTabPage.ResumeLayout(false);
            this.gameDatTabPage.PerformLayout();
            this.customTabPage.ResumeLayout(false);
            this.customTabPage.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox nefsFileTextBox;
        private System.Windows.Forms.Label fileLabel;
        private System.Windows.Forms.Button nefsFileButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button openButton;
        private System.Windows.Forms.ComboBox typeComboBox;
        private System.Windows.Forms.Label typeLabel;
        private System.Windows.Forms.Button customDataFileButton;
        private System.Windows.Forms.TextBox customDataFileTextBox;
        private System.Windows.Forms.Label customDataFileLabel;
        private System.Windows.Forms.Label customHeaderOffsetLabel;
        private TablessControl tablessControl1;
        private System.Windows.Forms.TabPage nefsTabPage;
        private System.Windows.Forms.TabPage gameDatTabPage;
        private System.Windows.Forms.TabPage customTabPage;
        private System.Windows.Forms.Button customHeaderFileButton;
        private System.Windows.Forms.TextBox customHeaderFileTextBox;
        private System.Windows.Forms.Label customHeaderFileLabel;
        private System.Windows.Forms.Button gameExeFileButton;
        private System.Windows.Forms.TextBox gameExeFileTextBox;
        private System.Windows.Forms.Label gameExeFileLabel;
        private System.Windows.Forms.TextBox customHeaderOffsetTextBox;
        private System.Windows.Forms.Label gameDatFilesLabel;
        private System.Windows.Forms.ListBox gameDatFilesListBox;
        private System.Windows.Forms.Button gameDatRefreshButton;
        private System.Windows.Forms.Button gameDatDirButton;
        private System.Windows.Forms.TextBox gameDatDirTextBox;
        private System.Windows.Forms.Label gameDatDirLabel;
    }
}
