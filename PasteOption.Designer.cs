namespace ACT_Notes
{
    partial class PasteOption
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
            this.label1 = new System.Windows.Forms.Label();
            this.buttonAppend = new System.Windows.Forms.Button();
            this.buttonOverwrite = new System.Windows.Forms.Button();
            this.buttonIgnore = new System.Windows.Forms.Button();
            this.labelCountdown = new System.Windows.Forms.Label();
            this.labelWhat = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(13, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(239, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "How should the shared note be handled?";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // buttonAppend
            // 
            this.buttonAppend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAppend.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.buttonAppend.Location = new System.Drawing.Point(13, 67);
            this.buttonAppend.Name = "buttonAppend";
            this.buttonAppend.Size = new System.Drawing.Size(75, 23);
            this.buttonAppend.TabIndex = 1;
            this.buttonAppend.Text = "Append";
            this.buttonAppend.UseVisualStyleBackColor = true;
            // 
            // buttonOverwrite
            // 
            this.buttonOverwrite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOverwrite.DialogResult = System.Windows.Forms.DialogResult.No;
            this.buttonOverwrite.Location = new System.Drawing.Point(94, 67);
            this.buttonOverwrite.Name = "buttonOverwrite";
            this.buttonOverwrite.Size = new System.Drawing.Size(75, 23);
            this.buttonOverwrite.TabIndex = 2;
            this.buttonOverwrite.Text = "Replace";
            this.buttonOverwrite.UseVisualStyleBackColor = true;
            // 
            // buttonIgnore
            // 
            this.buttonIgnore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonIgnore.DialogResult = System.Windows.Forms.DialogResult.Ignore;
            this.buttonIgnore.Location = new System.Drawing.Point(175, 67);
            this.buttonIgnore.Name = "buttonIgnore";
            this.buttonIgnore.Size = new System.Drawing.Size(75, 23);
            this.buttonIgnore.TabIndex = 3;
            this.buttonIgnore.Text = "Ignore";
            this.buttonIgnore.UseVisualStyleBackColor = true;
            // 
            // labelCountdown
            // 
            this.labelCountdown.Location = new System.Drawing.Point(12, 45);
            this.labelCountdown.Name = "labelCountdown";
            this.labelCountdown.Size = new System.Drawing.Size(240, 16);
            this.labelCountdown.TabIndex = 4;
            this.labelCountdown.Text = "(30 seconds until it\'s appended)";
            this.labelCountdown.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // labelWhat
            // 
            this.labelWhat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWhat.ForeColor = System.Drawing.Color.Red;
            this.labelWhat.Location = new System.Drawing.Point(2, 6);
            this.labelWhat.Name = "labelWhat";
            this.labelWhat.Size = new System.Drawing.Size(260, 16);
            this.labelWhat.TabIndex = 5;
            this.labelWhat.Text = "Mob Name";
            this.labelWhat.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // PasteOption
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(264, 100);
            this.ControlBox = false;
            this.Controls.Add(this.labelWhat);
            this.Controls.Add(this.labelCountdown);
            this.Controls.Add(this.buttonIgnore);
            this.Controls.Add(this.buttonOverwrite);
            this.Controls.Add(this.buttonAppend);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PasteOption";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Note Paste Options";
            this.Load += new System.EventHandler(this.PasteOption_Load);
            this.Shown += new System.EventHandler(this.PasteOption_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonAppend;
        private System.Windows.Forms.Button buttonOverwrite;
        private System.Windows.Forms.Button buttonIgnore;
        private System.Windows.Forms.Label labelCountdown;
        private System.Windows.Forms.Label labelWhat;
    }
}