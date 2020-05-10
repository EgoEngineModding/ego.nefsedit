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
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.closeMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.saveMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.itemMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quickExtractMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.extractToMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.replaceMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeViewMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugViewMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.archiveDebugMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.itemDebugViewMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.itemDetailsMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.consoleMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.vS2005Theme1 = new WeifenLuo.WinFormsUI.Docking.VS2005Theme();
            this.browserDockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.itemContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.quickExtractContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractToContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ReplaceContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainMenuStrip.SuspendLayout();
            this.itemContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.mainMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMainMenuItem,
            this.editToolStripMenuItem,
            this.itemMainMenuItem,
            this.viewMainMenuItem,
            this.toolsMainMenuItem,
            this.helpMainMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Padding = new System.Windows.Forms.Padding(6, 2, 0, 2);
            this.mainMenuStrip.Size = new System.Drawing.Size(1408, 36);
            this.mainMenuStrip.TabIndex = 8;
            this.mainMenuStrip.Text = "menuStrip1";
            // 
            // fileMainMenuItem
            // 
            this.fileMainMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openMainMenuItem,
            this.toolStripSeparator5,
            this.closeMainMenuItem,
            this.toolStripSeparator6,
            this.saveMainMenuItem,
            this.saveAsMainMenuItem,
            this.toolStripSeparator2,
            this.exitMainMenuItem});
            this.fileMainMenuItem.Name = "fileMainMenuItem";
            this.fileMainMenuItem.Size = new System.Drawing.Size(54, 32);
            this.fileMainMenuItem.Text = "File";
            // 
            // openMainMenuItem
            // 
            this.openMainMenuItem.Name = "openMainMenuItem";
            this.openMainMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openMainMenuItem.Size = new System.Drawing.Size(235, 34);
            this.openMainMenuItem.Text = "Open...";
            this.openMainMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(232, 6);
            // 
            // closeMainMenuItem
            // 
            this.closeMainMenuItem.Name = "closeMainMenuItem";
            this.closeMainMenuItem.Size = new System.Drawing.Size(235, 34);
            this.closeMainMenuItem.Text = "Close";
            this.closeMainMenuItem.Click += new System.EventHandler(this.CloseMainMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(232, 6);
            // 
            // saveMainMenuItem
            // 
            this.saveMainMenuItem.Name = "saveMainMenuItem";
            this.saveMainMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveMainMenuItem.Size = new System.Drawing.Size(235, 34);
            this.saveMainMenuItem.Text = "Save";
            this.saveMainMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItem_Click);
            // 
            // saveAsMainMenuItem
            // 
            this.saveAsMainMenuItem.Name = "saveAsMainMenuItem";
            this.saveAsMainMenuItem.Size = new System.Drawing.Size(235, 34);
            this.saveAsMainMenuItem.Text = "Save As...";
            this.saveAsMainMenuItem.Click += new System.EventHandler(this.SaveAsToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(232, 6);
            // 
            // exitMainMenuItem
            // 
            this.exitMainMenuItem.Name = "exitMainMenuItem";
            this.exitMainMenuItem.Size = new System.Drawing.Size(235, 34);
            this.exitMainMenuItem.Text = "Exit";
            this.exitMainMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoMainMenuItem,
            this.redoMainMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(58, 32);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // undoMainMenuItem
            // 
            this.undoMainMenuItem.Name = "undoMainMenuItem";
            this.undoMainMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.undoMainMenuItem.Size = new System.Drawing.Size(219, 34);
            this.undoMainMenuItem.Text = "Undo";
            this.undoMainMenuItem.Click += new System.EventHandler(this.UndoMainMenuItem_Click);
            // 
            // redoMainMenuItem
            // 
            this.redoMainMenuItem.Name = "redoMainMenuItem";
            this.redoMainMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.redoMainMenuItem.Size = new System.Drawing.Size(219, 34);
            this.redoMainMenuItem.Text = "Redo";
            this.redoMainMenuItem.Click += new System.EventHandler(this.RedoMainMenuItem_Click);
            // 
            // itemMainMenuItem
            // 
            this.itemMainMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quickExtractMainMenuItem,
            this.toolStripSeparator4,
            this.extractToMainMenuItem,
            this.toolStripSeparator3,
            this.replaceMainMenuItem});
            this.itemMainMenuItem.Name = "itemMainMenuItem";
            this.itemMainMenuItem.Size = new System.Drawing.Size(64, 32);
            this.itemMainMenuItem.Text = "Item";
            // 
            // quickExtractMainMenuItem
            // 
            this.quickExtractMainMenuItem.Name = "quickExtractMainMenuItem";
            this.quickExtractMainMenuItem.Size = new System.Drawing.Size(261, 34);
            this.quickExtractMainMenuItem.Text = "Quick Extract";
            this.quickExtractMainMenuItem.Click += new System.EventHandler(this.QuickExtractToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(258, 6);
            // 
            // extractToMainMenuItem
            // 
            this.extractToMainMenuItem.Name = "extractToMainMenuItem";
            this.extractToMainMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.extractToMainMenuItem.Size = new System.Drawing.Size(261, 34);
            this.extractToMainMenuItem.Text = "Extract To...";
            this.extractToMainMenuItem.Click += new System.EventHandler(this.ExtractToToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(258, 6);
            // 
            // replaceMainMenuItem
            // 
            this.replaceMainMenuItem.Name = "replaceMainMenuItem";
            this.replaceMainMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.replaceMainMenuItem.Size = new System.Drawing.Size(261, 34);
            this.replaceMainMenuItem.Text = "Replace...";
            this.replaceMainMenuItem.Click += new System.EventHandler(this.ReplaceToolStripMenuItem_Click);
            // 
            // viewMainMenuItem
            // 
            this.viewMainMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.treeViewMainMenuItem,
            this.debugViewMainMenuItem,
            this.archiveDebugMainMenuItem,
            this.itemDebugViewMainMenuItem,
            this.toolStripSeparator1,
            this.itemDetailsMainMenuItem,
            this.consoleMainMenuItem});
            this.viewMainMenuItem.Name = "viewMainMenuItem";
            this.viewMainMenuItem.Size = new System.Drawing.Size(65, 32);
            this.viewMainMenuItem.Text = "View";
            // 
            // treeViewMainMenuItem
            // 
            this.treeViewMainMenuItem.Name = "treeViewMainMenuItem";
            this.treeViewMainMenuItem.Size = new System.Drawing.Size(273, 34);
            this.treeViewMainMenuItem.Text = "Tree View";
            this.treeViewMainMenuItem.Click += new System.EventHandler(this.TreeViewToolStripMenuItem_Click);
            // 
            // debugViewMainMenuItem
            // 
            this.debugViewMainMenuItem.Name = "debugViewMainMenuItem";
            this.debugViewMainMenuItem.Size = new System.Drawing.Size(273, 34);
            this.debugViewMainMenuItem.Text = "Debug View";
            this.debugViewMainMenuItem.Click += new System.EventHandler(this.DebugViewToolStripMenuItem_Click);
            // 
            // archiveDebugMainMenuItem
            // 
            this.archiveDebugMainMenuItem.Name = "archiveDebugMainMenuItem";
            this.archiveDebugMainMenuItem.Size = new System.Drawing.Size(273, 34);
            this.archiveDebugMainMenuItem.Text = "Archive Debug View";
            this.archiveDebugMainMenuItem.Click += new System.EventHandler(this.ArchiveDebugMainMenuItem_Click);
            // 
            // itemDebugViewMainMenuItem
            // 
            this.itemDebugViewMainMenuItem.Name = "itemDebugViewMainMenuItem";
            this.itemDebugViewMainMenuItem.Size = new System.Drawing.Size(273, 34);
            this.itemDebugViewMainMenuItem.Text = "Item Debug View";
            this.itemDebugViewMainMenuItem.Click += new System.EventHandler(this.ItemDebugViewMainMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(270, 6);
            // 
            // itemDetailsMainMenuItem
            // 
            this.itemDetailsMainMenuItem.Name = "itemDetailsMainMenuItem";
            this.itemDetailsMainMenuItem.Size = new System.Drawing.Size(273, 34);
            this.itemDetailsMainMenuItem.Text = "Item Details";
            this.itemDetailsMainMenuItem.Click += new System.EventHandler(this.ItemDetailsToolStripMenuItem_Click);
            // 
            // consoleMainMenuItem
            // 
            this.consoleMainMenuItem.Name = "consoleMainMenuItem";
            this.consoleMainMenuItem.Size = new System.Drawing.Size(273, 34);
            this.consoleMainMenuItem.Text = "Console";
            this.consoleMainMenuItem.Click += new System.EventHandler(this.ConsoleToolStripMenuItem_Click);
            // 
            // toolsMainMenuItem
            // 
            this.toolsMainMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsMainMenuItem});
            this.toolsMainMenuItem.Name = "toolsMainMenuItem";
            this.toolsMainMenuItem.Size = new System.Drawing.Size(69, 32);
            this.toolsMainMenuItem.Text = "Tools";
            // 
            // optionsMainMenuItem
            // 
            this.optionsMainMenuItem.Name = "optionsMainMenuItem";
            this.optionsMainMenuItem.Size = new System.Drawing.Size(178, 34);
            this.optionsMainMenuItem.Text = "Options";
            this.optionsMainMenuItem.Click += new System.EventHandler(this.OptionsMainMenuItem_Click);
            // 
            // helpMainMenuItem
            // 
            this.helpMainMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutMainMenuItem});
            this.helpMainMenuItem.Name = "helpMainMenuItem";
            this.helpMainMenuItem.Size = new System.Drawing.Size(65, 32);
            this.helpMainMenuItem.Text = "Help";
            // 
            // aboutMainMenuItem
            // 
            this.aboutMainMenuItem.Name = "aboutMainMenuItem";
            this.aboutMainMenuItem.Size = new System.Drawing.Size(164, 34);
            this.aboutMainMenuItem.Text = "About";
            this.aboutMainMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // browserDockPanel
            // 
            this.browserDockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browserDockPanel.DocumentStyle = WeifenLuo.WinFormsUI.Docking.DocumentStyle.DockingWindow;
            this.browserDockPanel.Location = new System.Drawing.Point(0, 36);
            this.browserDockPanel.Name = "browserDockPanel";
            this.browserDockPanel.Size = new System.Drawing.Size(1408, 1026);
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
            this.quickExtractContextMenuItem.Click += new System.EventHandler(this.QuickExtractContextMenuItem_Click);
            // 
            // extractToContextMenuItem
            // 
            this.extractToContextMenuItem.Name = "extractToContextMenuItem";
            this.extractToContextMenuItem.Size = new System.Drawing.Size(186, 32);
            this.extractToContextMenuItem.Text = "Extract To...";
            this.extractToContextMenuItem.Click += new System.EventHandler(this.ExtractToContextMenuItem_Click);
            // 
            // ReplaceContextMenuItem
            // 
            this.ReplaceContextMenuItem.Name = "ReplaceContextMenuItem";
            this.ReplaceContextMenuItem.Size = new System.Drawing.Size(186, 32);
            this.ReplaceContextMenuItem.Text = "Replace...";
            this.ReplaceContextMenuItem.Click += new System.EventHandler(this.ReplaceToolStripMenuItem_Click);
            // 
            // EditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1408, 1062);
            this.Controls.Add(this.browserDockPanel);
            this.Controls.Add(this.mainMenuStrip);
            this.Name = "EditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NeFS Edit";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditorForm_FormClosing);
            this.Load += new System.EventHandler(this.EditorForm_Load);
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.itemContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileMainMenuItem;
        private WeifenLuo.WinFormsUI.Docking.VS2005Theme vS2005Theme1;
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
    }
}

