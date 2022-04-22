
namespace ACT_Notes
{
    partial class XmlCopyForm
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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.buttonCopy = new System.Windows.Forms.Button();
            this.buttonDone = new System.Windows.Forms.Button();
            this.labelReady = new System.Windows.Forms.Label();
            this.radioButtonG = new System.Windows.Forms.RadioButton();
            this.radioButtonR = new System.Windows.Forms.RadioButton();
            this.radioButtonCustom = new System.Windows.Forms.RadioButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.textBoxCustom = new System.Windows.Forms.TextBox();
            this.buttonMacro = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(13, 13);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(239, 108);
            this.listBox1.TabIndex = 0;
            // 
            // buttonCopy
            // 
            this.buttonCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCopy.Location = new System.Drawing.Point(95, 173);
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.Size = new System.Drawing.Size(75, 23);
            this.buttonCopy.TabIndex = 5;
            this.buttonCopy.Text = "Copy";
            this.toolTip1.SetToolTip(this.buttonCopy, "Press to copy the selected XML chunk to the clipboard");
            this.buttonCopy.UseVisualStyleBackColor = true;
            this.buttonCopy.Click += new System.EventHandler(this.buttonCopy_Click);
            // 
            // buttonDone
            // 
            this.buttonDone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonDone.Location = new System.Drawing.Point(176, 173);
            this.buttonDone.Name = "buttonDone";
            this.buttonDone.Size = new System.Drawing.Size(75, 23);
            this.buttonDone.TabIndex = 6;
            this.buttonDone.Text = "Done";
            this.buttonDone.UseVisualStyleBackColor = true;
            this.buttonDone.Click += new System.EventHandler(this.buttonDone_Click);
            // 
            // labelReady
            // 
            this.labelReady.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelReady.AutoSize = true;
            this.labelReady.ForeColor = System.Drawing.SystemColors.Highlight;
            this.labelReady.Location = new System.Drawing.Point(12, 153);
            this.labelReady.Name = "labelReady";
            this.labelReady.Size = new System.Drawing.Size(131, 13);
            this.labelReady.TabIndex = 4;
            this.labelReady.Text = "[Copy] the selected chunk";
            // 
            // radioButtonG
            // 
            this.radioButtonG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButtonG.AutoSize = true;
            this.radioButtonG.Location = new System.Drawing.Point(15, 127);
            this.radioButtonG.Name = "radioButtonG";
            this.radioButtonG.Size = new System.Drawing.Size(36, 17);
            this.radioButtonG.TabIndex = 2;
            this.radioButtonG.TabStop = true;
            this.radioButtonG.Text = "/g";
            this.toolTip1.SetToolTip(this.radioButtonG, "Prefix the XML with a /g for pasting in EQ2 group chat");
            this.radioButtonG.UseVisualStyleBackColor = true;
            // 
            // radioButtonR
            // 
            this.radioButtonR.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButtonR.AutoSize = true;
            this.radioButtonR.Location = new System.Drawing.Point(57, 127);
            this.radioButtonR.Name = "radioButtonR";
            this.radioButtonR.Size = new System.Drawing.Size(33, 17);
            this.radioButtonR.TabIndex = 3;
            this.radioButtonR.TabStop = true;
            this.radioButtonR.Text = "/r";
            this.toolTip1.SetToolTip(this.radioButtonR, "Prefix the XML with /r for pasting in EQ2 raid chat");
            this.radioButtonR.UseVisualStyleBackColor = true;
            // 
            // radioButtonCustom
            // 
            this.radioButtonCustom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButtonCustom.AutoSize = true;
            this.radioButtonCustom.Location = new System.Drawing.Point(93, 127);
            this.radioButtonCustom.Name = "radioButtonCustom";
            this.radioButtonCustom.Size = new System.Drawing.Size(62, 17);
            this.radioButtonCustom.TabIndex = 1;
            this.radioButtonCustom.TabStop = true;
            this.radioButtonCustom.Text = "custom:";
            this.toolTip1.SetToolTip(this.radioButtonCustom, "Custom prefix. e.g. /gu for guild chat");
            this.radioButtonCustom.UseVisualStyleBackColor = true;
            this.radioButtonCustom.CheckedChanged += new System.EventHandler(this.radioButtonCustom_CheckedChanged);
            // 
            // textBoxCustom
            // 
            this.textBoxCustom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCustom.Location = new System.Drawing.Point(151, 126);
            this.textBoxCustom.Name = "textBoxCustom";
            this.textBoxCustom.Size = new System.Drawing.Size(101, 20);
            this.textBoxCustom.TabIndex = 7;
            this.textBoxCustom.TextChanged += new System.EventHandler(this.textBoxCustom_TextChanged);
            // 
            // buttonMacro
            // 
            this.buttonMacro.Location = new System.Drawing.Point(15, 173);
            this.buttonMacro.Name = "buttonMacro";
            this.buttonMacro.Size = new System.Drawing.Size(75, 23);
            this.buttonMacro.TabIndex = 8;
            this.buttonMacro.Text = "Macro";
            this.toolTip1.SetToolTip(this.buttonMacro, "Create note-macroX.txt file(s)");
            this.buttonMacro.UseVisualStyleBackColor = true;
            this.buttonMacro.Click += new System.EventHandler(this.buttonMacro_Click);
            // 
            // XmlCopyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(264, 204);
            this.ControlBox = false;
            this.Controls.Add(this.buttonMacro);
            this.Controls.Add(this.textBoxCustom);
            this.Controls.Add(this.radioButtonCustom);
            this.Controls.Add(this.radioButtonR);
            this.Controls.Add(this.radioButtonG);
            this.Controls.Add(this.labelReady);
            this.Controls.Add(this.buttonDone);
            this.Controls.Add(this.buttonCopy);
            this.Controls.Add(this.listBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "XmlCopyForm";
            this.Text = "Xml Share";
            this.Load += new System.EventHandler(this.XmlCopyForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button buttonCopy;
        private System.Windows.Forms.Button buttonDone;
        private System.Windows.Forms.Label labelReady;
        private System.Windows.Forms.RadioButton radioButtonG;
        private System.Windows.Forms.RadioButton radioButtonR;
        private System.Windows.Forms.RadioButton radioButtonCustom;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox textBoxCustom;
        private System.Windows.Forms.Button buttonMacro;
    }
}