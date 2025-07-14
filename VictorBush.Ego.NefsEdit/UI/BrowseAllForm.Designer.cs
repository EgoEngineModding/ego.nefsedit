namespace VictorBush.Ego.NefsEdit.UI
{
    partial class BrowseAllForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BrowseAllForm));
			this.itemsListView = new ListView();
			SuspendLayout();
			// 
			// itemsListView
			// 
			this.itemsListView.Dock = DockStyle.Fill;
			this.itemsListView.FullRowSelect = true;
			this.itemsListView.GridLines = true;
			this.itemsListView.Location = new Point(0, 0);
			this.itemsListView.Margin = new Padding(2);
			this.itemsListView.Name = "itemsListView";
			this.itemsListView.Size = new Size(710, 614);
			this.itemsListView.TabIndex = 0;
			this.itemsListView.UseCompatibleStateImageBehavior = false;
			this.itemsListView.View = View.Details;
			this.itemsListView.SelectedIndexChanged += ItemsListView_SelectedIndexChanged;
			this.itemsListView.MouseUp += ItemsListView_MouseUp;
			// 
			// BrowseAllForm
			// 
			AutoScaleDimensions = new SizeF(7F, 16F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(710, 614);
			Controls.Add(this.itemsListView);
			Icon = (Icon)resources.GetObject("$this.Icon");
			Margin = new Padding(2);
			Name = "BrowseAllForm";
			Text = "Debug View";
			ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView itemsListView;
    }
}