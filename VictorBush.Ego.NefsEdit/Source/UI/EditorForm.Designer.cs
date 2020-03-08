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
            this.MainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ItemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.QuickExtractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetExtractionDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.ExtractToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ReplaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TreeViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DebugViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ArchiveDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TtemDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.vS2005Theme1 = new WeifenLuo.WinFormsUI.Docking.VS2005Theme();
            this.browserDockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.itemContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.quickExtractContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractToContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ReplaceContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MainMenuStrip.SuspendLayout();
            this.itemContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainMenuStrip
            // 
            this.MainMenuStrip.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.MainMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.MainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.ItemToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.MainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MainMenuStrip.Name = "MainMenuStrip";
            this.MainMenuStrip.Size = new System.Drawing.Size(1408, 33);
            this.MainMenuStrip.TabIndex = 8;
            this.MainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenToolStripMenuItem,
            this.SaveToolStripMenuItem,
            this.SaveAsToolStripMenuItem,
            this.toolStripSeparator2,
            this.ExitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(54, 29);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // OpenToolStripMenuItem
            // 
            this.OpenToolStripMenuItem.Name = "OpenToolStripMenuItem";
            this.OpenToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.OpenToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.OpenToolStripMenuItem.Text = "Open...";
            this.OpenToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
            // 
            // SaveToolStripMenuItem
            // 
            this.SaveToolStripMenuItem.Name = "SaveToolStripMenuItem";
            this.SaveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.SaveToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.SaveToolStripMenuItem.Text = "Save";
            this.SaveToolStripMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItem_Click);
            // 
            // SaveAsToolStripMenuItem
            // 
            this.SaveAsToolStripMenuItem.Name = "SaveAsToolStripMenuItem";
            this.SaveAsToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.SaveAsToolStripMenuItem.Text = "Save As...";
            this.SaveAsToolStripMenuItem.Click += new System.EventHandler(this.SaveAsToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(267, 6);
            // 
            // ExitToolStripMenuItem
            // 
            this.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem";
            this.ExitToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.ExitToolStripMenuItem.Text = "Exit";
            this.ExitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // itemToolStripMenuItem
            // 
            this.ItemToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.QuickExtractToolStripMenuItem,
            this.SetExtractionDirectoryToolStripMenuItem,
            this.toolStripSeparator4,
            this.ExtractToToolStripMenuItem,
            this.toolStripSeparator3,
            this.ReplaceToolStripMenuItem});
            this.ItemToolStripMenuItem.Name = "itemToolStripMenuItem";
            this.ItemToolStripMenuItem.Size = new System.Drawing.Size(64, 29);
            this.ItemToolStripMenuItem.Text = "Item";
            // 
            // QuickExtractToolStripMenuItem
            // 
            this.QuickExtractToolStripMenuItem.Name = "QuickExtractToolStripMenuItem";
            this.QuickExtractToolStripMenuItem.Size = new System.Drawing.Size(311, 34);
            this.QuickExtractToolStripMenuItem.Text = "Quick Extract";
            this.QuickExtractToolStripMenuItem.Click += new System.EventHandler(this.quickExtractToolStripMenuItem_Click);
            // 
            // SetExtractionDirectoryToolStripMenuItem
            // 
            this.SetExtractionDirectoryToolStripMenuItem.Name = "SetExtractionDirectoryToolStripMenuItem";
            this.SetExtractionDirectoryToolStripMenuItem.Size = new System.Drawing.Size(311, 34);
            this.SetExtractionDirectoryToolStripMenuItem.Text = "Configure Quick Extract...";
            this.SetExtractionDirectoryToolStripMenuItem.Click += new System.EventHandler(this.setExtractionDirectoryToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(308, 6);
            // 
            // ExtractToToolStripMenuItem
            // 
            this.ExtractToToolStripMenuItem.Name = "ExtractToToolStripMenuItem";
            this.ExtractToToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.ExtractToToolStripMenuItem.Size = new System.Drawing.Size(311, 34);
            this.ExtractToToolStripMenuItem.Text = "Extract To...";
            this.ExtractToToolStripMenuItem.Click += new System.EventHandler(this.extractToToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(308, 6);
            // 
            // ReplaceToolStripMenuItem
            // 
            this.ReplaceToolStripMenuItem.Name = "ReplaceToolStripMenuItem";
            this.ReplaceToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.ReplaceToolStripMenuItem.Size = new System.Drawing.Size(311, 34);
            this.ReplaceToolStripMenuItem.Text = "Replace...";
            this.ReplaceToolStripMenuItem.Click += new System.EventHandler(this.replaceToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TreeViewToolStripMenuItem,
            this.DebugViewToolStripMenuItem,
            this.toolStripSeparator1,
            this.ArchiveDetailsToolStripMenuItem,
            this.TtemDetailsToolStripMenuItem,
            this.ConsoleToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(65, 29);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // TreeViewToolStripMenuItem
            // 
            this.TreeViewToolStripMenuItem.Name = "TreeViewToolStripMenuItem";
            this.TreeViewToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.TreeViewToolStripMenuItem.Text = "Tree View";
            this.TreeViewToolStripMenuItem.Click += new System.EventHandler(this.TreeViewToolStripMenuItem_Click);
            // 
            // DebugViewToolStripMenuItem
            // 
            this.DebugViewToolStripMenuItem.Name = "DebugViewToolStripMenuItem";
            this.DebugViewToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.DebugViewToolStripMenuItem.Text = "Debug View";
            this.DebugViewToolStripMenuItem.Click += new System.EventHandler(this.debugViewToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(267, 6);
            // 
            // ArchiveDetailsToolStripMenuItem
            // 
            this.ArchiveDetailsToolStripMenuItem.Name = "ArchiveDetailsToolStripMenuItem";
            this.ArchiveDetailsToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.ArchiveDetailsToolStripMenuItem.Text = "Archive Details";
            this.ArchiveDetailsToolStripMenuItem.Click += new System.EventHandler(this.archiveDetailsToolStripMenuItem_Click);
            // 
            // TtemDetailsToolStripMenuItem
            // 
            this.TtemDetailsToolStripMenuItem.Name = "TtemDetailsToolStripMenuItem";
            this.TtemDetailsToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.TtemDetailsToolStripMenuItem.Text = "Item Details";
            this.TtemDetailsToolStripMenuItem.Click += new System.EventHandler(this.itemDetailsToolStripMenuItem_Click);
            // 
            // ConsoleToolStripMenuItem
            // 
            this.ConsoleToolStripMenuItem.Name = "ConsoleToolStripMenuItem";
            this.ConsoleToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.ConsoleToolStripMenuItem.Text = "Console";
            this.ConsoleToolStripMenuItem.Click += new System.EventHandler(this.consoleToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(65, 29);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // AboutToolStripMenuItem
            // 
            this.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem";
            this.AboutToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.AboutToolStripMenuItem.Text = "About";
            this.AboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // BrowserDockPanel
            // 
            this.browserDockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browserDockPanel.DocumentStyle = WeifenLuo.WinFormsUI.Docking.DocumentStyle.DockingWindow;
            this.browserDockPanel.Location = new System.Drawing.Point(0, 33);
            this.browserDockPanel.Name = "BrowserDockPanel";
            this.browserDockPanel.Size = new System.Drawing.Size(1408, 1322);
            this.browserDockPanel.TabIndex = 9;
            // 
            // itemContextMenuStrip
            // 
            this.itemContextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.itemContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quickExtractContextMenuItem,
            this.extractToContextMenuItem,
            this.ReplaceContextMenuItem});
            this.itemContextMenuStrip.Name = "itemContextMenuStrip";
            this.itemContextMenuStrip.Size = new System.Drawing.Size(187, 100);
            this.itemContextMenuStrip.Text = "Item";
            // 
            // quickExtractContextMenuItem
            // 
            this.quickExtractContextMenuItem.Name = "quickExtractContextMenuItem";
            this.quickExtractContextMenuItem.Size = new System.Drawing.Size(186, 32);
            this.quickExtractContextMenuItem.Text = "Quick Extract";
            this.quickExtractContextMenuItem.Click += new System.EventHandler(this.quickExtractContextMenuItem_Click);
            // 
            // extractToContextMenuItem
            // 
            this.extractToContextMenuItem.Name = "extractToContextMenuItem";
            this.extractToContextMenuItem.Size = new System.Drawing.Size(186, 32);
            this.extractToContextMenuItem.Text = "Extract To...";
            this.extractToContextMenuItem.Click += new System.EventHandler(this.extractToContextMenuItem_Click);
            // 
            // replaceContextMenuItem
            // 
            this.ReplaceContextMenuItem.Name = "replaceContextMenuItem";
            this.ReplaceContextMenuItem.Size = new System.Drawing.Size(186, 32);
            this.ReplaceContextMenuItem.Text = "Replace...";
            this.ReplaceContextMenuItem.Click += new System.EventHandler(this.ReplaceToolStripMenuItem_Click);
            // 
            // EditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1408, 1355);
            this.Controls.Add(this.browserDockPanel);
            this.Controls.Add(this.MainMenuStrip);
            this.MainMenuStrip = this.MainMenuStrip;
            this.Name = "EditorForm";
            this.Text = "NeFS Edit";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditorForm_FormClosing);
            this.Load += new System.EventHandler(this.EditorForm_Load);
            this.MainMenuStrip.ResumeLayout(false);
            this.MainMenuStrip.PerformLayout();
            this.itemContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip MainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private WeifenLuo.WinFormsUI.Docking.VS2005Theme vS2005Theme1;
        private WeifenLuo.WinFormsUI.Docking.DockPanel browserDockPanel;
        private System.Windows.Forms.ToolStripMenuItem ItemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem QuickExtractToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem OpenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ReplaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SaveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ArchiveDetailsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem TtemDetailsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem TreeViewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DebugViewToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ConsoleToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem ExitToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip itemContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem quickExtractContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ReplaceContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SaveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ExtractToToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem extractToContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SetExtractionDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
    }
}

