namespace ACT_Notes
{
    partial class Differences
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.labelOrig = new System.Windows.Forms.Label();
            this.webBrowser2 = new System.Windows.Forms.WebBrowser();
            this.labelReceived = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonRepalce = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.webBrowser1);
            this.splitContainer1.Panel1.Controls.Add(this.labelOrig);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.webBrowser2);
            this.splitContainer1.Panel2.Controls.Add(this.labelReceived);
            this.splitContainer1.Size = new System.Drawing.Size(800, 418);
            this.splitContainer1.SplitterDistance = 382;
            this.splitContainer1.TabIndex = 0;
            // 
            // webBrowser1
            // 
            this.webBrowser1.AllowWebBrowserDrop = false;
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.IsWebBrowserContextMenuEnabled = false;
            this.webBrowser1.Location = new System.Drawing.Point(0, 13);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(382, 405);
            this.webBrowser1.TabIndex = 0;
            this.webBrowser1.WebBrowserShortcutsEnabled = false;
            // 
            // labelOrig
            // 
            this.labelOrig.AutoSize = true;
            this.labelOrig.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelOrig.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelOrig.Location = new System.Drawing.Point(0, 0);
            this.labelOrig.Name = "labelOrig";
            this.labelOrig.Size = new System.Drawing.Size(50, 13);
            this.labelOrig.TabIndex = 1;
            this.labelOrig.Text = "Original";
            // 
            // webBrowser2
            // 
            this.webBrowser2.AllowNavigation = false;
            this.webBrowser2.AllowWebBrowserDrop = false;
            this.webBrowser2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser2.IsWebBrowserContextMenuEnabled = false;
            this.webBrowser2.Location = new System.Drawing.Point(0, 13);
            this.webBrowser2.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser2.Name = "webBrowser2";
            this.webBrowser2.Size = new System.Drawing.Size(414, 405);
            this.webBrowser2.TabIndex = 0;
            this.webBrowser2.WebBrowserShortcutsEnabled = false;
            // 
            // labelReceived
            // 
            this.labelReceived.AutoSize = true;
            this.labelReceived.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelReceived.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelReceived.Location = new System.Drawing.Point(0, 0);
            this.labelReceived.Name = "labelReceived";
            this.labelReceived.Size = new System.Drawing.Size(61, 13);
            this.labelReceived.TabIndex = 1;
            this.labelReceived.Text = "Received";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonRepalce);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 418);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 32);
            this.panel1.TabIndex = 1;
            // 
            // buttonRepalce
            // 
            this.buttonRepalce.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonRepalce.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonRepalce.Location = new System.Drawing.Point(359, 5);
            this.buttonRepalce.Name = "buttonRepalce";
            this.buttonRepalce.Size = new System.Drawing.Size(75, 23);
            this.buttonRepalce.TabIndex = 0;
            this.buttonRepalce.Text = "Replace";
            this.toolTip1.SetToolTip(this.buttonRepalce, "Replace Original with Received");
            this.buttonRepalce.UseVisualStyleBackColor = true;
            this.buttonRepalce.Click += new System.EventHandler(this.buttonRepalce_Click);
            // 
            // Differences
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Name = "Differences";
            this.Text = "Differences";
            this.Shown += new System.EventHandler(this.Differences_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.WebBrowser webBrowser2;
        private System.Windows.Forms.Label labelOrig;
        private System.Windows.Forms.Label labelReceived;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonRepalce;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}