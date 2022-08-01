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
            this.fileLabel = new System.Windows.Forms.Label();
            this.nefsFileButton = new System.Windows.Forms.Button();
            this.nefsFileTextBox = new System.Windows.Forms.TextBox();
            this.nefsInjectTabPage = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.nefsInjectHeaderFileButton = new System.Windows.Forms.Button();
            this.nefsInjectFileTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.nefsInjectDataFileButton = new System.Windows.Forms.Button();
            this.nefsInjectDataFileTextBox = new System.Windows.Forms.TextBox();
            this.headlessTabPage = new System.Windows.Forms.TabPage();
            this.gameDatRefreshButton = new System.Windows.Forms.Button();
            this.gameDatFilesLabel = new System.Windows.Forms.Label();
            this.gameDatFilesListBox = new System.Windows.Forms.ListBox();
            this.gameExeFileButton = new System.Windows.Forms.Button();
            this.headlessGameExeFileTextBox = new System.Windows.Forms.TextBox();
            this.gameExeFileLabel = new System.Windows.Forms.Label();
            this.headlessCustomTabPage = new System.Windows.Forms.TabPage();
            this.label6 = new System.Windows.Forms.Label();
            this.splitSecondarySizeTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.splitSecondaryOffsetTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.splitPrimarySizeTextBox = new System.Windows.Forms.TextBox();
            this.primaryOffsetLabel = new System.Windows.Forms.Label();
            this.splitPrimaryOffsetTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.splitHeaderFileBrowseButton = new System.Windows.Forms.Button();
            this.splitHeaderFileTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.splitDataFileBrowseButton = new System.Windows.Forms.Button();
            this.splitDataFileTextBox = new System.Windows.Forms.TextBox();
            this.recentTabPage = new System.Windows.Forms.TabPage();
            this.recentListBox = new System.Windows.Forms.ListBox();
            this.modeListBox = new System.Windows.Forms.ListBox();
            this.tablessControl1.SuspendLayout();
            this.nefsTabPage.SuspendLayout();
            this.nefsInjectTabPage.SuspendLayout();
            this.headlessTabPage.SuspendLayout();
            this.headlessCustomTabPage.SuspendLayout();
            this.recentTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(509, 412);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 35);
            this.cancelButton.TabIndex = 24;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // openButton
            // 
            this.openButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.openButton.Location = new System.Drawing.Point(401, 412);
            this.openButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(100, 35);
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
            this.tablessControl1.Controls.Add(this.nefsInjectTabPage);
            this.tablessControl1.Controls.Add(this.headlessTabPage);
            this.tablessControl1.Controls.Add(this.headlessCustomTabPage);
            this.tablessControl1.Controls.Add(this.recentTabPage);
            this.tablessControl1.Location = new System.Drawing.Point(187, 14);
            this.tablessControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tablessControl1.Name = "tablessControl1";
            this.tablessControl1.SelectedIndex = 0;
            this.tablessControl1.Size = new System.Drawing.Size(428, 388);
            this.tablessControl1.TabIndex = 18;
            // 
            // nefsTabPage
            // 
            this.nefsTabPage.Controls.Add(this.fileLabel);
            this.nefsTabPage.Controls.Add(this.nefsFileButton);
            this.nefsTabPage.Controls.Add(this.nefsFileTextBox);
            this.nefsTabPage.Location = new System.Drawing.Point(4, 32);
            this.nefsTabPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nefsTabPage.Name = "nefsTabPage";
            this.nefsTabPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nefsTabPage.Size = new System.Drawing.Size(420, 352);
            this.nefsTabPage.TabIndex = 0;
            this.nefsTabPage.Text = "NeFS";
            this.nefsTabPage.UseVisualStyleBackColor = true;
            // 
            // fileLabel
            // 
            this.fileLabel.AutoSize = true;
            this.fileLabel.Location = new System.Drawing.Point(8, 12);
            this.fileLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.fileLabel.Name = "fileLabel";
            this.fileLabel.Size = new System.Drawing.Size(32, 20);
            this.fileLabel.TabIndex = 0;
            this.fileLabel.Text = "File";
            // 
            // nefsFileButton
            // 
            this.nefsFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nefsFileButton.Location = new System.Drawing.Point(371, 34);
            this.nefsFileButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nefsFileButton.Name = "nefsFileButton";
            this.nefsFileButton.Size = new System.Drawing.Size(35, 35);
            this.nefsFileButton.TabIndex = 1;
            this.nefsFileButton.Text = "...";
            this.nefsFileButton.UseVisualStyleBackColor = true;
            this.nefsFileButton.Click += new System.EventHandler(this.NefsFileButton_Click);
            // 
            // nefsFileTextBox
            // 
            this.nefsFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nefsFileTextBox.Location = new System.Drawing.Point(12, 38);
            this.nefsFileTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nefsFileTextBox.Name = "nefsFileTextBox";
            this.nefsFileTextBox.Size = new System.Drawing.Size(349, 27);
            this.nefsFileTextBox.TabIndex = 0;
            // 
            // nefsInjectTabPage
            // 
            this.nefsInjectTabPage.Controls.Add(this.label1);
            this.nefsInjectTabPage.Controls.Add(this.nefsInjectHeaderFileButton);
            this.nefsInjectTabPage.Controls.Add(this.nefsInjectFileTextBox);
            this.nefsInjectTabPage.Controls.Add(this.label2);
            this.nefsInjectTabPage.Controls.Add(this.nefsInjectDataFileButton);
            this.nefsInjectTabPage.Controls.Add(this.nefsInjectDataFileTextBox);
            this.nefsInjectTabPage.Location = new System.Drawing.Point(4, 32);
            this.nefsInjectTabPage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.nefsInjectTabPage.Name = "nefsInjectTabPage";
            this.nefsInjectTabPage.Size = new System.Drawing.Size(420, 352);
            this.nefsInjectTabPage.TabIndex = 5;
            this.nefsInjectTabPage.Text = "NefsInject";
            this.nefsInjectTabPage.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 82);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 20);
            this.label1.TabIndex = 11;
            this.label1.Text = "NefsInject file";
            // 
            // nefsInjectHeaderFileButton
            // 
            this.nefsInjectHeaderFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nefsInjectHeaderFileButton.Location = new System.Drawing.Point(372, 104);
            this.nefsInjectHeaderFileButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nefsInjectHeaderFileButton.Name = "nefsInjectHeaderFileButton";
            this.nefsInjectHeaderFileButton.Size = new System.Drawing.Size(35, 35);
            this.nefsInjectHeaderFileButton.TabIndex = 13;
            this.nefsInjectHeaderFileButton.Text = "...";
            this.nefsInjectHeaderFileButton.UseVisualStyleBackColor = true;
            // 
            // nefsInjectFileTextBox
            // 
            this.nefsInjectFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nefsInjectFileTextBox.Location = new System.Drawing.Point(13, 108);
            this.nefsInjectFileTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nefsInjectFileTextBox.Name = "nefsInjectFileTextBox";
            this.nefsInjectFileTextBox.Size = new System.Drawing.Size(349, 27);
            this.nefsInjectFileTextBox.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 12);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(269, 20);
            this.label2.TabIndex = 8;
            this.label2.Text = "Data File (i.e., game.dat, game.bin, etc.)";
            // 
            // nefsInjectDataFileButton
            // 
            this.nefsInjectDataFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nefsInjectDataFileButton.Location = new System.Drawing.Point(371, 34);
            this.nefsInjectDataFileButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nefsInjectDataFileButton.Name = "nefsInjectDataFileButton";
            this.nefsInjectDataFileButton.Size = new System.Drawing.Size(35, 35);
            this.nefsInjectDataFileButton.TabIndex = 10;
            this.nefsInjectDataFileButton.Text = "...";
            this.nefsInjectDataFileButton.UseVisualStyleBackColor = true;
            // 
            // nefsInjectDataFileTextBox
            // 
            this.nefsInjectDataFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nefsInjectDataFileTextBox.Location = new System.Drawing.Point(12, 38);
            this.nefsInjectDataFileTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nefsInjectDataFileTextBox.Name = "nefsInjectDataFileTextBox";
            this.nefsInjectDataFileTextBox.Size = new System.Drawing.Size(349, 27);
            this.nefsInjectDataFileTextBox.TabIndex = 9;
            // 
            // headlessTabPage
            // 
            this.headlessTabPage.Controls.Add(this.gameDatRefreshButton);
            this.headlessTabPage.Controls.Add(this.gameDatFilesLabel);
            this.headlessTabPage.Controls.Add(this.gameDatFilesListBox);
            this.headlessTabPage.Controls.Add(this.gameExeFileButton);
            this.headlessTabPage.Controls.Add(this.headlessGameExeFileTextBox);
            this.headlessTabPage.Controls.Add(this.gameExeFileLabel);
            this.headlessTabPage.Location = new System.Drawing.Point(4, 32);
            this.headlessTabPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.headlessTabPage.Name = "headlessTabPage";
            this.headlessTabPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.headlessTabPage.Size = new System.Drawing.Size(420, 352);
            this.headlessTabPage.TabIndex = 1;
            this.headlessTabPage.Text = "Headless";
            this.headlessTabPage.UseVisualStyleBackColor = true;
            // 
            // gameDatRefreshButton
            // 
            this.gameDatRefreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.gameDatRefreshButton.Location = new System.Drawing.Point(13, 305);
            this.gameDatRefreshButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gameDatRefreshButton.Name = "gameDatRefreshButton";
            this.gameDatRefreshButton.Size = new System.Drawing.Size(100, 35);
            this.gameDatRefreshButton.TabIndex = 25;
            this.gameDatRefreshButton.Text = "Search";
            this.gameDatRefreshButton.UseVisualStyleBackColor = true;
            this.gameDatRefreshButton.Click += new System.EventHandler(this.GameDatRefreshButton_Click);
            // 
            // gameDatFilesLabel
            // 
            this.gameDatFilesLabel.AutoSize = true;
            this.gameDatFilesLabel.Location = new System.Drawing.Point(13, 80);
            this.gameDatFilesLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.gameDatFilesLabel.Name = "gameDatFilesLabel";
            this.gameDatFilesLabel.Size = new System.Drawing.Size(119, 20);
            this.gameDatFilesLabel.TabIndex = 15;
            this.gameDatFilesLabel.Text = "Injection Profiles";
            // 
            // gameDatFilesListBox
            // 
            this.gameDatFilesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gameDatFilesListBox.FormattingEnabled = true;
            this.gameDatFilesListBox.ItemHeight = 20;
            this.gameDatFilesListBox.Location = new System.Drawing.Point(13, 108);
            this.gameDatFilesListBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gameDatFilesListBox.Name = "gameDatFilesListBox";
            this.gameDatFilesListBox.Size = new System.Drawing.Size(391, 184);
            this.gameDatFilesListBox.TabIndex = 14;
            // 
            // gameExeFileButton
            // 
            this.gameExeFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gameExeFileButton.Location = new System.Drawing.Point(372, 34);
            this.gameExeFileButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gameExeFileButton.Name = "gameExeFileButton";
            this.gameExeFileButton.Size = new System.Drawing.Size(35, 35);
            this.gameExeFileButton.TabIndex = 13;
            this.gameExeFileButton.Text = "...";
            this.gameExeFileButton.UseVisualStyleBackColor = true;
            this.gameExeFileButton.Click += new System.EventHandler(this.GameExeFileButton_Click);
            // 
            // headlessGameExeFileTextBox
            // 
            this.headlessGameExeFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.headlessGameExeFileTextBox.Location = new System.Drawing.Point(13, 38);
            this.headlessGameExeFileTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.headlessGameExeFileTextBox.Name = "headlessGameExeFileTextBox";
            this.headlessGameExeFileTextBox.Size = new System.Drawing.Size(349, 27);
            this.headlessGameExeFileTextBox.TabIndex = 12;
            // 
            // gameExeFileLabel
            // 
            this.gameExeFileLabel.AutoSize = true;
            this.gameExeFileLabel.Location = new System.Drawing.Point(9, 12);
            this.gameExeFileLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.gameExeFileLabel.Name = "gameExeFileLabel";
            this.gameExeFileLabel.Size = new System.Drawing.Size(124, 20);
            this.gameExeFileLabel.TabIndex = 12;
            this.gameExeFileLabel.Text = "Game Executable";
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
            this.headlessCustomTabPage.Location = new System.Drawing.Point(4, 32);
            this.headlessCustomTabPage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.headlessCustomTabPage.Name = "headlessCustomTabPage";
            this.headlessCustomTabPage.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.headlessCustomTabPage.Size = new System.Drawing.Size(420, 352);
            this.headlessCustomTabPage.TabIndex = 4;
            this.headlessCustomTabPage.Text = "Headless (Custom)";
            this.headlessCustomTabPage.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(187, 224);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(109, 20);
            this.label6.TabIndex = 32;
            this.label6.Text = "Secondary Size";
            // 
            // splitSecondarySizeTextBox
            // 
            this.splitSecondarySizeTextBox.Location = new System.Drawing.Point(191, 250);
            this.splitSecondarySizeTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitSecondarySizeTextBox.Name = "splitSecondarySizeTextBox";
            this.splitSecondarySizeTextBox.Size = new System.Drawing.Size(113, 27);
            this.splitSecondarySizeTextBox.TabIndex = 33;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 224);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(162, 20);
            this.label7.TabIndex = 30;
            this.label7.Text = "Secondary Offset (Hex)";
            // 
            // splitSecondaryOffsetTextBox
            // 
            this.splitSecondaryOffsetTextBox.Location = new System.Drawing.Point(15, 250);
            this.splitSecondaryOffsetTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitSecondaryOffsetTextBox.Name = "splitSecondaryOffsetTextBox";
            this.splitSecondaryOffsetTextBox.Size = new System.Drawing.Size(113, 27);
            this.splitSecondaryOffsetTextBox.TabIndex = 31;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(186, 152);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(90, 20);
            this.label5.TabIndex = 28;
            this.label5.Text = "Primary Size";
            // 
            // splitPrimarySizeTextBox
            // 
            this.splitPrimarySizeTextBox.Location = new System.Drawing.Point(190, 179);
            this.splitPrimarySizeTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitPrimarySizeTextBox.Name = "splitPrimarySizeTextBox";
            this.splitPrimarySizeTextBox.Size = new System.Drawing.Size(113, 27);
            this.splitPrimarySizeTextBox.TabIndex = 29;
            // 
            // primaryOffsetLabel
            // 
            this.primaryOffsetLabel.AutoSize = true;
            this.primaryOffsetLabel.Location = new System.Drawing.Point(10, 152);
            this.primaryOffsetLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.primaryOffsetLabel.Name = "primaryOffsetLabel";
            this.primaryOffsetLabel.Size = new System.Drawing.Size(143, 20);
            this.primaryOffsetLabel.TabIndex = 26;
            this.primaryOffsetLabel.Text = "Primary Offset (Hex)";
            // 
            // splitPrimaryOffsetTextBox
            // 
            this.splitPrimaryOffsetTextBox.Location = new System.Drawing.Point(14, 179);
            this.splitPrimaryOffsetTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitPrimaryOffsetTextBox.Name = "splitPrimaryOffsetTextBox";
            this.splitPrimaryOffsetTextBox.Size = new System.Drawing.Size(113, 27);
            this.splitPrimaryOffsetTextBox.TabIndex = 27;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 82);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(238, 20);
            this.label4.TabIndex = 5;
            this.label4.Text = "Header File (i.e., game executable)";
            // 
            // splitHeaderFileBrowseButton
            // 
            this.splitHeaderFileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.splitHeaderFileBrowseButton.Location = new System.Drawing.Point(372, 104);
            this.splitHeaderFileBrowseButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitHeaderFileBrowseButton.Name = "splitHeaderFileBrowseButton";
            this.splitHeaderFileBrowseButton.Size = new System.Drawing.Size(35, 35);
            this.splitHeaderFileBrowseButton.TabIndex = 7;
            this.splitHeaderFileBrowseButton.Text = "...";
            this.splitHeaderFileBrowseButton.UseVisualStyleBackColor = true;
            this.splitHeaderFileBrowseButton.Click += new System.EventHandler(this.splitHeaderFileBrowseButton_Click);
            // 
            // splitHeaderFileTextBox
            // 
            this.splitHeaderFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitHeaderFileTextBox.Location = new System.Drawing.Point(13, 108);
            this.splitHeaderFileTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitHeaderFileTextBox.Name = "splitHeaderFileTextBox";
            this.splitHeaderFileTextBox.Size = new System.Drawing.Size(349, 27);
            this.splitHeaderFileTextBox.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 12);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(269, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "Data File (i.e., game.dat, game.bin, etc.)";
            // 
            // splitDataFileBrowseButton
            // 
            this.splitDataFileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.splitDataFileBrowseButton.Location = new System.Drawing.Point(371, 34);
            this.splitDataFileBrowseButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitDataFileBrowseButton.Name = "splitDataFileBrowseButton";
            this.splitDataFileBrowseButton.Size = new System.Drawing.Size(35, 35);
            this.splitDataFileBrowseButton.TabIndex = 4;
            this.splitDataFileBrowseButton.Text = "...";
            this.splitDataFileBrowseButton.UseVisualStyleBackColor = true;
            this.splitDataFileBrowseButton.Click += new System.EventHandler(this.splitDataFileBrowseButton_Click);
            // 
            // splitDataFileTextBox
            // 
            this.splitDataFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitDataFileTextBox.Location = new System.Drawing.Point(12, 38);
            this.splitDataFileTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitDataFileTextBox.Name = "splitDataFileTextBox";
            this.splitDataFileTextBox.Size = new System.Drawing.Size(349, 27);
            this.splitDataFileTextBox.TabIndex = 3;
            // 
            // recentTabPage
            // 
            this.recentTabPage.Controls.Add(this.recentListBox);
            this.recentTabPage.Location = new System.Drawing.Point(4, 32);
            this.recentTabPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.recentTabPage.Name = "recentTabPage";
            this.recentTabPage.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.recentTabPage.Size = new System.Drawing.Size(420, 352);
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
            this.recentListBox.ItemHeight = 20;
            this.recentListBox.Location = new System.Drawing.Point(5, 6);
            this.recentListBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.recentListBox.Name = "recentListBox";
            this.recentListBox.Size = new System.Drawing.Size(411, 324);
            this.recentListBox.TabIndex = 26;
            // 
            // modeListBox
            // 
            this.modeListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.modeListBox.FormattingEnabled = true;
            this.modeListBox.ItemHeight = 20;
            this.modeListBox.Location = new System.Drawing.Point(11, 12);
            this.modeListBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.modeListBox.Name = "modeListBox";
            this.modeListBox.Size = new System.Drawing.Size(171, 384);
            this.modeListBox.TabIndex = 25;
            this.modeListBox.SelectedIndexChanged += new System.EventHandler(this.ModeListBox_SelectedIndexChanged);
            // 
            // OpenFileForm
            // 
            this.AcceptButton = this.openButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(625, 466);
            this.Controls.Add(this.modeListBox);
            this.Controls.Add(this.tablessControl1);
            this.Controls.Add(this.openButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
            this.nefsInjectTabPage.ResumeLayout(false);
            this.nefsInjectTabPage.PerformLayout();
            this.headlessTabPage.ResumeLayout(false);
            this.headlessTabPage.PerformLayout();
            this.headlessCustomTabPage.ResumeLayout(false);
            this.headlessCustomTabPage.PerformLayout();
            this.recentTabPage.ResumeLayout(false);
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
        private System.Windows.Forms.TabPage headlessTabPage;
        private System.Windows.Forms.Button gameExeFileButton;
        private System.Windows.Forms.TextBox headlessGameExeFileTextBox;
        private System.Windows.Forms.Label gameExeFileLabel;
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
        private System.Windows.Forms.TabPage nefsInjectTabPage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button nefsInjectHeaderFileButton;
        private System.Windows.Forms.TextBox nefsInjectFileTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button nefsInjectDataFileButton;
        private System.Windows.Forms.TextBox nefsInjectDataFileTextBox;
    }
}
