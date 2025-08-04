namespace VictorBush.Ego.NefsEdit.UI
{
    partial class EditorForm
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditorForm));
			this.mainMenuStrip = new MenuStrip();
			this.fileMainMenuItem = new ToolStripMenuItem();
			this.openMainMenuItem = new ToolStripMenuItem();
			this.toolStripSeparator5 = new ToolStripSeparator();
			this.closeMainMenuItem = new ToolStripMenuItem();
			this.toolStripSeparator6 = new ToolStripSeparator();
			this.saveMainMenuItem = new ToolStripMenuItem();
			this.saveAsMainMenuItem = new ToolStripMenuItem();
			this.toolStripSeparator2 = new ToolStripSeparator();
			this.exitMainMenuItem = new ToolStripMenuItem();
			this.editToolStripMenuItem = new ToolStripMenuItem();
			this.undoMainMenuItem = new ToolStripMenuItem();
			this.redoMainMenuItem = new ToolStripMenuItem();
			this.itemMainMenuItem = new ToolStripMenuItem();
			this.quickExtractMainMenuItem = new ToolStripMenuItem();
			this.toolStripSeparator4 = new ToolStripSeparator();
			this.extractToMainMenuItem = new ToolStripMenuItem();
			this.extractRawToToolStripMenuItem = new ToolStripMenuItem();
			this.toolStripSeparator3 = new ToolStripSeparator();
			this.replaceMainMenuItem = new ToolStripMenuItem();
			this.viewMainMenuItem = new ToolStripMenuItem();
			this.treeViewMainMenuItem = new ToolStripMenuItem();
			this.debugViewMainMenuItem = new ToolStripMenuItem();
			this.archiveDebugMainMenuItem = new ToolStripMenuItem();
			this.itemDebugViewMainMenuItem = new ToolStripMenuItem();
			this.toolStripSeparator1 = new ToolStripSeparator();
			this.itemDetailsMainMenuItem = new ToolStripMenuItem();
			this.consoleMainMenuItem = new ToolStripMenuItem();
			this.toolsMainMenuItem = new ToolStripMenuItem();
			this.optionsMainMenuItem = new ToolStripMenuItem();
			this.helpMainMenuItem = new ToolStripMenuItem();
			this.aboutMainMenuItem = new ToolStripMenuItem();
			this.vS2005Theme1 = new WeifenLuo.WinFormsUI.Docking.VS2015BlueTheme();
			this.browserDockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
			this.itemContextMenuStrip = new ContextMenuStrip(this.components);
			this.quickExtractContextMenuItem = new ToolStripMenuItem();
			this.extractToContextMenuItem = new ToolStripMenuItem();
			this.ReplaceContextMenuItem = new ToolStripMenuItem();
			this.mainMenuStrip.SuspendLayout();
			this.itemContextMenuStrip.SuspendLayout();
			SuspendLayout();
			// 
			// mainMenuStrip
			// 
			this.mainMenuStrip.ImageScalingSize = new Size(24, 24);
			this.mainMenuStrip.Items.AddRange(new ToolStripItem[] { this.fileMainMenuItem, this.editToolStripMenuItem, this.itemMainMenuItem, this.viewMainMenuItem, this.toolsMainMenuItem, this.helpMainMenuItem });
			this.mainMenuStrip.Location = new Point(0, 0);
			this.mainMenuStrip.Name = "mainMenuStrip";
			this.mainMenuStrip.Padding = new Padding(4, 2, 0, 2);
			this.mainMenuStrip.Size = new Size(1096, 24);
			this.mainMenuStrip.TabIndex = 8;
			this.mainMenuStrip.Text = "menuStrip1";
			// 
			// fileMainMenuItem
			// 
			this.fileMainMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.openMainMenuItem, this.toolStripSeparator5, this.closeMainMenuItem, this.toolStripSeparator6, this.saveMainMenuItem, this.saveAsMainMenuItem, this.toolStripSeparator2, this.exitMainMenuItem });
			this.fileMainMenuItem.Name = "fileMainMenuItem";
			this.fileMainMenuItem.Size = new Size(38, 20);
			this.fileMainMenuItem.Text = "File";
			// 
			// openMainMenuItem
			// 
			this.openMainMenuItem.Name = "openMainMenuItem";
			this.openMainMenuItem.ShortcutKeys = Keys.Control | Keys.O;
			this.openMainMenuItem.Size = new Size(161, 22);
			this.openMainMenuItem.Text = "Open...";
			this.openMainMenuItem.Click += OpenToolStripMenuItem_Click;
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new Size(158, 6);
			// 
			// closeMainMenuItem
			// 
			this.closeMainMenuItem.Name = "closeMainMenuItem";
			this.closeMainMenuItem.Size = new Size(161, 22);
			this.closeMainMenuItem.Text = "Close";
			this.closeMainMenuItem.Click += CloseMainMenuItem_Click;
			// 
			// toolStripSeparator6
			// 
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.toolStripSeparator6.Size = new Size(158, 6);
			// 
			// saveMainMenuItem
			// 
			this.saveMainMenuItem.Name = "saveMainMenuItem";
			this.saveMainMenuItem.ShortcutKeys = Keys.Control | Keys.S;
			this.saveMainMenuItem.Size = new Size(161, 22);
			this.saveMainMenuItem.Text = "Save";
			this.saveMainMenuItem.Click += SaveToolStripMenuItem_Click;
			// 
			// saveAsMainMenuItem
			// 
			this.saveAsMainMenuItem.Name = "saveAsMainMenuItem";
			this.saveAsMainMenuItem.Size = new Size(161, 22);
			this.saveAsMainMenuItem.Text = "Save As...";
			this.saveAsMainMenuItem.Click += SaveAsToolStripMenuItem_Click;
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new Size(158, 6);
			// 
			// exitMainMenuItem
			// 
			this.exitMainMenuItem.Name = "exitMainMenuItem";
			this.exitMainMenuItem.Size = new Size(161, 22);
			this.exitMainMenuItem.Text = "Exit";
			this.exitMainMenuItem.Click += ExitToolStripMenuItem_Click;
			// 
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.undoMainMenuItem, this.redoMainMenuItem });
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			this.editToolStripMenuItem.Size = new Size(42, 20);
			this.editToolStripMenuItem.Text = "Edit";
			// 
			// undoMainMenuItem
			// 
			this.undoMainMenuItem.Name = "undoMainMenuItem";
			this.undoMainMenuItem.ShortcutKeys = Keys.Control | Keys.Z;
			this.undoMainMenuItem.Size = new Size(149, 22);
			this.undoMainMenuItem.Text = "Undo";
			this.undoMainMenuItem.Click += UndoMainMenuItem_Click;
			// 
			// redoMainMenuItem
			// 
			this.redoMainMenuItem.Name = "redoMainMenuItem";
			this.redoMainMenuItem.ShortcutKeys = Keys.Control | Keys.Y;
			this.redoMainMenuItem.Size = new Size(149, 22);
			this.redoMainMenuItem.Text = "Redo";
			this.redoMainMenuItem.Click += RedoMainMenuItem_Click;
			// 
			// itemMainMenuItem
			// 
			this.itemMainMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.quickExtractMainMenuItem, this.toolStripSeparator4, this.extractToMainMenuItem, this.extractRawToToolStripMenuItem, this.toolStripSeparator3, this.replaceMainMenuItem });
			this.itemMainMenuItem.Name = "itemMainMenuItem";
			this.itemMainMenuItem.Size = new Size(46, 20);
			this.itemMainMenuItem.Text = "Item";
			// 
			// quickExtractMainMenuItem
			// 
			this.quickExtractMainMenuItem.Name = "quickExtractMainMenuItem";
			this.quickExtractMainMenuItem.Size = new Size(188, 22);
			this.quickExtractMainMenuItem.Text = "Quick Extract";
			this.quickExtractMainMenuItem.Click += QuickExtractToolStripMenuItem_Click;
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new Size(185, 6);
			// 
			// extractToMainMenuItem
			// 
			this.extractToMainMenuItem.Name = "extractToMainMenuItem";
			this.extractToMainMenuItem.ShortcutKeys = Keys.Control | Keys.E;
			this.extractToMainMenuItem.Size = new Size(188, 22);
			this.extractToMainMenuItem.Text = "Extract To...";
			this.extractToMainMenuItem.Click += ExtractToToolStripMenuItem_Click;
			// 
			// extractRawToToolStripMenuItem
			// 
			this.extractRawToToolStripMenuItem.Name = "extractRawToToolStripMenuItem";
			this.extractRawToToolStripMenuItem.Size = new Size(188, 22);
			this.extractRawToToolStripMenuItem.Text = "Extract Raw To...";
			this.extractRawToToolStripMenuItem.Click += extractRawToToolStripMenuItem_Click;
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new Size(185, 6);
			// 
			// replaceMainMenuItem
			// 
			this.replaceMainMenuItem.Name = "replaceMainMenuItem";
			this.replaceMainMenuItem.ShortcutKeys = Keys.Control | Keys.R;
			this.replaceMainMenuItem.Size = new Size(188, 22);
			this.replaceMainMenuItem.Text = "Replace...";
			this.replaceMainMenuItem.Click += ReplaceToolStripMenuItem_Click;
			// 
			// viewMainMenuItem
			// 
			this.viewMainMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.treeViewMainMenuItem, this.debugViewMainMenuItem, this.archiveDebugMainMenuItem, this.itemDebugViewMainMenuItem, this.toolStripSeparator1, this.itemDetailsMainMenuItem, this.consoleMainMenuItem });
			this.viewMainMenuItem.Name = "viewMainMenuItem";
			this.viewMainMenuItem.Size = new Size(45, 20);
			this.viewMainMenuItem.Text = "View";
			// 
			// treeViewMainMenuItem
			// 
			this.treeViewMainMenuItem.Name = "treeViewMainMenuItem";
			this.treeViewMainMenuItem.Size = new Size(187, 22);
			this.treeViewMainMenuItem.Text = "Tree View";
			this.treeViewMainMenuItem.Click += TreeViewToolStripMenuItem_Click;
			// 
			// debugViewMainMenuItem
			// 
			this.debugViewMainMenuItem.Name = "debugViewMainMenuItem";
			this.debugViewMainMenuItem.Size = new Size(187, 22);
			this.debugViewMainMenuItem.Text = "Debug View";
			this.debugViewMainMenuItem.Click += DebugViewToolStripMenuItem_Click;
			// 
			// archiveDebugMainMenuItem
			// 
			this.archiveDebugMainMenuItem.Name = "archiveDebugMainMenuItem";
			this.archiveDebugMainMenuItem.Size = new Size(187, 22);
			this.archiveDebugMainMenuItem.Text = "Archive Debug View";
			this.archiveDebugMainMenuItem.Click += ArchiveDebugMainMenuItem_Click;
			// 
			// itemDebugViewMainMenuItem
			// 
			this.itemDebugViewMainMenuItem.Name = "itemDebugViewMainMenuItem";
			this.itemDebugViewMainMenuItem.Size = new Size(187, 22);
			this.itemDebugViewMainMenuItem.Text = "Item Debug View";
			this.itemDebugViewMainMenuItem.Click += ItemDebugViewMainMenuItem_Click;
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new Size(184, 6);
			// 
			// itemDetailsMainMenuItem
			// 
			this.itemDetailsMainMenuItem.Name = "itemDetailsMainMenuItem";
			this.itemDetailsMainMenuItem.Size = new Size(187, 22);
			this.itemDetailsMainMenuItem.Text = "Item Details";
			this.itemDetailsMainMenuItem.Click += ItemDetailsToolStripMenuItem_Click;
			// 
			// consoleMainMenuItem
			// 
			this.consoleMainMenuItem.Name = "consoleMainMenuItem";
			this.consoleMainMenuItem.Size = new Size(187, 22);
			this.consoleMainMenuItem.Text = "Console";
			this.consoleMainMenuItem.Click += ConsoleToolStripMenuItem_Click;
			// 
			// toolsMainMenuItem
			// 
			this.toolsMainMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.optionsMainMenuItem });
			this.toolsMainMenuItem.Name = "toolsMainMenuItem";
			this.toolsMainMenuItem.Size = new Size(48, 20);
			this.toolsMainMenuItem.Text = "Tools";
			// 
			// optionsMainMenuItem
			// 
			this.optionsMainMenuItem.Name = "optionsMainMenuItem";
			this.optionsMainMenuItem.Size = new Size(122, 22);
			this.optionsMainMenuItem.Text = "Options";
			this.optionsMainMenuItem.Click += OptionsMainMenuItem_Click;
			// 
			// helpMainMenuItem
			// 
			this.helpMainMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.aboutMainMenuItem });
			this.helpMainMenuItem.Name = "helpMainMenuItem";
			this.helpMainMenuItem.Size = new Size(44, 20);
			this.helpMainMenuItem.Text = "Help";
			// 
			// aboutMainMenuItem
			// 
			this.aboutMainMenuItem.Name = "aboutMainMenuItem";
			this.aboutMainMenuItem.Size = new Size(111, 22);
			this.aboutMainMenuItem.Text = "About";
			this.aboutMainMenuItem.Click += AboutToolStripMenuItem_Click;
			// 
			// browserDockPanel
			// 
			this.browserDockPanel.Dock = DockStyle.Fill;
			this.browserDockPanel.Location = new Point(0, 24);
			this.browserDockPanel.Margin = new Padding(3, 2, 3, 2);
			this.browserDockPanel.Name = "browserDockPanel";
			this.browserDockPanel.Size = new Size(1096, 826);
			this.browserDockPanel.TabIndex = 9;
			// 
			// itemContextMenuStrip
			// 
			this.itemContextMenuStrip.ImageScalingSize = new Size(24, 24);
			this.itemContextMenuStrip.Items.AddRange(new ToolStripItem[] { this.quickExtractContextMenuItem, this.extractToContextMenuItem, this.ReplaceContextMenuItem });
			this.itemContextMenuStrip.Name = "itemContextMenuStrip";
			this.itemContextMenuStrip.Size = new Size(156, 70);
			this.itemContextMenuStrip.Text = "Item";
			// 
			// quickExtractContextMenuItem
			// 
			this.quickExtractContextMenuItem.Name = "quickExtractContextMenuItem";
			this.quickExtractContextMenuItem.Size = new Size(155, 22);
			this.quickExtractContextMenuItem.Text = "Quick Extract";
			this.quickExtractContextMenuItem.Click += QuickExtractContextMenuItem_Click;
			// 
			// extractToContextMenuItem
			// 
			this.extractToContextMenuItem.Name = "extractToContextMenuItem";
			this.extractToContextMenuItem.Size = new Size(155, 22);
			this.extractToContextMenuItem.Text = "Extract To...";
			this.extractToContextMenuItem.Click += ExtractToContextMenuItem_Click;
			// 
			// ReplaceContextMenuItem
			// 
			this.ReplaceContextMenuItem.Name = "ReplaceContextMenuItem";
			this.ReplaceContextMenuItem.Size = new Size(155, 22);
			this.ReplaceContextMenuItem.Text = "Replace...";
			this.ReplaceContextMenuItem.Click += ReplaceToolStripMenuItem_Click;
			// 
			// EditorForm
			// 
			AutoScaleDimensions = new SizeF(7F, 16F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1096, 850);
			Controls.Add(this.browserDockPanel);
			Controls.Add(this.mainMenuStrip);
			Icon = (Icon)resources.GetObject("$this.Icon");
			Margin = new Padding(3, 2, 3, 2);
			Name = "EditorForm";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "NeFS Edit";
			FormClosing += EditorForm_FormClosing;
			Load += EditorForm_Load;
			this.mainMenuStrip.ResumeLayout(false);
			this.mainMenuStrip.PerformLayout();
			this.itemContextMenuStrip.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();

		}

		#endregion
		private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileMainMenuItem;
        private WeifenLuo.WinFormsUI.Docking.VS2015BlueTheme vS2005Theme1;
        private WeifenLuo.WinFormsUI.Docking.DockPanel browserDockPanel;
        private System.Windows.Forms.ToolStripMenuItem itemMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quickExtractMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem archiveDebugMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem itemDetailsMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem treeViewMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugViewMainMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem helpMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem consoleMainMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitMainMenuItem;
        private System.Windows.Forms.ContextMenuStrip itemContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem quickExtractContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ReplaceContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractToMainMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem extractToContextMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem closeMainMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem toolsMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem itemDebugViewMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractRawToToolStripMenuItem;
    }
}

