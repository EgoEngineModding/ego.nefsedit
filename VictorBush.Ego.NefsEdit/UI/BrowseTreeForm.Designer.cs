namespace VictorBush.Ego.NefsEdit.UI
{
    partial class BrowseTreeForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BrowseTreeForm));
			this.filesListView = new ListView();
			this.directoryTreeView = new TreeView();
			this.splitContainer1 = new SplitContainer();
			this.toolStrip = new ToolStrip();
			this.backButton = new ToolStripButton();
			this.forwardButton = new ToolStripButton();
			this.upButton = new ToolStripButton();
			this.pathLabel = new ToolStripLabel();
			((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.toolStrip.SuspendLayout();
			SuspendLayout();
			// 
			// filesListView
			// 
			this.filesListView.Dock = DockStyle.Fill;
			this.filesListView.FullRowSelect = true;
			this.filesListView.GridLines = true;
			this.filesListView.Location = new Point(0, 25);
			this.filesListView.Margin = new Padding(2);
			this.filesListView.Name = "filesListView";
			this.filesListView.Size = new Size(776, 805);
			this.filesListView.TabIndex = 3;
			this.filesListView.UseCompatibleStateImageBehavior = false;
			this.filesListView.View = View.Details;
			this.filesListView.SelectedIndexChanged += FilesListView_SelectedIndexChanged;
			this.filesListView.DoubleClick += FilesListView_DoubleClick;
			this.filesListView.MouseUp += FilesListView_MouseUp;
			// 
			// directoryTreeView
			// 
			this.directoryTreeView.Dock = DockStyle.Fill;
			this.directoryTreeView.Location = new Point(0, 0);
			this.directoryTreeView.Margin = new Padding(2);
			this.directoryTreeView.Name = "directoryTreeView";
			this.directoryTreeView.Size = new Size(263, 830);
			this.directoryTreeView.TabIndex = 2;
			this.directoryTreeView.AfterSelect += DirectoryTreeView_AfterSelect;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = DockStyle.Fill;
			this.splitContainer1.Location = new Point(0, 0);
			this.splitContainer1.Margin = new Padding(2);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.directoryTreeView);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.filesListView);
			this.splitContainer1.Panel2.Controls.Add(this.toolStrip);
			this.splitContainer1.Size = new Size(1043, 830);
			this.splitContainer1.SplitterDistance = 263;
			this.splitContainer1.TabIndex = 4;
			// 
			// toolStrip
			// 
			this.toolStrip.Items.AddRange(new ToolStripItem[] { this.backButton, this.forwardButton, this.upButton, this.pathLabel });
			this.toolStrip.Location = new Point(0, 0);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = new Size(776, 25);
			this.toolStrip.TabIndex = 5;
			this.toolStrip.Text = "toolStrip1";
			// 
			// backButton
			// 
			this.backButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.backButton.Image = Properties.Resources.arrow_back_16xLG;
			this.backButton.ImageTransparentColor = Color.Magenta;
			this.backButton.Name = "backButton";
			this.backButton.Size = new Size(23, 22);
			this.backButton.Visible = false;
			// 
			// forwardButton
			// 
			this.forwardButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.forwardButton.Image = Properties.Resources.arrow_Forward_16xLG;
			this.forwardButton.ImageTransparentColor = Color.Magenta;
			this.forwardButton.Name = "forwardButton";
			this.forwardButton.Size = new Size(23, 22);
			this.forwardButton.Text = "toolStripButton1";
			this.forwardButton.Visible = false;
			// 
			// upButton
			// 
			this.upButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
			this.upButton.Image = Properties.Resources.arrow_Up_16xLG;
			this.upButton.ImageTransparentColor = Color.Magenta;
			this.upButton.Name = "upButton";
			this.upButton.Size = new Size(23, 22);
			this.upButton.Text = "toolStripButton1";
			this.upButton.Click += UpButton_Click;
			// 
			// pathLabel
			// 
			this.pathLabel.Name = "pathLabel";
			this.pathLabel.Size = new Size(12, 22);
			this.pathLabel.Text = "\\";
			// 
			// BrowseTreeForm
			// 
			AutoScaleDimensions = new SizeF(7F, 16F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1043, 830);
			Controls.Add(this.splitContainer1);
			Icon = (Icon)resources.GetObject("$this.Icon");
			Margin = new Padding(2);
			Name = "BrowseTreeForm";
			Text = "Tree View";
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView filesListView;
        private System.Windows.Forms.TreeView directoryTreeView;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton backButton;
        private System.Windows.Forms.ToolStripButton forwardButton;
        private System.Windows.Forms.ToolStripButton upButton;
        private System.Windows.Forms.ToolStripLabel pathLabel;
    }
}