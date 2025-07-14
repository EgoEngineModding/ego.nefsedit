namespace VictorBush.Ego.NefsEdit.UI
{
    partial class PropertyGridForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertyGridForm));
			this.propertyGrid = new PropertyGrid();
			SuspendLayout();
			// 
			// propertyGrid
			// 
			this.propertyGrid.Dock = DockStyle.Fill;
			this.propertyGrid.LineColor = SystemColors.ControlDark;
			this.propertyGrid.Location = new Point(0, 0);
			this.propertyGrid.Margin = new Padding(2, 2, 2, 2);
			this.propertyGrid.Name = "propertyGrid";
			this.propertyGrid.Size = new Size(459, 519);
			this.propertyGrid.TabIndex = 0;
			// 
			// PropertyGridForm
			// 
			AutoScaleDimensions = new SizeF(7F, 16F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(459, 519);
			Controls.Add(this.propertyGrid);
			Icon = (Icon)resources.GetObject("$this.Icon");
			Margin = new Padding(2, 2, 2, 2);
			Name = "PropertyGridForm";
			Text = "PropertyGridForm";
			ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PropertyGrid propertyGrid;
    }
}