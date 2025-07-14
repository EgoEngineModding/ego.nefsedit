namespace VictorBush.Ego.NefsEdit.UI
{
    partial class SettingsForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
			this.cancelButton = new Button();
			this.saveButton = new Button();
			this.quickExtractTextBox = new TextBox();
			this.dirtRally2TextBox = new TextBox();
			this.quickExtractLabel = new Label();
			this.dirtRally2Label = new Label();
			this.browseDirtRally2Button = new Button();
			this.quickExtractButton = new Button();
			this.dirt4Button = new Button();
			this.dirt4Label = new Label();
			this.dirt4TextBox = new TextBox();
			this.dirtRallyButton = new Button();
			this.dirtRallyLabel = new Label();
			this.dirtRallyTextBox = new TextBox();
			SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.cancelButton.DialogResult = DialogResult.Cancel;
			this.cancelButton.Location = new Point(220, 215);
			this.cancelButton.Margin = new Padding(4, 4, 4, 4);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new Size(88, 28);
			this.cancelButton.TabIndex = 0;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += CancelButton_Click;
			// 
			// saveButton
			// 
			this.saveButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.saveButton.Location = new Point(126, 215);
			this.saveButton.Margin = new Padding(4, 4, 4, 4);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new Size(88, 28);
			this.saveButton.TabIndex = 1;
			this.saveButton.Text = "Save";
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += SaveButton_Click;
			// 
			// quickExtractTextBox
			// 
			this.quickExtractTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.quickExtractTextBox.Location = new Point(14, 30);
			this.quickExtractTextBox.Margin = new Padding(4, 4, 4, 4);
			this.quickExtractTextBox.Name = "quickExtractTextBox";
			this.quickExtractTextBox.Size = new Size(252, 23);
			this.quickExtractTextBox.TabIndex = 2;
			// 
			// dirtRally2TextBox
			// 
			this.dirtRally2TextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.dirtRally2TextBox.Location = new Point(14, 126);
			this.dirtRally2TextBox.Margin = new Padding(4, 4, 4, 4);
			this.dirtRally2TextBox.Name = "dirtRally2TextBox";
			this.dirtRally2TextBox.Size = new Size(252, 23);
			this.dirtRally2TextBox.TabIndex = 3;
			// 
			// quickExtractLabel
			// 
			this.quickExtractLabel.AutoSize = true;
			this.quickExtractLabel.Location = new Point(10, 10);
			this.quickExtractLabel.Margin = new Padding(4, 0, 4, 0);
			this.quickExtractLabel.Name = "quickExtractLabel";
			this.quickExtractLabel.Size = new Size(114, 16);
			this.quickExtractLabel.TabIndex = 4;
			this.quickExtractLabel.Text = "Quick Extract Path";
			// 
			// dirtRally2Label
			// 
			this.dirtRally2Label.AutoSize = true;
			this.dirtRally2Label.Location = new Point(14, 106);
			this.dirtRally2Label.Margin = new Padding(4, 0, 4, 0);
			this.dirtRally2Label.Name = "dirtRally2Label";
			this.dirtRally2Label.Size = new Size(101, 16);
			this.dirtRally2Label.TabIndex = 5;
			this.dirtRally2Label.Text = "DiRT Rally 2 Path";
			// 
			// browseDirtRally2Button
			// 
			this.browseDirtRally2Button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.browseDirtRally2Button.Location = new Point(274, 123);
			this.browseDirtRally2Button.Margin = new Padding(4, 4, 4, 4);
			this.browseDirtRally2Button.Name = "browseDirtRally2Button";
			this.browseDirtRally2Button.Size = new Size(34, 28);
			this.browseDirtRally2Button.TabIndex = 6;
			this.browseDirtRally2Button.Text = "...";
			this.browseDirtRally2Button.UseVisualStyleBackColor = true;
			this.browseDirtRally2Button.Click += BrowseDirtRally2Button_Click;
			// 
			// quickExtractButton
			// 
			this.quickExtractButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.quickExtractButton.Location = new Point(274, 27);
			this.quickExtractButton.Margin = new Padding(4, 4, 4, 4);
			this.quickExtractButton.Name = "quickExtractButton";
			this.quickExtractButton.Size = new Size(34, 28);
			this.quickExtractButton.TabIndex = 7;
			this.quickExtractButton.Text = "...";
			this.quickExtractButton.UseVisualStyleBackColor = true;
			this.quickExtractButton.Click += QuickExtractButton_Click;
			// 
			// dirt4Button
			// 
			this.dirt4Button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.dirt4Button.Location = new Point(274, 171);
			this.dirt4Button.Margin = new Padding(4, 4, 4, 4);
			this.dirt4Button.Name = "dirt4Button";
			this.dirt4Button.Size = new Size(34, 28);
			this.dirt4Button.TabIndex = 10;
			this.dirt4Button.Text = "...";
			this.dirt4Button.UseVisualStyleBackColor = true;
			this.dirt4Button.Click += Dirt4Button_Click;
			// 
			// dirt4Label
			// 
			this.dirt4Label.AutoSize = true;
			this.dirt4Label.Location = new Point(14, 154);
			this.dirt4Label.Margin = new Padding(4, 0, 4, 0);
			this.dirt4Label.Name = "dirt4Label";
			this.dirt4Label.Size = new Size(73, 16);
			this.dirt4Label.TabIndex = 9;
			this.dirt4Label.Text = "DiRT 4 Path";
			// 
			// dirt4TextBox
			// 
			this.dirt4TextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.dirt4TextBox.Location = new Point(14, 174);
			this.dirt4TextBox.Margin = new Padding(4, 4, 4, 4);
			this.dirt4TextBox.Name = "dirt4TextBox";
			this.dirt4TextBox.Size = new Size(252, 23);
			this.dirt4TextBox.TabIndex = 8;
			// 
			// dirtRallyButton
			// 
			this.dirtRallyButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.dirtRallyButton.Location = new Point(275, 75);
			this.dirtRallyButton.Margin = new Padding(4, 4, 4, 4);
			this.dirtRallyButton.Name = "dirtRallyButton";
			this.dirtRallyButton.Size = new Size(34, 28);
			this.dirtRallyButton.TabIndex = 13;
			this.dirtRallyButton.Text = "...";
			this.dirtRallyButton.UseVisualStyleBackColor = true;
			this.dirtRallyButton.Click += DirtRallyButton_Click;
			// 
			// dirtRallyLabel
			// 
			this.dirtRallyLabel.AutoSize = true;
			this.dirtRallyLabel.Location = new Point(15, 58);
			this.dirtRallyLabel.Margin = new Padding(4, 0, 4, 0);
			this.dirtRallyLabel.Name = "dirtRallyLabel";
			this.dirtRallyLabel.Size = new Size(91, 16);
			this.dirtRallyLabel.TabIndex = 12;
			this.dirtRallyLabel.Text = "DiRT Rally Path";
			// 
			// dirtRallyTextBox
			// 
			this.dirtRallyTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.dirtRallyTextBox.Location = new Point(15, 78);
			this.dirtRallyTextBox.Margin = new Padding(4, 4, 4, 4);
			this.dirtRallyTextBox.Name = "dirtRallyTextBox";
			this.dirtRallyTextBox.Size = new Size(252, 23);
			this.dirtRallyTextBox.TabIndex = 11;
			// 
			// SettingsForm
			// 
			AcceptButton = this.saveButton;
			AutoScaleDimensions = new SizeF(7F, 16F);
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = this.cancelButton;
			ClientSize = new Size(322, 258);
			Controls.Add(this.dirtRallyButton);
			Controls.Add(this.dirtRallyLabel);
			Controls.Add(this.dirtRallyTextBox);
			Controls.Add(this.dirt4Button);
			Controls.Add(this.dirt4Label);
			Controls.Add(this.dirt4TextBox);
			Controls.Add(this.quickExtractButton);
			Controls.Add(this.browseDirtRally2Button);
			Controls.Add(this.dirtRally2Label);
			Controls.Add(this.quickExtractLabel);
			Controls.Add(this.dirtRally2TextBox);
			Controls.Add(this.quickExtractTextBox);
			Controls.Add(this.saveButton);
			Controls.Add(this.cancelButton);
			Icon = (Icon)resources.GetObject("$this.Icon");
			Margin = new Padding(4, 4, 4, 4);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "SettingsForm";
			StartPosition = FormStartPosition.CenterParent;
			Text = "Settings";
			Load += SettingsForm_Load;
			ResumeLayout(false);
			PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.TextBox quickExtractTextBox;
        private System.Windows.Forms.TextBox dirtRally2TextBox;
        private System.Windows.Forms.Label quickExtractLabel;
        private System.Windows.Forms.Label dirtRally2Label;
        private System.Windows.Forms.Button browseDirtRally2Button;
        private System.Windows.Forms.Button quickExtractButton;
        private System.Windows.Forms.Button dirt4Button;
        private System.Windows.Forms.Label dirt4Label;
        private System.Windows.Forms.TextBox dirt4TextBox;
        private System.Windows.Forms.Button dirtRallyButton;
        private System.Windows.Forms.Label dirtRallyLabel;
        private System.Windows.Forms.TextBox dirtRallyTextBox;
    }
}
