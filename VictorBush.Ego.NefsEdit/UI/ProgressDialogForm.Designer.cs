namespace VictorBush.Ego.NefsEdit.UI
{
    partial class ProgressDialogForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgressDialogForm));
			this.statusLabel = new Label();
			this.progressBar = new ProgressBar();
			this.cancelButton = new Button();
			this.panel1 = new Panel();
			this.panel1.SuspendLayout();
			SuspendLayout();
			// 
			// statusLabel
			// 
			this.statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.statusLabel.Location = new Point(9, 7);
			this.statusLabel.Margin = new Padding(2, 0, 2, 0);
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new Size(400, 57);
			this.statusLabel.TabIndex = 0;
			this.statusLabel.Text = "Status";
			this.statusLabel.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// progressBar
			// 
			this.progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.progressBar.Location = new Point(13, 66);
			this.progressBar.Margin = new Padding(2);
			this.progressBar.MarqueeAnimationSpeed = 25;
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new Size(398, 15);
			this.progressBar.Style = ProgressBarStyle.Continuous;
			this.progressBar.TabIndex = 1;
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = DialogResult.Cancel;
			this.cancelButton.Location = new Point(170, 91);
			this.cancelButton.Margin = new Padding(2);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new Size(72, 28);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += CancelButton_Click;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.statusLabel);
			this.panel1.Controls.Add(this.cancelButton);
			this.panel1.Controls.Add(this.progressBar);
			this.panel1.Dock = DockStyle.Fill;
			this.panel1.Location = new Point(0, 0);
			this.panel1.Margin = new Padding(2);
			this.panel1.Name = "panel1";
			this.panel1.Size = new Size(419, 133);
			this.panel1.TabIndex = 3;
			// 
			// ProgressDialogForm
			// 
			AutoScaleDimensions = new SizeF(7F, 16F);
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = this.cancelButton;
			ClientSize = new Size(419, 133);
			ControlBox = false;
			Controls.Add(this.panel1);
			Icon = (Icon)resources.GetObject("$this.Icon");
			Margin = new Padding(2);
			MaximizeBox = false;
			Name = "ProgressDialogForm";
			SizeGripStyle = SizeGripStyle.Hide;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Progress";
			this.panel1.ResumeLayout(false);
			ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Panel panel1;
    }
}