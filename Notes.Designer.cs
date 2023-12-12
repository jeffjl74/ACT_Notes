using System.Windows.Forms;

namespace ACT_Notes
{
    partial class Notes
    {
        #region Designer Created Code (Avoid editing)

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
            this.treeViewZones = new System.Windows.Forms.TreeView();
            this.panel4 = new System.Windows.Forms.Panel();
            this.buttonAddZone = new System.Windows.Forms.Button();
            this.buttonAddMob = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonZoneFindNext = new System.Windows.Forms.Button();
            this.panel5 = new System.Windows.Forms.Panel();
            this.radioButtonAccept = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.radioButtonAsk = new System.Windows.Forms.RadioButton();
            this.radioButtonReplace = new System.Windows.Forms.RadioButton();
            this.radioButtonAppend = new System.Windows.Forms.RadioButton();
            this.buttonCompare = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.checkBoxCurrentCategory = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonFindNext = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.contextMenuStripZone = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyEntireZoneToXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportRTFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.setAlertDelaysToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.skipKillCheckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textBoxZoneFind = new ACT_Notes.TextBoxX();
            this.richEditCtrl1 = new ACT_Notes.EditCtrl();
            this.textBoxEditFind = new ACT_Notes.TextBoxX();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel2.SuspendLayout();
            this.contextMenuStripZone.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 38);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeViewZones);
            this.splitContainer1.Panel1.Controls.Add(this.panel4);
            this.splitContainer1.Panel1.Controls.Add(this.panel3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.richEditCtrl1);
            this.splitContainer1.Panel2.Controls.Add(this.panel5);
            this.splitContainer1.Panel2.Controls.Add(this.panel2);
            this.splitContainer1.Size = new System.Drawing.Size(727, 560);
            this.splitContainer1.SplitterDistance = 240;
            this.splitContainer1.TabIndex = 0;
            // 
            // treeViewZones
            // 
            this.treeViewZones.AllowDrop = true;
            this.treeViewZones.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewZones.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.treeViewZones.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeViewZones.LabelEdit = true;
            this.treeViewZones.Location = new System.Drawing.Point(0, 30);
            this.treeViewZones.Name = "treeViewZones";
            this.treeViewZones.Size = new System.Drawing.Size(240, 494);
            this.treeViewZones.TabIndex = 1;
            this.treeViewZones.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeViewZones_AfterLabelEdit);
            this.treeViewZones.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.treeViewZones_DrawNode);
            this.treeViewZones.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeViewZones_ItemDrag);
            this.treeViewZones.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewZones_BeforeSelect);
            this.treeViewZones.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewZones_AfterSelect);
            this.treeViewZones.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeViewZones_DragDrop);
            this.treeViewZones.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeViewZones_DragEnter);
            this.treeViewZones.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeViewZones_KeyDown);
            this.treeViewZones.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeViewZones_MouseDown);
            // 
            // panel4
            // 
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel4.Controls.Add(this.buttonAddZone);
            this.panel4.Controls.Add(this.buttonAddMob);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(0, 524);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(240, 36);
            this.panel4.TabIndex = 2;
            // 
            // buttonAddZone
            // 
            this.buttonAddZone.Location = new System.Drawing.Point(3, 7);
            this.buttonAddZone.Name = "buttonAddZone";
            this.buttonAddZone.Size = new System.Drawing.Size(75, 23);
            this.buttonAddZone.TabIndex = 0;
            this.buttonAddZone.Text = "Add Zone";
            this.buttonAddZone.UseVisualStyleBackColor = true;
            this.buttonAddZone.Click += new System.EventHandler(this.buttonAddZone_Click);
            // 
            // buttonAddMob
            // 
            this.buttonAddMob.Location = new System.Drawing.Point(84, 7);
            this.buttonAddMob.Name = "buttonAddMob";
            this.buttonAddMob.Size = new System.Drawing.Size(75, 23);
            this.buttonAddMob.TabIndex = 1;
            this.buttonAddMob.Text = "Add Mob";
            this.toolTip1.SetToolTip(this.buttonAddMob, "Add a mob to the currently selected zone");
            this.buttonAddMob.UseVisualStyleBackColor = true;
            this.buttonAddMob.Click += new System.EventHandler(this.buttonAddMob_Click);
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel3.Controls.Add(this.label3);
            this.panel3.Controls.Add(this.buttonZoneFindNext);
            this.panel3.Controls.Add(this.textBoxZoneFind);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(240, 30);
            this.panel3.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Find:";
            // 
            // buttonZoneFindNext
            // 
            this.buttonZoneFindNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonZoneFindNext.Enabled = false;
            this.buttonZoneFindNext.Font = new System.Drawing.Font("Webdings", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.buttonZoneFindNext.Location = new System.Drawing.Point(195, 2);
            this.buttonZoneFindNext.Name = "buttonZoneFindNext";
            this.buttonZoneFindNext.Size = new System.Drawing.Size(38, 23);
            this.buttonZoneFindNext.TabIndex = 2;
            this.buttonZoneFindNext.Text = "8";
            this.buttonZoneFindNext.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.toolTip1.SetToolTip(this.buttonZoneFindNext, "Find the next matching category");
            this.buttonZoneFindNext.UseVisualStyleBackColor = true;
            this.buttonZoneFindNext.Click += new System.EventHandler(this.buttonNodeFindNext_Click);
            // 
            // panel5
            // 
            this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel5.Controls.Add(this.radioButtonAccept);
            this.panel5.Controls.Add(this.label5);
            this.panel5.Controls.Add(this.radioButtonAsk);
            this.panel5.Controls.Add(this.radioButtonReplace);
            this.panel5.Controls.Add(this.radioButtonAppend);
            this.panel5.Controls.Add(this.buttonCompare);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel5.Location = new System.Drawing.Point(0, 524);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(483, 36);
            this.panel5.TabIndex = 2;
            // 
            // radioButtonAccept
            // 
            this.radioButtonAccept.AutoSize = true;
            this.radioButtonAccept.Location = new System.Drawing.Point(240, 10);
            this.radioButtonAccept.Name = "radioButtonAccept";
            this.radioButtonAccept.Size = new System.Drawing.Size(59, 17);
            this.radioButtonAccept.TabIndex = 4;
            this.radioButtonAccept.TabStop = true;
            this.radioButtonAccept.Text = "Accept";
            this.toolTip1.SetToolTip(this.radioButtonAccept, "Replace the existing note if the shared note is from a whitelisted player. Otherw" +
        "ise, append");
            this.radioButtonAccept.UseVisualStyleBackColor = true;
            this.radioButtonAccept.Click += new System.EventHandler(this.radioButtonAccept_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 12);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Shared:";
            // 
            // radioButtonAsk
            // 
            this.radioButtonAsk.AutoSize = true;
            this.radioButtonAsk.Location = new System.Drawing.Point(192, 10);
            this.radioButtonAsk.Name = "radioButtonAsk";
            this.radioButtonAsk.Size = new System.Drawing.Size(43, 17);
            this.radioButtonAsk.TabIndex = 3;
            this.radioButtonAsk.TabStop = true;
            this.radioButtonAsk.Text = "Ask";
            this.toolTip1.SetToolTip(this.radioButtonAsk, "Ask whether to append or overwrite a shared note");
            this.radioButtonAsk.UseVisualStyleBackColor = true;
            this.radioButtonAsk.Click += new System.EventHandler(this.radioButtonAsk_Click);
            // 
            // radioButtonReplace
            // 
            this.radioButtonReplace.AutoSize = true;
            this.radioButtonReplace.Location = new System.Drawing.Point(122, 10);
            this.radioButtonReplace.Name = "radioButtonReplace";
            this.radioButtonReplace.Size = new System.Drawing.Size(65, 17);
            this.radioButtonReplace.TabIndex = 2;
            this.radioButtonReplace.TabStop = true;
            this.radioButtonReplace.Text = "Replace";
            this.toolTip1.SetToolTip(this.radioButtonReplace, "Replace the existing note with the shared note");
            this.radioButtonReplace.UseVisualStyleBackColor = true;
            this.radioButtonReplace.Click += new System.EventHandler(this.radioButtonReplace_Click);
            // 
            // radioButtonAppend
            // 
            this.radioButtonAppend.AutoSize = true;
            this.radioButtonAppend.Location = new System.Drawing.Point(55, 10);
            this.radioButtonAppend.Name = "radioButtonAppend";
            this.radioButtonAppend.Size = new System.Drawing.Size(62, 17);
            this.radioButtonAppend.TabIndex = 1;
            this.radioButtonAppend.TabStop = true;
            this.radioButtonAppend.Text = "Append";
            this.toolTip1.SetToolTip(this.radioButtonAppend, "Append shared note to the existing note");
            this.radioButtonAppend.UseVisualStyleBackColor = true;
            this.radioButtonAppend.Click += new System.EventHandler(this.radioButtonAppend_Click);
            // 
            // buttonCompare
            // 
            this.buttonCompare.Location = new System.Drawing.Point(361, 6);
            this.buttonCompare.Name = "buttonCompare";
            this.buttonCompare.Size = new System.Drawing.Size(75, 23);
            this.buttonCompare.TabIndex = 5;
            this.buttonCompare.Text = "Compare";
            this.toolTip1.SetToolTip(this.buttonCompare, "Compare original text with shared text");
            this.buttonCompare.UseVisualStyleBackColor = true;
            this.buttonCompare.Click += new System.EventHandler(this.buttonCompare_Click);
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel2.Controls.Add(this.checkBoxCurrentCategory);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.buttonFindNext);
            this.panel2.Controls.Add(this.textBoxEditFind);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(483, 30);
            this.panel2.TabIndex = 0;
            // 
            // checkBoxCurrentCategory
            // 
            this.checkBoxCurrentCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxCurrentCategory.AutoSize = true;
            this.checkBoxCurrentCategory.Location = new System.Drawing.Point(372, 7);
            this.checkBoxCurrentCategory.Name = "checkBoxCurrentCategory";
            this.checkBoxCurrentCategory.Size = new System.Drawing.Size(59, 17);
            this.checkBoxCurrentCategory.TabIndex = 2;
            this.checkBoxCurrentCategory.Text = "current";
            this.toolTip1.SetToolTip(this.checkBoxCurrentCategory, "Search only the current note");
            this.checkBoxCurrentCategory.UseVisualStyleBackColor = true;
            this.checkBoxCurrentCategory.CheckedChanged += new System.EventHandler(this.checkBoxCurrentCategory_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(30, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Find:";
            // 
            // buttonFindNext
            // 
            this.buttonFindNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFindNext.Enabled = false;
            this.buttonFindNext.Font = new System.Drawing.Font("Webdings", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.buttonFindNext.Location = new System.Drawing.Point(437, 2);
            this.buttonFindNext.Name = "buttonFindNext";
            this.buttonFindNext.Size = new System.Drawing.Size(38, 23);
            this.buttonFindNext.TabIndex = 3;
            this.buttonFindNext.Text = "8";
            this.toolTip1.SetToolTip(this.buttonFindNext, "Find the next matching trigger");
            this.buttonFindNext.UseVisualStyleBackColor = true;
            this.buttonFindNext.Click += new System.EventHandler(this.buttonTextFindNext_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.AutomaticDelay = 750;
            // 
            // contextMenuStripZone
            // 
            this.contextMenuStripZone.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToXMLToolStripMenuItem,
            this.copyEntireZoneToXMLToolStripMenuItem,
            this.exportRTFToolStripMenuItem,
            this.toolStripSeparator1,
            this.skipKillCheckToolStripMenuItem,
            this.setAlertDelaysToolStripMenuItem,
            this.toolStripSeparator2,
            this.deleteToolStripMenuItem});
            this.contextMenuStripZone.Name = "contextMenuStrip2";
            this.contextMenuStripZone.Size = new System.Drawing.Size(216, 170);
            this.contextMenuStripZone.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripZone_Opening);
            // 
            // copyToXMLToolStripMenuItem
            // 
            this.copyToXMLToolStripMenuItem.Name = "copyToXMLToolStripMenuItem";
            this.copyToXMLToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.copyToXMLToolStripMenuItem.Text = "Copy to XML...";
            this.copyToXMLToolStripMenuItem.ToolTipText = "Open the XML share dialog";
            this.copyToXMLToolStripMenuItem.Click += new System.EventHandler(this.copyToXMLToolStripMenuItem_Click);
            // 
            // copyEntireZoneToXMLToolStripMenuItem
            // 
            this.copyEntireZoneToXMLToolStripMenuItem.Name = "copyEntireZoneToXMLToolStripMenuItem";
            this.copyEntireZoneToXMLToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.copyEntireZoneToXMLToolStripMenuItem.Text = "Copy Entire Zone to XML...";
            this.copyEntireZoneToXMLToolStripMenuItem.Click += new System.EventHandler(this.copyEntireZoneToXMLToolStripMenuItem_Click);
            // 
            // exportRTFToolStripMenuItem
            // 
            this.exportRTFToolStripMenuItem.Name = "exportRTFToolStripMenuItem";
            this.exportRTFToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.exportRTFToolStripMenuItem.Text = "Export RTF...";
            this.exportRTFToolStripMenuItem.ToolTipText = "Export to a WordPad file";
            this.exportRTFToolStripMenuItem.Click += new System.EventHandler(this.exportRTFToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(212, 6);
            // 
            // setAlertDelaysToolStripMenuItem
            // 
            this.setAlertDelaysToolStripMenuItem.Name = "setAlertDelaysToolStripMenuItem";
            this.setAlertDelaysToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.setAlertDelaysToolStripMenuItem.Text = "Set Alert Delays...";
            this.setAlertDelaysToolStripMenuItem.Click += new System.EventHandler(this.setAlertDelaysToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(212, 6);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.ToolTipText = "Delete the zone or mob";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(5, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(473, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "If a zone name matches the in-game zone name, that item will be automatically sel" +
    "ected on zone-in.";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.linkLabel1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(727, 38);
            this.panel1.TabIndex = 0;
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(690, 3);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(29, 13);
            this.linkLabel1.TabIndex = 2;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Help";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHelp_LinkClicked);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(169, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Right-click a tree node for options.";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "rtf";
            this.saveFileDialog1.Filter = "WordPad files|*.rtf|All files|*.*";
            // 
            // skipKillCheckToolStripMenuItem
            // 
            this.skipKillCheckToolStripMenuItem.Name = "skipKillCheckToolStripMenuItem";
            this.skipKillCheckToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.skipKillCheckToolStripMenuItem.Text = "Skip Kill Check";
            this.skipKillCheckToolStripMenuItem.Click += new System.EventHandler(this.skipKillCheckToolStripMenuItem_Click);
            // 
            // textBoxZoneFind
            // 
            this.textBoxZoneFind.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxZoneFind.ButtonTextClear = true;
            this.textBoxZoneFind.Location = new System.Drawing.Point(40, 4);
            this.textBoxZoneFind.Name = "textBoxZoneFind";
            this.textBoxZoneFind.Size = new System.Drawing.Size(148, 20);
            this.textBoxZoneFind.TabIndex = 1;
            this.toolTip1.SetToolTip(this.textBoxZoneFind, "Incremental search in the category name");
            this.textBoxZoneFind.TextChanged += new System.EventHandler(this.textBoxZoneFind_TextChanged);
            this.textBoxZoneFind.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxZoneFind_KeyDown);
            // 
            // richEditCtrl1
            // 
            this.richEditCtrl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richEditCtrl1.Location = new System.Drawing.Point(0, 30);
            this.richEditCtrl1.Name = "richEditCtrl1";
            this.richEditCtrl1.Size = new System.Drawing.Size(483, 494);
            this.richEditCtrl1.TabIndex = 1;
            // 
            // textBoxEditFind
            // 
            this.textBoxEditFind.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxEditFind.ButtonTextClear = true;
            this.textBoxEditFind.Location = new System.Drawing.Point(40, 4);
            this.textBoxEditFind.Name = "textBoxEditFind";
            this.textBoxEditFind.Size = new System.Drawing.Size(326, 20);
            this.textBoxEditFind.TabIndex = 1;
            this.toolTip1.SetToolTip(this.textBoxEditFind, "Incremental search for text in the trigger\'s regular expression, alert, or timer " +
        "name");
            this.textBoxEditFind.TextChanged += new System.EventHandler(this.textBoxTextFind_TextChanged);
            this.textBoxEditFind.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxEditFind_KeyDown);
            // 
            // Notes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Name = "Notes";
            this.Size = new System.Drawing.Size(727, 598);
            this.VisibleChanged += new System.EventHandler(this.Plugin_VisibleChanged);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.contextMenuStripZone.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

		}

        #endregion

        private Label label3;
        private Label label4;
        private SplitContainer splitContainer1;
        private TreeView treeViewZones;
        private ToolTip toolTip1;
        private ContextMenuStrip contextMenuStripZone;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private Label label1;
        private Panel panel1;
        private Button buttonFindNext;
        private TextBoxX textBoxEditFind;
        private Panel panel2;
        private Panel panel3;
        private TextBoxX textBoxZoneFind;
        private Button buttonZoneFindNext;
        private Label label2;

        #endregion

        private EditCtrl richEditCtrl1;
        private Button buttonAddMob;
        private Button buttonAddZone;
        private Panel panel4;
        private ToolStripMenuItem copyToXMLToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private CheckBox checkBoxCurrentCategory;
        private LinkLabel linkLabel1;
        private Panel panel5;
        private Button buttonCompare;
        private Label label5;
        private RadioButton radioButtonAsk;
        private RadioButton radioButtonReplace;
        private RadioButton radioButtonAppend;
        private RadioButton radioButtonAccept;
        private ToolStripMenuItem exportRTFToolStripMenuItem;
        private SaveFileDialog saveFileDialog1;
        private ToolStripMenuItem setAlertDelaysToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem copyEntireZoneToXMLToolStripMenuItem;
        private ToolStripMenuItem skipKillCheckToolStripMenuItem;
    }
}