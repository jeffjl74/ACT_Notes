namespace ACT_Notes
{
    partial class EditCtrl
    {
        //Bstring rtbDoc="";
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditCtrl));
            this.ToolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tbrSave = new System.Windows.Forms.ToolStripButton();
            this.ToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tbrFont = new System.Windows.Forms.ToolStripButton();
            this.ToolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tbrLeft = new System.Windows.Forms.ToolStripButton();
            this.tbrCenter = new System.Windows.Forms.ToolStripButton();
            this.tbrRight = new System.Windows.Forms.ToolStripButton();
            this.ToolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tbrBullets = new System.Windows.Forms.ToolStripButton();
            this.tbrIndent = new System.Windows.Forms.ToolStripButton();
            this.tbrOutdent = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.tbrBold = new System.Windows.Forms.ToolStripButton();
            this.tbrItalic = new System.Windows.Forms.ToolStripButton();
            this.tbrUnderline = new System.Windows.Forms.ToolStripButton();
            this.tbrStrikethrough = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.tbrSpeaker = new System.Windows.Forms.ToolStripButton();
            this.tbrPopup = new System.Windows.Forms.ToolStripButton();
            this.tbrAudioVisual = new System.Windows.Forms.ToolStripButton();
            this.ToolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tbrImage = new System.Windows.Forms.ToolStripButton();
            this.OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tbrNumbered = new System.Windows.Forms.ToolStripButton();
            this.rtbDoc = new ACT_Notes.RichTextBoxEx();
            this.colorStripDropDownFontColor = new ACT_Notes.ColorToolStripDropDownButton();
            this.toolDropDownButtonBackground = new ACT_Notes.ColorToolStripDropDownButton();
            this.ToolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ToolStrip1
            // 
            this.ToolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbrSave,
            this.ToolStripSeparator1,
            this.tbrFont,
            this.colorStripDropDownFontColor,
            this.toolDropDownButtonBackground,
            this.ToolStripSeparator4,
            this.tbrLeft,
            this.tbrCenter,
            this.tbrRight,
            this.ToolStripSeparator2,
            this.tbrBullets,
            this.tbrNumbered,
            this.tbrIndent,
            this.tbrOutdent,
            this.toolStripSeparator7,
            this.tbrBold,
            this.tbrItalic,
            this.tbrUnderline,
            this.tbrStrikethrough,
            this.toolStripSeparator5,
            this.tbrSpeaker,
            this.tbrPopup,
            this.tbrAudioVisual,
            this.ToolStripSeparator3,
            this.tbrImage});
            this.ToolStrip1.Location = new System.Drawing.Point(0, 0);
            this.ToolStrip1.Name = "ToolStrip1";
            this.ToolStrip1.Size = new System.Drawing.Size(657, 25);
            this.ToolStrip1.TabIndex = 1;
            this.ToolStrip1.Text = "toolstrip1";
            // 
            // tbrSave
            // 
            this.tbrSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrSave.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbrSave.Image = ((System.Drawing.Image)(resources.GetObject("tbrSave.Image")));
            this.tbrSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrSave.Name = "tbrSave";
            this.tbrSave.Size = new System.Drawing.Size(23, 22);
            this.tbrSave.Text = "Save All Notes to disk";
            this.tbrSave.Click += new System.EventHandler(this.SaveToolStripMenuItem_Click);
            // 
            // ToolStripSeparator1
            // 
            this.ToolStripSeparator1.Name = "ToolStripSeparator1";
            this.ToolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tbrFont
            // 
            this.tbrFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tbrFont.Font = new System.Drawing.Font("Arial Rounded MT Bold", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbrFont.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.tbrFont.Image = ((System.Drawing.Image)(resources.GetObject("tbrFont.Image")));
            this.tbrFont.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrFont.Name = "tbrFont";
            this.tbrFont.Size = new System.Drawing.Size(23, 22);
            this.tbrFont.Text = "A";
            this.tbrFont.ToolTipText = "Font";
            this.tbrFont.Click += new System.EventHandler(this.SelectFontToolStripMenuItem_Click);
            // 
            // ToolStripSeparator4
            // 
            this.ToolStripSeparator4.Name = "ToolStripSeparator4";
            this.ToolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // tbrLeft
            // 
            this.tbrLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrLeft.Image = ((System.Drawing.Image)(resources.GetObject("tbrLeft.Image")));
            this.tbrLeft.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrLeft.Name = "tbrLeft";
            this.tbrLeft.Size = new System.Drawing.Size(23, 22);
            this.tbrLeft.Text = "Left";
            this.tbrLeft.ToolTipText = "Left Justify";
            this.tbrLeft.Click += new System.EventHandler(this.tbrLeft_Click);
            // 
            // tbrCenter
            // 
            this.tbrCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrCenter.Image = ((System.Drawing.Image)(resources.GetObject("tbrCenter.Image")));
            this.tbrCenter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrCenter.Name = "tbrCenter";
            this.tbrCenter.Size = new System.Drawing.Size(23, 22);
            this.tbrCenter.Text = "Center";
            this.tbrCenter.ToolTipText = "Center Justify";
            this.tbrCenter.Click += new System.EventHandler(this.tbrCenter_Click);
            // 
            // tbrRight
            // 
            this.tbrRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrRight.Image = ((System.Drawing.Image)(resources.GetObject("tbrRight.Image")));
            this.tbrRight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrRight.Name = "tbrRight";
            this.tbrRight.Size = new System.Drawing.Size(23, 22);
            this.tbrRight.Text = "Right";
            this.tbrRight.ToolTipText = "Right Justify";
            this.tbrRight.Click += new System.EventHandler(this.tbrRight_Click);
            // 
            // ToolStripSeparator2
            // 
            this.ToolStripSeparator2.Name = "ToolStripSeparator2";
            this.ToolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tbrBullets
            // 
            this.tbrBullets.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrBullets.Image = ((System.Drawing.Image)(resources.GetObject("tbrBullets.Image")));
            this.tbrBullets.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrBullets.Name = "tbrBullets";
            this.tbrBullets.Size = new System.Drawing.Size(23, 22);
            this.tbrBullets.Text = "Bullets";
            this.tbrBullets.Click += new System.EventHandler(this.tbrBullets_Click);
            // 
            // tbrIndent
            // 
            this.tbrIndent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrIndent.Image = ((System.Drawing.Image)(resources.GetObject("tbrIndent.Image")));
            this.tbrIndent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrIndent.Name = "tbrIndent";
            this.tbrIndent.Size = new System.Drawing.Size(23, 22);
            this.tbrIndent.Text = "Indent";
            this.tbrIndent.Click += new System.EventHandler(this.tbrIndent_Click);
            // 
            // tbrOutdent
            // 
            this.tbrOutdent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrOutdent.Image = ((System.Drawing.Image)(resources.GetObject("tbrOutdent.Image")));
            this.tbrOutdent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrOutdent.Name = "tbrOutdent";
            this.tbrOutdent.Size = new System.Drawing.Size(23, 22);
            this.tbrOutdent.Text = "Outdent";
            this.tbrOutdent.Click += new System.EventHandler(this.tbrOutdent_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            // 
            // tbrBold
            // 
            this.tbrBold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrBold.Image = ((System.Drawing.Image)(resources.GetObject("tbrBold.Image")));
            this.tbrBold.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrBold.Name = "tbrBold";
            this.tbrBold.Size = new System.Drawing.Size(23, 22);
            this.tbrBold.Text = "Bold";
            this.tbrBold.Click += new System.EventHandler(this.BoldToolStripMenuItem_Click);
            // 
            // tbrItalic
            // 
            this.tbrItalic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrItalic.Image = ((System.Drawing.Image)(resources.GetObject("tbrItalic.Image")));
            this.tbrItalic.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrItalic.Name = "tbrItalic";
            this.tbrItalic.Size = new System.Drawing.Size(23, 22);
            this.tbrItalic.Text = "Italic";
            this.tbrItalic.Click += new System.EventHandler(this.ItalicToolStripMenuItem_Click);
            // 
            // tbrUnderline
            // 
            this.tbrUnderline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrUnderline.Image = ((System.Drawing.Image)(resources.GetObject("tbrUnderline.Image")));
            this.tbrUnderline.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrUnderline.Name = "tbrUnderline";
            this.tbrUnderline.Size = new System.Drawing.Size(23, 22);
            this.tbrUnderline.Text = "Underline";
            this.tbrUnderline.Click += new System.EventHandler(this.UnderlineToolStripMenuItem_Click);
            // 
            // tbrStrikethrough
            // 
            this.tbrStrikethrough.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrStrikethrough.Image = ((System.Drawing.Image)(resources.GetObject("tbrStrikethrough.Image")));
            this.tbrStrikethrough.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrStrikethrough.Name = "tbrStrikethrough";
            this.tbrStrikethrough.Size = new System.Drawing.Size(23, 22);
            this.tbrStrikethrough.Text = "Strikethrough";
            this.tbrStrikethrough.ToolTipText = "Strikethrough";
            this.tbrStrikethrough.Click += new System.EventHandler(this.tbrStrikethrough_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // tbrSpeaker
            // 
            this.tbrSpeaker.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrSpeaker.Font = new System.Drawing.Font("Webdings", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.tbrSpeaker.Image = ((System.Drawing.Image)(resources.GetObject("tbrSpeaker.Image")));
            this.tbrSpeaker.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrSpeaker.Name = "tbrSpeaker";
            this.tbrSpeaker.Size = new System.Drawing.Size(23, 22);
            this.tbrSpeaker.Text = "Audio Alert";
            this.tbrSpeaker.ToolTipText = "Add or remove an audio alert for the selected text";
            this.tbrSpeaker.Click += new System.EventHandler(this.tbrSpeaker_Click);
            // 
            // tbrPopup
            // 
            this.tbrPopup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrPopup.Image = ((System.Drawing.Image)(resources.GetObject("tbrPopup.Image")));
            this.tbrPopup.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrPopup.Name = "tbrPopup";
            this.tbrPopup.Size = new System.Drawing.Size(23, 22);
            this.tbrPopup.Text = "Popup Alert";
            this.tbrPopup.ToolTipText = "Add or remove a visual alert for the selected text";
            this.tbrPopup.Click += new System.EventHandler(this.tbrPopup_Click);
            // 
            // tbrAudioVisual
            // 
            this.tbrAudioVisual.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrAudioVisual.Image = ((System.Drawing.Image)(resources.GetObject("tbrAudioVisual.Image")));
            this.tbrAudioVisual.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrAudioVisual.Name = "tbrAudioVisual";
            this.tbrAudioVisual.Size = new System.Drawing.Size(23, 22);
            this.tbrAudioVisual.Text = "Audio-Visual alert";
            this.tbrAudioVisual.ToolTipText = "Add or remove audio and visual alerts for the selected text";
            this.tbrAudioVisual.Click += new System.EventHandler(this.tbrAudioVisual_Click);
            // 
            // ToolStripSeparator3
            // 
            this.ToolStripSeparator3.Name = "ToolStripSeparator3";
            this.ToolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // tbrImage
            // 
            this.tbrImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrImage.Image = ((System.Drawing.Image)(resources.GetObject("tbrImage.Image")));
            this.tbrImage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrImage.Name = "tbrImage";
            this.tbrImage.Size = new System.Drawing.Size(23, 22);
            this.tbrImage.Text = "Insert Image from file";
            this.tbrImage.Click += new System.EventHandler(this.InsertImageToolStripMenuItem_Click);
            // 
            // OpenFileDialog1
            // 
            this.OpenFileDialog1.FileName = "OpenFileDialog1";
            // 
            // tbrNumbered
            // 
            this.tbrNumbered.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbrNumbered.Image = ((System.Drawing.Image)(resources.GetObject("tbrNumbered.Image")));
            this.tbrNumbered.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbrNumbered.Name = "tbrNumbered";
            this.tbrNumbered.Size = new System.Drawing.Size(23, 22);
            this.tbrNumbered.Text = "Numbered";
            this.tbrNumbered.ToolTipText = "Numbered List";
            this.tbrNumbered.Click += new System.EventHandler(this.tbrNumbered_Click);
            // 
            // rtbDoc
            // 
            this.rtbDoc.AcceptsTab = true;
            this.rtbDoc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbDoc.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbDoc.HideSelection = false;
            this.rtbDoc.Location = new System.Drawing.Point(0, 25);
            this.rtbDoc.Name = "rtbDoc";
            this.rtbDoc.Size = new System.Drawing.Size(657, 479);
            this.rtbDoc.TabIndex = 2;
            this.rtbDoc.Text = "";
            this.rtbDoc.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtbDoc_LinkClicked);
            this.rtbDoc.SelectionChanged += new System.EventHandler(this.rtbDoc_SelectionChanged);
            this.rtbDoc.KeyDown += new System.Windows.Forms.KeyEventHandler(this.rtbDoc_KeyDown);
            // 
            // colorStripDropDownFontColor
            // 
            this.colorStripDropDownFontColor.DefaultPalette = ACT_Notes.Palette.Saturated;
            this.colorStripDropDownFontColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.colorStripDropDownFontColor.Font = new System.Drawing.Font("Arial Rounded MT Bold", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.colorStripDropDownFontColor.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.colorStripDropDownFontColor.Image = ((System.Drawing.Image)(resources.GetObject("colorStripDropDownFontColor.Image")));
            this.colorStripDropDownFontColor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.colorStripDropDownFontColor.Name = "colorStripDropDownFontColor";
            this.colorStripDropDownFontColor.Size = new System.Drawing.Size(29, 22);
            this.colorStripDropDownFontColor.Text = "A";
            this.colorStripDropDownFontColor.ToolTipText = "Font Color";
            this.colorStripDropDownFontColor.Value = System.Drawing.Color.Black;
            this.colorStripDropDownFontColor.Click += new ACT_Notes.ColorToolStripDropDownButton.ColorPaletteEventHandler(this.colorStripDropDownFontColor_Click);
            // 
            // toolDropDownButtonBackground
            // 
            this.toolDropDownButtonBackground.DefaultPalette = ACT_Notes.Palette.Highlights;
            this.toolDropDownButtonBackground.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolDropDownButtonBackground.Image = ((System.Drawing.Image)(resources.GetObject("toolDropDownButtonBackground.Image")));
            this.toolDropDownButtonBackground.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolDropDownButtonBackground.Name = "toolDropDownButtonBackground";
            this.toolDropDownButtonBackground.Size = new System.Drawing.Size(29, 22);
            this.toolDropDownButtonBackground.Text = "Background Color";
            this.toolDropDownButtonBackground.Value = System.Drawing.Color.White;
            this.toolDropDownButtonBackground.Click += new ACT_Notes.ColorToolStripDropDownButton.ColorPaletteEventHandler(this.toolDropDownButtonBackground_Click);
            // 
            // EditCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.rtbDoc);
            this.Controls.Add(this.ToolStrip1);
            this.Name = "EditCtrl";
            this.Size = new System.Drawing.Size(657, 504);
            this.ToolStrip1.ResumeLayout(false);
            this.ToolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal System.Windows.Forms.ToolStrip ToolStrip1;
        internal System.Windows.Forms.ToolStripButton tbrSave;
        internal System.Windows.Forms.ToolStripSeparator ToolStripSeparator1;
        internal System.Windows.Forms.ToolStripButton tbrFont;
        internal System.Windows.Forms.ToolStripButton tbrLeft;
        internal System.Windows.Forms.ToolStripButton tbrCenter;
        internal System.Windows.Forms.ToolStripButton tbrRight;
        internal System.Windows.Forms.ToolStripSeparator ToolStripSeparator2;
        internal System.Windows.Forms.ToolStripButton tbrBold;
        internal System.Windows.Forms.ToolStripButton tbrItalic;
        internal System.Windows.Forms.ToolStripButton tbrUnderline;
        internal System.Windows.Forms.ToolStripSeparator ToolStripSeparator3;
        internal System.Windows.Forms.ToolStripSeparator ToolStripSeparator4;
        private System.Windows.Forms.ToolStripButton tbrIndent;
        private System.Windows.Forms.ToolStripButton tbrOutdent;
        private System.Windows.Forms.ToolStripButton tbrBullets;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripButton tbrImage;
        private System.Windows.Forms.ToolStripButton tbrStrikethrough;
        private ColorToolStripDropDownButton toolDropDownButtonBackground;
        private ColorToolStripDropDownButton colorStripDropDownFontColor;
        internal System.Windows.Forms.OpenFileDialog OpenFileDialog1;
        public RichTextBoxEx rtbDoc;
        private System.Windows.Forms.ToolStripButton tbrSpeaker;
        private System.Windows.Forms.ToolStripButton tbrPopup;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton tbrAudioVisual;
        private System.Windows.Forms.ToolStripButton tbrNumbered;
    }
}