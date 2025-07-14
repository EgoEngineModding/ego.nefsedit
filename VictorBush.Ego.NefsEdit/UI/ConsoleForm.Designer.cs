namespace VictorBush.Ego.NefsEdit.UI
{
    partial class ConsoleForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConsoleForm));
			this.richTextBox = new RichTextBox();
			SuspendLayout();
			// 
			// richTextBox
			// 
			this.richTextBox.Dock = DockStyle.Fill;
			this.richTextBox.Font = new Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
			this.richTextBox.Location = new Point(0, 0);
			this.richTextBox.Margin = new Padding(2, 2, 2, 2);
			this.richTextBox.Name = "richTextBox";
			this.richTextBox.ReadOnly = true;
			this.richTextBox.Size = new Size(548, 288);
			this.richTextBox.TabIndex = 0;
			this.richTextBox.Text = "";
			// 
			// ConsoleForm
			// 
			AutoScaleDimensions = new SizeF(7F, 16F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(548, 288);
			Controls.Add(this.richTextBox);
			Icon = (Icon)resources.GetObject("$this.Icon");
			Margin = new Padding(2, 2, 2, 2);
			Name = "ConsoleForm";
			Text = "ConsoleForm";
			ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox richTextBox;
    }
}