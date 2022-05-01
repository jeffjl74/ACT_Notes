using Advanced_Combat_Tracker;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Linq;
using System.Threading;

[assembly: AssemblyTitle("Notes for zone mobs")]
[assembly: AssemblyDescription("Organize notes for mobs")]
[assembly: AssemblyCompany("Mineeme of Maj'Dul")]
[assembly: AssemblyVersion("1.0.0.0")]

namespace ACT_Notes
{
    public partial class Notes : UserControl, IActPluginV1
	{
        const string helpUlr = "https://github.com/jeffjl74/ACT_Notes#act-notes-plugin";

        // the data
        ZoneList zoneList = new ZoneList();
        XmlSerializer xmlSerializer;

        // zone change support
        System.Timers.Timer zoneTimer = new System.Timers.Timer();
        string currentZone = string.Empty;
        //tree enemies for the selected zone
        List<string> enemies = new List<string>();

        WindowsFormsSynchronizationContext mUiContext = new WindowsFormsSynchronizationContext();
        
        // tree node search
        List<TreeNode> foundNodes = new List<TreeNode>();
        int foundNodesIndex = 0;
        int foundZoneIndex = 0;

        // text find in zone notes and mob notes
        bool foundZoneNeedMobs = false;
        int foundMobIndex = 0;
        Color defaultBackground = Color.White;      //background default for the rich text box
        Color foundBackground = Color.Yellow;       //background for a found word

        // new node support
        const string newMob = "New Mob";
        const string newZone = "New Zone";
        string autoNodeName = string.Empty;         //when selecting from the ACT main tab

        // xml share
        static public string XmlSnippetType = "Note";
        static string pastePrefix = "--------- received ";
        List<string> whitelist = new List<string>();
        string xmlSendingPlayer = string.Empty;
        Zone xmlZone;
        int xmlNumSections;
        int xmlCurrentSection;
        string[] xmlSections;
        bool[] packetTrack;

        ImageList treeImages = new ImageList();     //tree folder images

        // context menu support
        TreeNode clickedZoneNode = null;
        Point clickedZonePoint;

        bool neverBeenVisible = true;               //save the splitter location only if it has been initialized 

        Label lblStatus;                            // The status label that appears in ACT's Plugin tab

        string settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\Notes.config.xml");


        public Notes()
		{
            InitializeComponent();
		}


        #region IActPluginV1 Members

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
		{
			lblStatus = pluginStatusText;	            // Hand the status label's reference to our local var
			pluginScreenSpace.Controls.Add(this);	    // Add this UserControl to the tab ACT provides
			this.Dock = DockStyle.Fill;	                // Expand the UserControl to fill the tab's client space

            xmlSerializer = new XmlSerializer(typeof(ZoneList));

            treeImages.Images.Add(GetFolderBitmap());
            treeImages.Images.Add(GetOpenFolderBitmap());
            treeViewZones.ImageList = treeImages;
            treeViewZones.TreeViewNodeSorter = new TreeNodeSorter();

            LoadSettings();

            richEditCtrl1.OnSave += RichEditCtrl1_OnSave;

            // check for a zone change once a second
            zoneTimer.Interval = 1000;
            zoneTimer.Enabled = true;
            zoneTimer.AutoReset = true;
            zoneTimer.SynchronizingObject = this;
            zoneTimer.Elapsed += ZoneTimer_Elapsed;
            zoneTimer.Start();

            ActGlobals.oFormActMain.XmlSnippetAdded += OFormActMain_XmlSnippetAdded;    //for incoming shared note
            ActGlobals.oFormActMain.OnCombatEnd += OFormActMain_OnCombatEnd;            //for tracking kills

            if (ActGlobals.oFormActMain.GetAutomaticUpdatesAllowed())
            {
                // If ACT is set to automatically check for updates, check for updates to the plugin
                // If we don't put this on a separate thread, web latency will delay the plugin init phase
                new Thread(new ThreadStart(oFormActMain_UpdateCheckClicked)).Start();
            }

            lblStatus.Text = "Plugin Started";
		}

        public void DeInitPlugin()
		{
			// Unsubscribe from any events you listen to when exiting!
            ActGlobals.oFormActMain.XmlSnippetAdded -= OFormActMain_XmlSnippetAdded;
            ActGlobals.oFormActMain.OnCombatEnd += OFormActMain_OnCombatEnd;

            SaveSettings();
			lblStatus.Text = "Plugin Exited";
		}

        #endregion IActPluginV1 Members

        #region ACT Interaction

        private void OFormActMain_OnCombatEnd(bool isImport, CombatToggleEventArgs encounterInfo)
        {
            // if we won, see if the enemy is in our list so we can move to the next enemy
            if (encounterInfo.encounter.GetEncounterSuccessLevel() == 1)
            {
                string enemy = encounterInfo.encounter.GetStrongestEnemy(ActGlobals.charName);
                if (enemies.Contains(enemy))
                    mUiContext.Post(KillProc, enemy);
            }
        }

        private void ZoneTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //look for zone change
            if (currentZone != ActGlobals.oFormActMain.CurrentZone)
            {
                currentZone = ActGlobals.oFormActMain.CurrentZone;
                TreeNode node = BuildEnemyList();
                if (node != null)
                {
                    treeViewZones.SelectedNode = BuildEnemyList();
                    treeViewZones.SelectedNode.EnsureVisible();
                }
            }
        }

        private TreeNode BuildEnemyList()
        {
            TreeNode result = null;
            TreeNode[] nodes = treeViewZones.Nodes.Find(currentZone, false);
            if (nodes.Length > 0)
            {
                TreeNode zoneNode = nodes[0];
                Zone zone = zoneNode.Tag as Zone;
                TreeNode mobNode = null;
                if (zoneNode.Nodes.Count > 0)
                {
                    mobNode = zoneNode.Nodes[0];
                    if (zone != null)
                    {
                        foreach (Mob m in zone.Mobs)
                        {
                            string[] names = m.MobName.Split(',');
                            foreach (string name in names)
                            {
                                enemies.Add(name.Trim());
                            }
                        }
                    }
                }
                if (mobNode == null || zone.Notes != null)
                    result = zoneNode;
                else
                    result = mobNode;
            }
            return result;
        }

        void oFormActMain_UpdateCheckClicked()
        {
            int pluginId = 89;

            try
            {
                Version localVersion = this.GetType().Assembly.GetName().Version;
                // Strip any leading 'v' from the string before passing to the Version constructor
                Version remoteVersion = new Version(ActGlobals.oFormActMain.PluginGetRemoteVersion(pluginId).TrimStart(new char[] { 'v' }));
                if (remoteVersion > localVersion)
                {
                    Rectangle screen = Screen.GetWorkingArea(ActGlobals.oFormActMain);
                    DialogResult result = SimpleMessageBox.Show(new Point(screen.Width/2 - 100, screen.Height/2 - 100),
                          @"There is an update for the Notes plugin."
                        + @"\line Update it now?"
                        + @"\line (If there is an update to ACT"
                        + @"\line you should click No and update ACT first.)"
                        + @"\line\line Release notes at project website:"
                        + @"{\line\ql " + helpUlr + "}"
                        , "Notes New Version", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        FileInfo updatedFile = ActGlobals.oFormActMain.PluginDownload(pluginId);
                        ActPluginData pluginData = ActGlobals.oFormActMain.PluginGetSelfData(this);
                        pluginData.pluginFile.Delete();
                        updatedFile.MoveTo(pluginData.pluginFile.FullName);
                        Application.DoEvents();
                        ThreadInvokes.CheckboxSetChecked(ActGlobals.oFormActMain, pluginData.cbEnabled, false);
                        Application.DoEvents();
                        ThreadInvokes.CheckboxSetChecked(ActGlobals.oFormActMain, pluginData.cbEnabled, true);
                    }
                }
            }
            catch (Exception ex)
            {
                ActGlobals.oFormActMain.WriteExceptionLog(ex, "Notes Plugin Update Download failed");
            }
        }

        private void OFormActMain_XmlSnippetAdded(object sender, XmlSnippetEventArgs e)
        {
            if (e.ShareType == XmlSnippetType)
            {
                string sections;
                if (e.XmlAttributes.TryGetValue("S", out sections))
                {
                    string[] counts = sections.Split('/');
                    if (counts.Length == 2)
                    {
                        int.TryParse(counts[0], out xmlCurrentSection);
                        int.TryParse(counts[1], out xmlNumSections);

                        string zone = "";
                        if (e.XmlAttributes.TryGetValue("Z", out zone))
                        {
                            if (xmlZone == null)
                            {
                                xmlZone = new Zone();
                                xmlSections = new string[xmlNumSections];
                                packetTrack = new bool[xmlNumSections];
                            }
                            xmlZone.ZoneName = XmlCopyForm.DecodeShare(zone);

                            string data;
                            e.XmlAttributes.TryGetValue("N", out data);
                            xmlSections[0] = data;
                            packetTrack[0] = true;

                            string mobName;
                            if (e.XmlAttributes.TryGetValue("M", out mobName))
                            {
                                Mob mob = new Mob { MobName = XmlCopyForm.DecodeShare(mobName) };
                                xmlZone.Mobs.Add(mob);
                            }

                            e.XmlAttributes.TryGetValue("P", out xmlSendingPlayer);

                            // should only end up pasting at this point if both are 1
                            if (!packetTrack.Contains(false))
                                mUiContext.Post(XmlPasteProc, null);
                        }
                        else
                        {
                            // no Z means this is a continuation section
                            // but need to have a zone instance if we have not seen section 1
                            if (xmlZone == null)
                            {
                                xmlZone = new Zone();
                                xmlSections = new string[xmlNumSections];
                                packetTrack = new bool[xmlNumSections];
                            }
                            if (xmlZone != null)
                            {
                                string data;
                                e.XmlAttributes.TryGetValue("N", out data);

                                if (xmlSections.Length > xmlCurrentSection - 1)
                                {
                                    xmlSections[xmlCurrentSection - 1] = data;
                                    packetTrack[xmlCurrentSection - 1] = true;
                                }

                                // process the note if we have all the sections
                                if (!packetTrack.Contains(false))
                                    mUiContext.Post(XmlPasteProc, null);
                            }
                        } // was a continuation section
                    } // found both section numbers
                } // found section ID

                e.Handled = true;
            }
        }

        // on the UI thread
        private void XmlPasteProc(object o)
        {
            string data_enc = string.Join("", xmlSections);
            // incoming data has substitutions to work around EQII and ACT expectations
            string data = XmlCopyForm.DecodeShare(data_enc);

            Zone zone = zoneList.Zones.Find(z => z.ZoneName == xmlZone.ZoneName);
            if (zone == null)
            {
                // new zone/mob
                if (xmlZone.Mobs.Count == 0)
                {
                    // no mob, just zone notes
                    xmlZone.Notes = data;
                }
                else
                {
                    xmlZone.Mobs[0].Notes = data;
                    xmlZone.Mobs[0].KillOrder = 0;
                }

                zoneList.Zones.Add(xmlZone);
                TreeNode zn = treeViewZones.Nodes.Add(xmlZone.ZoneName);
                zn.Tag = xmlZone;
                zn.Name = xmlZone.ZoneName;
                if (xmlZone.Mobs.Count > 0)
                {
                    TreeNode mn = zn.Nodes.Add(xmlZone.Mobs[0].MobName);
                    mn.Tag = xmlZone.Mobs[0];
                    mn.Name = xmlZone.Mobs[0].MobName;
                    mn.EnsureVisible();
                    treeViewZones.SelectedNode = mn;
                }
                else
                {
                    zn.EnsureVisible();
                    treeViewZones.SelectedNode = zn;
                }
            }
            else
            {
                // existing zone
                GetWhiteList();

                Zone.PasteType mergeOp = zone.Paste;

                if (xmlZone.Mobs.Count > 0)
                {
                    // existing mob note?
                    Mob mob = zone.Mobs.Find(m => m.MobName == xmlZone.Mobs[0].MobName);
                    if (mob != null)
                    {
                        //
                        // existing zone, existing mob
                        //
                        TreeNode destMobNode = null;
                        TreeNode[] zoneNodes = treeViewZones.Nodes.Find(xmlZone.ZoneName, false);
                        if (zoneNodes.Length > 0)
                        {
                            TreeNode[] mobNodes = zoneNodes[0].Nodes.Find(mob.MobName, false);
                            if (mobNodes.Length > 0)
                            {
                                destMobNode = mobNodes[0];
                                if (treeViewZones.SelectedNode == destMobNode && richEditCtrl1.rtbDoc.Modified)
                                    SaveNote(destMobNode, false);
                            }
                        }

                        if (mergeOp == Zone.PasteType.Ask)
                            mergeOp = AskPasteType(mob.MobName);
                        if (mergeOp == Zone.PasteType.Ignore)
                            return;
                        if (mergeOp == Zone.PasteType.Replace ||
                           (mergeOp == Zone.PasteType.Accept && whitelist.Contains(xmlSendingPlayer)))
                        {
                            mob.Notes = data;
                        }
                        else // Append
                        {
                            // use a RichTextBox to merge RTF docs
                            RichTextBox rtb = new RichTextBox();
                            rtb.Rtf = mob.Notes;

                            // delimiter between the original and the incoming
                            AppendDelimiter(rtb);

                            rtb.Select(rtb.TextLength, 0);
                            rtb.SelectedRtf = data;
                            mob.Notes = rtb.Rtf;
                        }

                        if(destMobNode != null)
                        {
                            if (treeViewZones.SelectedNode == destMobNode)
                                treeViewZones.SelectedNode = null; //force an update
                            destMobNode.EnsureVisible();
                            treeViewZones.SelectedNode = destMobNode;
                        }
                    }
                    else
                    {
                        //
                        // new mob in existing zone
                        //
                        xmlZone.Mobs[0].Notes = data;
                        xmlZone.Mobs[0].KillOrder = zone.Mobs.Count;
                        zone.Mobs.Add(xmlZone.Mobs[0]);
                        TreeNode[] nodes = treeViewZones.Nodes.Find(xmlZone.ZoneName, false);
                        if (nodes.Length > 0)
                        {
                            TreeNode mn = nodes[0].Nodes.Add(xmlZone.Mobs[0].MobName);
                            mn.Tag = xmlZone.Mobs[0];
                            mn.Name = xmlZone.Mobs[0].MobName;
                            mn.EnsureVisible();
                            treeViewZones.SelectedNode = mn;
                        }
                    }
                }
                else
                {
                    //
                    // zone note for existing zone
                    //
                    TreeNode destZoneNode = null;
                    TreeNode[] nodes = treeViewZones.Nodes.Find(zone.ZoneName, false);
                    if (nodes.Length > 0)
                    {
                        destZoneNode = nodes[0];
                        if (treeViewZones.SelectedNode == destZoneNode && richEditCtrl1.rtbDoc.Modified)
                            SaveNote(destZoneNode, false);
                    }

                    if (mergeOp == Zone.PasteType.Ask)
                        mergeOp = AskPasteType(zone.ZoneName);
                    if (mergeOp == Zone.PasteType.Ignore)
                        return;
                    if (mergeOp == Zone.PasteType.Replace ||
                       (mergeOp == Zone.PasteType.Accept && whitelist.Contains(xmlSendingPlayer)))
                    {
                        zone.Notes = data;
                    }
                    else // append
                    {
                        // use a RichTextBox to merge RTF docs
                        RichTextBox rtb = new RichTextBox();
                        rtb.Rtf = zone.Notes;

                        // delimiter
                        AppendDelimiter(rtb);

                        rtb.Select(rtb.TextLength, 0);
                        rtb.SelectedRtf = data;
                        zone.Notes = rtb.Rtf;
                    }

                    if(destZoneNode != null)
                    {
                        if (treeViewZones.SelectedNode == destZoneNode)
                            treeViewZones.SelectedNode = null; //force an update
                        destZoneNode.EnsureVisible();
                        treeViewZones.SelectedNode = destZoneNode;
                    }
                }
            }
            xmlZone = null;
        }

        private Zone.PasteType AskPasteType(string what)
        {
            Zone.PasteType ret = Zone.PasteType.Append;
            DialogResult result = new PasteOption(ActGlobals.oFormActMain, what).ShowDialog(ActGlobals.oFormActMain);
            switch (result)
            {
                case DialogResult.Yes:
                    ret = Zone.PasteType.Append;
                    break;
                case DialogResult.No:
                    ret = Zone.PasteType.Replace;
                    break;
                case DialogResult.Ignore:
                    ret = Zone.PasteType.Ignore;
                    break;
            }
            return ret;
        }

        private void AppendDelimiter(RichTextBox rtb)
        {
            rtb.Select(rtb.TextLength, 0);
            string who = string.Empty;
            if(!string.IsNullOrEmpty(xmlSendingPlayer))
                who = $" from {xmlSendingPlayer}";
            string stats = $"{pastePrefix}{who} {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}";
            rtb.SelectedRtf = @"{\rtf1\ansi\par\pard\b " + stats + @" ------------\b0\par}";
        }

        private void GetWhiteList()
        {
            whitelist = new List<string>();
            // Dive into the options tree to get the XML share whitelist
            // (Note: This is pretty delicate. Could not find an API way to get this list)
            List<Control> option;
            if (ActGlobals.oFormActMain.OptionsControlSets.TryGetValue(@"Configuration Import/Export\XML Share Snippets", out option))
            {
                try
                {
                    Control[] clb = option[0].Controls.Find("clbShareTrusted", true);
                    if (clb.Length > 0)
                    {
                        CheckedListBox.CheckedItemCollection players = ((CheckedListBox)clb[0]).CheckedItems;
                        foreach (var item in players)
                        {
                            whitelist.Add(item.ToString());
                        }
                    }
                }
                catch { } //just give up if something changed in the options tree
            }
            if(whitelist.Count > 0)
                radioButtonAccept.Visible = true;
            else
                radioButtonAccept.Visible = false;
        }

        #endregion ACT Interaction

        void LoadSettings()
		{

            if (File.Exists(settingsFile))
			{
				try
				{
                    using (FileStream fs = new FileStream(settingsFile, FileMode.Open))
                    {
                        zoneList = (ZoneList)xmlSerializer.Deserialize(fs);
                        zoneList.Zones.Sort();
                    }
                    foreach(Zone zone in zoneList.Zones)
                    {
                        zone.Mobs.Sort();
                    }
                    PopulateZonesTree();
                }
				catch (Exception ex)
				{
					lblStatus.Text = "Error loading settings: " + ex.Message;
				}
            }
        }

        void SaveSettings()
		{
            // save any current edits to its class
            if (richEditCtrl1.rtbDoc.Modified)
            {
                if (treeViewZones.SelectedNode != null)
                {
                    SaveNote(treeViewZones.SelectedNode, false);
                }
            }

            //store the splitter location
            // but only save it if it was ever set
            if (!neverBeenVisible)
            {
                zoneList.SplitterLoc = splitContainer1.SplitterDistance;
            }

            using(TextWriter writer = new StreamWriter(settingsFile))
            {
                xmlSerializer.Serialize(writer, zoneList);
                writer.Close();
            }
        }

        private void linkLabelHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                VisitLink();
            }
            catch (Exception ex)
            {
                SimpleMessageBox.Show(ActGlobals.oFormActMain, ex.Message, "Unable to open the link.");
            }
        }

        #region Tree Panel

        void PopulateZonesTree()
        {
            //try to save the current selection
            string prevZone = string.Empty;
            TreeNode sel = treeViewZones.SelectedNode;
            if (sel != null)
            {
                prevZone = sel.Text;
            }

            treeViewZones.Nodes.Clear();

            foreach(Zone zone in zoneList.Zones)
            {
                TreeNode znode = new TreeNode(zone.ZoneName);
                znode.Name = zone.ZoneName; // so .Find() will work
                znode.Tag = zone;
                treeViewZones.Nodes.Add(znode);
                foreach(Mob mob in zone.Mobs)
                {
                    TreeNode mnode = new TreeNode(mob.MobName);
                    mnode.Name = mob.MobName;
                    mnode.Tag = mob;
                    znode.Nodes.Add(mnode);
                }
            }

            treeViewZones.Sort();


            //try to restore previous selection
            if (!string.IsNullOrEmpty(prevZone))
            {
                TreeNode[] nodes = treeViewZones.Nodes.Find(prevZone, false);
                if (nodes.Length > 0)
                {
                    treeViewZones.SelectedNode = nodes[0];
                    treeViewZones.SelectedNode.EnsureVisible();
                }
            }
            else
            {
                if(treeViewZones.Nodes.Count > 0)
                    treeViewZones.SelectedNode = treeViewZones.Nodes[0];
            }
        }

        private void treeViewZones_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node == null) return;

            // if treeview's HideSelection property is "True", 
            // this will always returns "False" on unfocused treeview
            var selected = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;
            var unfocused = !e.Node.TreeView.Focused;

            // keep the focused highlight if selected and unfocused
            // otherwise, default colors
            if (selected && unfocused)
            {
                var font = e.Node.NodeFont ?? e.Node.TreeView.Font;
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, SystemColors.HighlightText, TextFormatFlags.GlyphOverhangPadding);
            }
            else
            {
                e.DrawDefault = true;
            }
        }

        private Bitmap GetFolderBitmap()
        {
            //use https://littlevgl.com/image-to-c-array to convert Visual Studio Image Library to C code
            byte[] png_data = new byte[] {
                  0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, 0x00, 0x00, 0x00, 0x0d, 0x49, 0x48, 0x44, 0x52,
                  0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x10, 0x08, 0x06, 0x00, 0x00, 0x00, 0x1f, 0xf3, 0xff,
                  0x61, 0x00, 0x00, 0x00, 0x09, 0x70, 0x48, 0x59, 0x73, 0x00, 0x00, 0x0e, 0xc4, 0x00, 0x00, 0x0e,
                  0xc4, 0x01, 0x95, 0x2b, 0x0e, 0x1b, 0x00, 0x00, 0x00, 0xa3, 0x49, 0x44, 0x41, 0x54, 0x78, 0x5e,
                  0xed, 0xd3, 0xad, 0x0a, 0x42, 0x41, 0x10, 0x05, 0xe0, 0x73, 0x75, 0x82, 0x4c, 0xf1, 0x06, 0x83,
                  0xd8, 0x6c, 0x26, 0x59, 0xa3, 0xcd, 0x27, 0xf0, 0x59, 0x6c, 0x16, 0xe1, 0x1a, 0x2c, 0x66, 0x1f,
                  0xc6, 0x6a, 0xd3, 0xe6, 0xbe, 0x80, 0xc5, 0x20, 0x08, 0x06, 0x7f, 0x60, 0x11, 0x44, 0xd6, 0x83,
                  0x20, 0x8b, 0x22, 0x0b, 0x72, 0x83, 0x20, 0x7e, 0x70, 0x18, 0xa6, 0xcc, 0xec, 0xc2, 0x6e, 0xe2,
                  0xbd, 0x47, 0x1e, 0x89, 0x73, 0x6e, 0xc9, 0x6a, 0x10, 0xb7, 0x67, 0xea, 0xaa, 0xca, 0x1a, 0x70,
                  0x39, 0xe4, 0x7a, 0x3e, 0x98, 0xed, 0x62, 0x82, 0x98, 0xb4, 0xd1, 0x4d, 0xb5, 0xda, 0xec, 0x01,
                  0x18, 0xe2, 0x85, 0x14, 0x4b, 0x65, 0x10, 0x2a, 0xed, 0x3e, 0xde, 0xb9, 0x1c, 0xd7, 0x38, 0xad,
                  0xa6, 0xe0, 0x80, 0x8c, 0xa7, 0xcd, 0x10, 0x58, 0xa6, 0x25, 0x8f, 0x6e, 0x37, 0x1f, 0x23, 0x66,
                  0x33, 0x1b, 0x3d, 0xf5, 0xb5, 0xce, 0xc0, 0x80, 0x0a, 0xc8, 0xe1, 0x67, 0x06, 0xfc, 0x07, 0x08,
                  0x63, 0xef, 0x8f, 0xe2, 0x73, 0x16, 0x14, 0x7e, 0xe3, 0xb7, 0xae, 0x70, 0x03, 0xb8, 0x17, 0x29,
                  0xc4, 0xad, 0x11, 0x0a, 0x52, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4e, 0x44, 0xae, 0x42, 0x60,
                  0x82,
            };

            Bitmap bmp;
            using (var ms = new MemoryStream(png_data))
            {
                bmp = new Bitmap(ms);
            }
            return bmp;
        }

        private Bitmap GetOpenFolderBitmap()
        {
            byte[] png_data = new byte[] {
                   0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, 0x00, 0x00, 0x00, 0x0d, 0x49, 0x48, 0x44, 0x52,
                  0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x10, 0x08, 0x06, 0x00, 0x00, 0x00, 0x1f, 0xf3, 0xff,
                  0x61, 0x00, 0x00, 0x00, 0x09, 0x70, 0x48, 0x59, 0x73, 0x00, 0x00, 0x0e, 0xc4, 0x00, 0x00, 0x0e,
                  0xc4, 0x01, 0x95, 0x2b, 0x0e, 0x1b, 0x00, 0x00, 0x01, 0x0b, 0x49, 0x44, 0x41, 0x54, 0x78, 0x5e,
                  0xa5, 0xd2, 0xb1, 0x4a, 0xc4, 0x40, 0x14, 0x86, 0xd1, 0x2f, 0x63, 0x84, 0x10, 0x15, 0x2c, 0x84,
                  0xdd, 0xc2, 0x62, 0x3b, 0x3b, 0x4b, 0xb5, 0x10, 0xf1, 0x15, 0x7c, 0x83, 0xdd, 0xde, 0xd2, 0xca,
                  0x46, 0x04, 0x2b, 0xb1, 0xb1, 0xb6, 0xd8, 0xf8, 0x0a, 0x16, 0x96, 0xba, 0x85, 0x85, 0x82, 0x85,
                  0x5a, 0x59, 0x88, 0xae, 0x20, 0x1a, 0x50, 0x24, 0xa2, 0x64, 0x57, 0xd8, 0xdd, 0xf1, 0x26, 0x38,
                  0xc1, 0x85, 0xd1, 0x44, 0x72, 0xe0, 0x27, 0x45, 0x98, 0x3b, 0xf7, 0xce, 0x8c, 0x83, 0x88, 0xe3,
                  0xb8, 0x06, 0x48, 0x7e, 0x75, 0xe1, 0xfb, 0x7e, 0x84, 0x85, 0x2b, 0x8b, 0x9b, 0x40, 0x9d, 0xbf,
                  0x05, 0x40, 0x03, 0x0b, 0xe7, 0xfd, 0xf5, 0x49, 0x3f, 0x9f, 0xef, 0x31, 0xe8, 0x75, 0xb1, 0x19,
                  0xf1, 0x26, 0xa9, 0x2c, 0xac, 0x22, 0x56, 0x00, 0xd3, 0x45, 0x5b, 0x3a, 0x6a, 0xf3, 0xdd, 0xbe,
                  0xbe, 0x39, 0x5c, 0xd7, 0xf2, 0xb5, 0xe6, 0xee, 0x68, 0x5b, 0xbf, 0xdc, 0x9e, 0xda, 0xfe, 0x35,
                  0x11, 0x2e, 0x39, 0x26, 0x6a, 0x4b, 0x44, 0xd7, 0x07, 0x69, 0x0c, 0xe5, 0x7a, 0x54, 0x17, 0xd7,
                  0xea, 0x40, 0x23, 0xb7, 0x80, 0x5f, 0x9d, 0x4d, 0xf3, 0xd3, 0x63, 0x6b, 0x0b, 0x43, 0x51, 0x82,
                  0x8c, 0xb1, 0xac, 0x28, 0xe7, 0xd8, 0x1d, 0xaa, 0x18, 0x5e, 0x99, 0x59, 0x73, 0x85, 0x27, 0x3b,
                  0xc9, 0x39, 0x0c, 0x8f, 0xd0, 0x09, 0x2f, 0x29, 0xca, 0x9b, 0x9a, 0x41, 0x04, 0x59, 0x81, 0x7e,
                  0xf7, 0x8d, 0xcf, 0xe8, 0x9e, 0xa2, 0xc6, 0xa6, 0xe7, 0x10, 0xfb, 0x59, 0x81, 0x8f, 0x87, 0x33,
                  0x8a, 0x1a, 0x1d, 0xaf, 0x24, 0x49, 0x1e, 0x53, 0x4b, 0x99, 0x7b, 0xed, 0xc8, 0xfc, 0xc5, 0x77,
                  0x9f, 0x47, 0xec, 0x9a, 0x87, 0xb4, 0x29, 0x87, 0xb1, 0xc1, 0xff, 0x05, 0x08, 0x47, 0x6b, 0x4d,
                  0x19, 0x8a, 0x92, 0xbe, 0x00, 0xfd, 0xab, 0x7c, 0xc4, 0x09, 0xdb, 0x7d, 0x59, 0x00, 0x00, 0x00,
                  0x00, 0x49, 0x45, 0x4e, 0x44, 0xae, 0x42, 0x60, 0x82,
              };

            Bitmap bmp;
            using (var ms = new MemoryStream(png_data))
            {
                bmp = new Bitmap(ms);
            }
            return bmp;
        }

        private void textBoxZoneFind_TextChanged(object sender, EventArgs e)
        {
            // remove any highlight from the current RTB before we move on
            UnHighlight();

            if (string.IsNullOrEmpty(textBoxZoneFind.Text))
            {
                buttonZoneFindNext.Enabled = false;
            }
            else
            {
                buttonZoneFindNext.Enabled = true;
                string finding = textBoxZoneFind.Text.ToLower();
                foundNodes = treeViewZones.FlattenTree().Where(n => n.Text.ToLower().Contains(finding)).ToList();
                if (foundNodes.Count > 0)
                {
                    foundNodesIndex = 0;
                    treeViewZones.SelectedNode = foundNodes[foundNodesIndex];
                    treeViewZones.SelectedNode.EnsureVisible();
                }
                else
                    SimpleMessageBox.Show(ActGlobals.oFormActMain, "Not found", "Find");
            }
        }

        private void FindNextNode()
        {
            // remove any highlight from the current RTB before we move on
            UnHighlight();

            if (foundNodesIndex < foundNodes.Count - 1)
            {
                foundNodesIndex++;
                treeViewZones.SelectedNode = foundNodes[foundNodesIndex];
                treeViewZones.SelectedNode.EnsureVisible();
            }
            else
                SimpleMessageBox.Show(ActGlobals.oFormActMain, "No more found", "Find");

        }

        private void buttonNodeFindNext_Click(object sender, EventArgs e)
        {
            FindNextNode();
        }

        private void textBoxZoneFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!string.IsNullOrEmpty(textBoxZoneFind.Text))
                {
                    FindNextNode();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
        }

        private void buttonAddZone_Click(object sender, EventArgs e)
        {
            autoNodeName = string.Empty;
            string name = newZone;
            // if there is a zone selection on the ACT Main tab, use it as the default
            TreeNode actNode = ActGlobals.oFormActMain.MainTreeView.SelectedNode;
            if (actNode != null)
            {
                if (actNode.Level == 0)
                {
                    string fight = actNode.Text;
                    int dash = fight.IndexOf(" - ");
                    name = fight.Substring(0, dash);
                    autoNodeName = name; //if the user just accepts this, this is how .AfterLabledit() figures it out
                }
            }
            TreeNode add = new TreeNode(name);
            treeViewZones.Nodes.Add(add);
            treeViewZones.SelectedNode = add;
            Application.DoEvents();
            add.BeginEdit();
        }

        private void buttonAddMob_Click(object sender, EventArgs e)
        {
            TreeNode zoneNode = treeViewZones.SelectedNode;
            if (zoneNode != null)
            {
                if (zoneNode.Level == 1)
                    zoneNode = zoneNode.Parent;
                string name = newMob;
                autoNodeName = string.Empty;
                // default to selected mob on the main tree
                TreeNode actNode = ActGlobals.oFormActMain.MainTreeView.SelectedNode;
                if (actNode != null)
                {
                    if (actNode.Level == 1)
                    {
                        // zone mobs level
                        string fight = actNode.Text;
                        if (!string.IsNullOrEmpty(fight))
                        {
                            int dash = fight.IndexOf(" - ");
                            if (dash > 0)
                            {
                                string test = fight.Substring(0, dash);
                                if (!test.Equals("All"))
                                {
                                    name = test;
                                    autoNodeName = name; //if the user just accepts this, .AfterLabledit() needs to know
                                }
                            }
                        }
                    }
                    else if (actNode.Level == 2)
                    {
                        // individual fight level
                        name = actNode.Text;
                        autoNodeName = name;
                        if (treeViewZones.SelectedNode.Level == 1)
                        {
                            // plugin mob is selected
                            DialogResult result = SimpleMessageBox.Show(ActGlobals.oFormActMain, $"Append {autoNodeName} to {treeViewZones.SelectedNode.Text}?", "Apppend",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                            if (result == DialogResult.Yes)
                            {
                                Mob mob = treeViewZones.SelectedNode.Tag as Mob;
                                if(mob != null)
                                {
                                    mob.MobName += $", {autoNodeName}";
                                    treeViewZones.SelectedNode.Text = mob.MobName;
                                    BuildEnemyList();
                                    return;
                                }
                            }
                        }
                    }
                }
                
                TreeNode add = new TreeNode(name);
                zoneNode.Nodes.Add(add);
                treeViewZones.SelectedNode = add;
                Application.DoEvents();
                zoneNode.Expand();
                add.BeginEdit();
            }

        }

        private void treeViewZones_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (richEditCtrl1.rtbDoc.Modified)
            {
                if (treeViewZones.SelectedNode != null)
                {
                    // remove any highlight from the current RTB before we move on
                    UnHighlight();

                    // save the note for the current (about to be un-selected) node
                    SaveNote(treeViewZones.SelectedNode, false);
                }
            }
        }

        private void treeViewZones_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.IsSelected)
            {
                treeViewZones.SelectedNode.ImageIndex = 0;
                treeViewZones.SelectedNode.SelectedImageIndex = 1;
                treeViewZones.HideSelection = false;

                richEditCtrl1.rtbDoc.Clear();

                Zone zone = null;
                TreeNode sel = treeViewZones.SelectedNode;
                if (sel != null)
                {
                    if (sel.Level == 0)
                    {
                        zone = sel.Tag as Zone;
                        if (zone != null)
                        {
                            if (!string.IsNullOrEmpty(zone.Notes))
                            {
                                richEditCtrl1.rtbDoc.Rtf = zone.Notes;
                                richEditCtrl1.rtbDoc.Modified = false;
                            }
                        }
                    }
                    else if (sel.Level == 1)
                    {
                        TreeNode parent = sel.Parent;
                        zone = parent.Tag as Zone;
                        if (zone != null)
                        {
                            Mob mob = sel.Tag as Mob;
                            if (mob != null)
                            {
                                if (!string.IsNullOrEmpty(mob.Notes))
                                {
                                    richEditCtrl1.rtbDoc.Rtf = mob.Notes;
                                    richEditCtrl1.rtbDoc.Modified = false;
                                }
                            }
                        }
                    }

                    if(zone != null)
                    {
                        switch(zone.Paste)
                        {
                            case Zone.PasteType.Append:
                                radioButtonAppend.Checked = true;
                                break;
                            case Zone.PasteType.Replace:
                                radioButtonReplace.Checked = true;
                                break;
                            case Zone.PasteType.Ask:
                                radioButtonAsk.Checked = true;
                                break;
                            case Zone.PasteType.Accept:
                                radioButtonAccept.Checked = true;
                                break;
                        }
                    }

                    // do we have a paste section?
                    bool hasPaste = false;
                    string[] lines = richEditCtrl1.rtbDoc.Text.Split('\n');
                    foreach(string line in lines)
                    {
                        if (line.StartsWith(pastePrefix))
                        {
                            hasPaste = true;
                            buttonCompare.Enabled = true;
                            break;
                        }
                    }
                    if(!hasPaste)
                        buttonCompare.Enabled = false;
                }
            }
        }

        private void treeViewZones_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node != null)
            {
                bool removed = false;
                // if the user accepted a mob name from the main ACT tree without edit,
                // e.Label will be null and autoMobName will not be null
                string nodeName = e.Label;
                if (e.Label == null && !string.IsNullOrEmpty(autoNodeName))
                    nodeName = autoNodeName;

                if (string.IsNullOrEmpty(nodeName))
                {
                    if ((e.Node.Level == 0 && e.Node.Text == newZone)
                        || e.Node.Level == 1 && e.Node.Text == newMob)
                    {
                        // we don't accept "New Mob" or "New Zone". The user must enter something else
                        DialogResult result = SimpleMessageBox.Show(ActGlobals.oFormActMain, @"Please enter a new name.\line Or press [Cancel] to cancel the add.", "Invalid entry",
                            MessageBoxButtons.OKCancel);
                        if (result == DialogResult.OK)
                            e.Node.BeginEdit();
                        else
                        {
                            // user cancelled
                            e.Node.EndEdit(true);
                            e.Node.Remove();
                        }
                    }
                    else
                    {
                        // user hit ESC while editing an existing node
                        // revert to the unedited state
                        e.Node.EndEdit(true);
                    }
                }
                else
                {
                    // user modified the node name
                    if (e.Node.Level == 0)
                    {
                        Zone newZone = new Zone { ZoneName = nodeName };
                        if (zoneList.Zones.Contains(newZone))
                        {
                            SimpleMessageBox.Show(ActGlobals.oFormActMain, "Duplicate Zone Names are not allowed.", "Invalid entry",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Node.EndEdit(true);
                            e.Node.Remove();
                            autoNodeName = String.Empty;
                            removed = true;
                        }
                        else
                        {
                            // zone node
                            if (e.Node.Tag == null)
                            {
                                // new zone
                                zoneList.Zones.Add(newZone);
                                e.Node.Tag = newZone;
                                e.Node.Name = nodeName;
                            }
                            else
                            {
                                // editing an existing zone
                                Zone zone = e.Node.Tag as Zone;
                                if (zone != null)
                                    zone.ZoneName = nodeName;
                            }
                        }
                    }
                    else
                    {
                        // mob node
                        Zone zone = e.Node.Parent.Tag as Zone;
                        if (zone != null)
                        {
                            Mob newMob = new Mob { MobName = nodeName, KillOrder = zone.Mobs.Count };
                            if (zone.Mobs.Contains(newMob))
                            {
                                SimpleMessageBox.Show(ActGlobals.oFormActMain, "Duplicate Mob Names are not allowed.", "Invalid entry",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                e.Node.EndEdit(true);
                                e.Node.Remove();
                                autoNodeName = String.Empty;
                                removed = true;
                            }
                            else
                            {
                                if (e.Node.Tag == null)
                                {
                                    // new mob
                                    zone.Mobs.Add(newMob);
                                    e.Node.Tag = newMob;
                                    e.Node.Name = nodeName;
                                }
                                else
                                {
                                    // editing an existing mob
                                    Mob mob = e.Node.Tag as Mob;
                                    if (mob != null)
                                        mob.MobName = nodeName;
                                }
                            }
                        }
                    }
                    if (!removed)
                        treeViewZones.SelectedNode = e.Node;
                }
            }
        }

        private void treeViewZones_ItemDrag(object sender, ItemDragEventArgs e)
        {
            TreeNode node = e.Item as TreeNode;
            if (node != null)
            {
                if (node.Level == 0)
                    return; // can't drag zones

                if (treeViewZones.SelectedNode == node && richEditCtrl1.rtbDoc.Modified)
                {
                    SaveNote(node, false);
                }
            }
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treeViewZones_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void treeViewZones_DragDrop(object sender, DragEventArgs e)
        {
            TreeView tv = sender as TreeView;

            // Retrieve the client coordinates of the drop location.
            Point targetPoint = tv.PointToClient(new Point(e.X, e.Y));

            // Retrieve the node at the drop location.
            TreeNode targetNode = tv.GetNodeAt(targetPoint);

            // Retrieve the node that was dragged.
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            if (draggedNode == null || targetNode == null || draggedNode.Equals(targetNode))
                return;

            // only allowing dragging within the same zone
            // both nodes must have the same parent
            if (targetNode.Parent != null
                && draggedNode.Parent != null
                && targetNode.Parent.Equals(draggedNode.Parent))
            {
                // need to update the source data since the treeview is sorted
                // and will automatically re-sort when the nodes move
                Zone zone = targetNode.Parent.Tag as Zone;
                if (zone != null)
                {
                    try
                    {
                        Mob targetMob = targetNode.Tag as Mob;
                        Mob sourceMob = draggedNode.Tag as Mob;
                        if (sourceMob != null && targetMob != null)
                        {
                            int targetIndex = targetMob.KillOrder;
                            int sourceIndex = sourceMob.KillOrder;

                            if (targetIndex < sourceIndex)
                            {
                                // moved up
                                zone.Mobs.Insert(targetIndex, sourceMob);
                                zone.Mobs.RemoveAt(sourceIndex+1);
                            }
                            else // sourceIndex < targetIndex
                            {
                                // moved down
                                zone.Mobs.Insert(targetIndex+1, sourceMob);
                                zone.Mobs.RemoveAt(sourceIndex);
                            }
                            // fix the kill order
                            for (int i = 0; i < zone.Mobs.Count; i++)
                                zone.Mobs[i].KillOrder = i;

                            // Remove the node from its current location in the tree
                            // and add it to the drop location.
                            draggedNode.Remove();
                            targetNode.Parent.Nodes.Insert(targetIndex, draggedNode);
                        }
                    }
                    catch(Exception dx)
                    {
                        SimpleMessageBox.Show(ActGlobals.oFormActMain, dx.Message, "Drag Problem");
                    }
                }
            }
        }

        // from the ACT thread, on the UI thread; select the next mob after killing a mob
        private void KillProc(object o)
        {
            string nameKilled = o as string;
            if (!string.IsNullOrEmpty(nameKilled))
            {
                // see if we have anything for the current zone
                TreeNode[] zoneNodes = treeViewZones.Nodes.Find(ActGlobals.oFormActMain.CurrentZone, false);
                if (zoneNodes.Length > 0)
                {
                    Zone zone = zoneNodes[0].Tag as Zone;
                    if(zone != null)
                    {
                        // see if we have a mob that matches the passed name
                        //Mob mob = zone.Mobs.Find(m => m.MobName == nameKilled);
                        Mob mob = null;
                        foreach(Mob m in zone.Mobs)
                        {
                            if (m.MobName.Contains(nameKilled))
                            {
                                mob = m;
                                break;
                            }
                        }
                        if (mob != null)
                        {
                            // see if we have the next mob in the kill order
                            int nextKill = mob.KillOrder + 1;
                            Mob next = zone.Mobs.Find(n => n.KillOrder == nextKill);
                            if (next != null)
                            {
                                // find that mob in the tree
                                TreeNode[] mobNodes = zoneNodes[0].Nodes.Find(next.MobName, false);
                                if (mobNodes.Length > 0)
                                {
                                    // select the next mob in the kill order
                                    treeViewZones.SelectedNode = mobNodes[0];
                                    treeViewZones.SelectedNode.EnsureVisible();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void treeViewZones_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                if (treeViewZones.SelectedNode != null)
                {
                    treeViewZones.SelectedNode.BeginEdit();
                    e.Handled = true;
                }
            }
        }

        #region Tree Context Menu

        private void treeViewZones_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point pt = new Point(e.X, e.Y);
                clickedZoneNode = treeViewZones.GetNodeAt(pt);
                if (clickedZoneNode != null)
                {
                    clickedZonePoint = treeViewZones.PointToScreen(pt);
                    contextMenuStripZone.Show(clickedZonePoint);
                }
            }
        }

        private void contextMenuStripZone_Opening(object sender, CancelEventArgs e)
        {
            if (clickedZoneNode.Parent != null)
            {
                deleteToolStripMenuItem.Text = "Delete Mob";
            }
            else if (clickedZoneNode != null)
            {
                deleteToolStripMenuItem.Text = "Delete Zone";
            }
        }

        private void copyToXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (clickedZoneNode != null)
            {
                if (clickedZoneNode.Equals(treeViewZones.SelectedNode))
                {
                    SaveNote(clickedZoneNode, false);
                }

                Zone zone;
                Mob mob = null;
                bool haveNote = false;

                if (clickedZoneNode.Parent != null)
                {
                    zone = clickedZoneNode.Parent.Tag as Zone;
                    mob = clickedZoneNode.Tag as Mob;
                    haveNote = mob.Notes != null;
                }
                else
                {
                    zone = clickedZoneNode.Tag as Zone;
                    haveNote = zone.Notes != null;
                }

                if (!haveNote)
                    SimpleMessageBox.Show(ActGlobals.oFormActMain, "Selected item does not have any notes to copy.",
                        "Empty note", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    XmlCopyForm form = new XmlCopyForm(zone, mob);
                    form.Show();
                    PositionChildForm(form, clickedZonePoint);
                    form.TopMost = true;
                }
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (clickedZoneNode != null)
                {
                    if (clickedZoneNode.Level == 0)
                    {
                        // delete zone
                        string category = clickedZoneNode.Text;
                        if (SimpleMessageBox.Show(ActGlobals.oFormActMain, "Delete zone '" + category + "' and all its mobs and notes?", "Are you sure?",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                        {
                            Zone zone = clickedZoneNode.Tag as Zone;
                            if (zone != null)
                            {
                                zoneList.Zones.Remove(zone);
                                treeViewZones.Nodes.Remove(clickedZoneNode);
                            }
                        }
                    }
                    else if (clickedZoneNode.Level == 1)
                    {
                        // delete mob
                        Zone zone = clickedZoneNode.Parent.Tag as Zone;
                        Mob mob = clickedZoneNode.Tag as Mob;
                        if (zone != null && mob != null)
                        {
                            zone.Mobs.Remove(mob);
                            // fix the kill order so there is no gap
                            for (int i = 0; i < zone.Mobs.Count; i++)
                                zone.Mobs[i].KillOrder = i;
                            treeViewZones.Nodes.Remove(clickedZoneNode);
                        }
                    }
                }
            }
            catch { }
        }

        private void exportRTFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (clickedZoneNode != null)
            {
                saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    RichTextBox rtb = new RichTextBox();

                    // zone name
                    if(clickedZoneNode.Level == 0)
                        rtb.Text = clickedZoneNode.Text + "\n\n";
                    else
                        rtb.Text = clickedZoneNode.Parent.Text + "\n\n";
                    rtb.Select(0, rtb.TextLength);
                    Font currentFont = rtb.SelectionFont;
                    FontStyle newFontStyle;
                    newFontStyle = rtb.SelectionFont.Style | FontStyle.Bold | FontStyle.Underline;
                    rtb.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newFontStyle);

                    if (clickedZoneNode.Level == 0)
                    {
                        // export the whole zone
                        Zone zone = clickedZoneNode.Tag as Zone;
                        if (zone != null)
                        {
                            // zone note
                            if (zone.Notes != null)
                            {
                                rtb.Select(rtb.TextLength, 0);
                                rtb.SelectedRtf = @"{\rtf1\ansi\par\pard " + zone.Notes + @"\par}";
                            }

                            // mobs
                            foreach (Mob mob in zone.Mobs)
                            {
                                rtb.Select(rtb.TextLength, 0);
                                rtb.SelectedRtf = @"{\rtf1\ansi\pard\b " + mob.MobName + @"\b0\par}";

                                rtb.Select(rtb.TextLength, 0);
                                rtb.SelectedRtf = @"{\rtf1\ansi\par " + mob.Notes + @"\par}";

                            }
                        }
                    } // end of zone save
                    else
                    {
                        // mob note
                        Mob mob = clickedZoneNode.Tag as Mob;
                        if (mob != null)
                        {
                            rtb.Select(rtb.TextLength, 0);
                            rtb.SelectedRtf = @"{\rtf1\ansi\pard\b " + mob.MobName + @"\b0\par}";

                            rtb.Select(rtb.TextLength, 0);
                            rtb.SelectedRtf = @"{\rtf1\ansi\par " + mob.Notes + @"\par}";
                        }
                    }
                    File.WriteAllText(saveFileDialog1.FileName, rtb.Rtf);
                }
            }
        }


        #endregion Tree Context Menu


        #endregion Tree Panel

        #region Editor Panel

        private void Plugin_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                if (string.IsNullOrEmpty(currentZone))
                {
                    //we've never seen a zone change
                    // let's try to use wherever ACT thinks we are
                    currentZone = ActGlobals.oFormActMain.CurrentZone;
                    if (!string.IsNullOrEmpty(currentZone))
                    {
                        TreeNode[] nodes = treeViewZones.Nodes.Find(currentZone, false);
                        if (nodes.Length > 0)
                        {
                            if(nodes[0].Nodes.Count > 0)
                            {
                                // select the first mob
                                treeViewZones.SelectedNode = nodes[0].Nodes[0];
                                treeViewZones.SelectedNode.EnsureVisible();
                            }
                            else
                            {
                                // select the zone
                                treeViewZones.SelectedNode = nodes[0];
                                treeViewZones.SelectedNode.EnsureVisible();
                            }
                        }
                    }
                }

                GetWhiteList(); // to show/hide the Accept radio button

                if (neverBeenVisible)
                {
                    //set the splitter only on the first time shown
                    if (zoneList.SplitterLoc > 0)
                        splitContainer1.SplitterDistance = zoneList.SplitterLoc;
                    neverBeenVisible = false;
                }
            }
        }

        private void PositionChildForm(Form form, Point loc)
        {
            //make sure it fits on screen
            if (loc.X + form.Width > SystemInformation.VirtualScreen.Right)
                loc.X = SystemInformation.VirtualScreen.Right - form.Width;
            if (loc.Y + form.Height > SystemInformation.WorkingArea.Bottom)
                loc.Y = SystemInformation.WorkingArea.Bottom - form.Height;
            form.Location = loc;
        }

        private void textBoxTextFind_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxEditFind.Text))
            {
                if(checkBoxCurrentCategory.Checked)
                {
                    buttonFindNext.Enabled = false;
                    if(treeViewZones.SelectedNode != null)
                    {
                        string nodeName = treeViewZones.SelectedNode.Text;
                        string searchText = textBoxEditFind.Text.ToLower();
                        bool found = FindInNote(nodeName, searchText, richEditCtrl1.rtbDoc.Rtf);
                        if (!found)
                            SimpleMessageBox.Show(ActGlobals.oFormActMain, "Not found", "Find Text");
                    }
                }
                else
                {
                    buttonFindNext.Enabled = true;
                    foundZoneIndex = 0;
                    foundMobIndex = 0;
                    foundZoneNeedMobs = false;
                    FindNextText();
                }
            }
            else
            {
                buttonFindNext.Enabled = false;
                UnHighlight();
            }
        }

        private void FindNextText()
        {
            string searchText = textBoxEditFind.Text.ToLower();
            bool found = false;
            for (; foundZoneIndex < zoneList.Zones.Count; foundZoneIndex++)
            {
                Zone zone = zoneList.Zones[foundZoneIndex];
                if (zone.Notes != null && !foundZoneNeedMobs)
                {
                    found = FindInNote(zone.ZoneName, searchText, zone.Notes);
                    if (found)
                    {
                        foundZoneNeedMobs = true;
                        break;
                    }
                }
                if (!found && zone.Mobs.Count > 0)
                {
                    for (; foundMobIndex < zone.Mobs.Count; foundMobIndex++)
                    {
                        Mob mob = zone.Mobs[foundMobIndex];
                        found = FindInNote(mob.MobName, searchText, mob.Notes);
                        if (found)
                        {
                            foundMobIndex++; // find-next will start with the next mob
                            break;
                        }
                    }
                }
                if (found)
                    break;
                foundZoneNeedMobs = false;
                foundMobIndex = 0;
            }
            if (!found)
            {
                UnHighlight();
                SimpleMessageBox.Show(ActGlobals.oFormActMain, "Not found", "Find");
            }
        }

        private bool FindInNote(string nodeName, string searchText, string note)
        {
            bool found = false;
            RichTextBox rtb = new RichTextBox();
            rtb.Rtf = note;
            int startIndex = 0;
            bool foundFirst = false;
            while (startIndex < rtb.TextLength)
            {
                int wordstartIndex = rtb.Find(searchText, startIndex, RichTextBoxFinds.None);
                if (wordstartIndex != -1)
                {
                    TreeNode[] nodes = treeViewZones.Nodes.Find(nodeName, true);
                    if (nodes.Length > 0)
                    {
                        found = true;
                        if (!foundFirst)
                        {
                            foundFirst = true;
                            treeViewZones.SelectedNode = nodes[0];
                            treeViewZones.SelectedNode.EnsureVisible();
                            UnHighlight();
                        }
                        richEditCtrl1.rtbDoc.SelectionStart = wordstartIndex;
                        richEditCtrl1.rtbDoc.SelectionLength = searchText.Length;
                        richEditCtrl1.rtbDoc.SelectionBackColor = foundBackground;
                    }
                }
                else
                    break;
                startIndex += wordstartIndex + searchText.Length;
            }
            return found;
        }

        private void buttonTextFindNext_Click(object sender, EventArgs e)
        {
            FindNextText();
        }

        private void textBoxEditFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!string.IsNullOrEmpty(textBoxEditFind.Text))
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    FindNextText();
                }
            }
        }

        private void checkBoxCurrentCategory_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCurrentCategory.Checked)
                textBoxEditFind.Text = string.Empty;
        }

        private void RichEditCtrl1_OnSave(object sender, EventArgs e)
        {
            TreeNode sel = treeViewZones.SelectedNode;
            SaveNote(sel, true);
        }

        private void SaveNote(TreeNode node, bool saveToDisk)
        {
            if (node != null)
            {
                // remove any "find" highlights before we save it
                UnHighlight();

                Zone zone = null;
                if (node.Level == 0)
                {
                    // zone note
                    string zoneName = node.Text;
                    zone = zoneList.Zones.Find(z => z.ZoneName == zoneName);
                    if (zone != null)
                    {
                        if (richEditCtrl1.rtbDoc.Text.Length > 0)
                            zone.Notes = richEditCtrl1.rtbDoc.Rtf;
                        else
                            zone.Notes = null;
                    }
                }
                else
                {
                    // mob note
                    string zoneName = node.Parent.Text;
                    string mobName = node.Text;
                    zone = zoneList.Zones.Find(z => z.ZoneName == zoneName);
                    if (zone != null)
                    {
                        Mob mob = zone.Mobs.Find(m => m.MobName == mobName);
                        if (mob != null)
                        {
                            if(richEditCtrl1.rtbDoc.Text.Length > 0)
                                mob.Notes = richEditCtrl1.rtbDoc.Rtf;
                            else
                                mob.Notes = null;
                        }
                    }
                }

                if(zone != null)
                {
                    zone.Paste = GetPasteButton();
                    if (saveToDisk)
                        SaveSettings();
                }
            }
        }

        private Zone.PasteType GetPasteButton()
        {
            Zone.PasteType ret = Zone.PasteType.Append;
            if (radioButtonAccept.Checked)
                ret = Zone.PasteType.Accept;
            else if (radioButtonAppend.Checked)
                ret = Zone.PasteType.Append;
            else if (radioButtonReplace.Checked)
                ret = Zone.PasteType.Replace;
            else if (radioButtonAsk.Checked)
                ret  = Zone.PasteType.Ask;
            return ret;
        }

        private void UnHighlight()
        {
            if(richEditCtrl1.rtbDoc.Text.Length > 0)
            {
                // remove any "find" highlights
                richEditCtrl1.SuspendLayout();
                int selStart = richEditCtrl1.rtbDoc.SelectionStart;
                int selLen = richEditCtrl1.rtbDoc.SelectionLength;
                richEditCtrl1.rtbDoc.SelectAll();
                richEditCtrl1.rtbDoc.SelectionBackColor = defaultBackground;
                richEditCtrl1.rtbDoc.SelectionStart = selStart;
                richEditCtrl1.rtbDoc.SelectionLength = selLen;
                richEditCtrl1.ResumeLayout();
            }
        }

        private void VisitLink()
        {
            // Change the color of the link text by setting LinkVisited
            // to true.
            linkLabel1.LinkVisited = true;
            //Call the Process.Start method to open the default browser
            //with a URL:
            System.Diagnostics.Process.Start("https://github.com/jeffjl74/ACT_Notes#act-notes-plugin");
        }

        private void buttonCompare_Click(object sender, EventArgs e)
        {
            if (richEditCtrl1.rtbDoc.Modified)
                SaveNote(treeViewZones.SelectedNode, false);

            string[] lines = richEditCtrl1.rtbDoc.Text.Split('\n');
            List<string> orig = new List<string>();
            List<string> paste = new List<string>();
            bool found = false;
            // break into two sections using the "received a share" delimiter as the break
            foreach (string line in lines)
            {
                if (!found)
                {
                    if (line.StartsWith(pastePrefix))
                        found = true;
                    else
                        orig.Add(line);
                }
                else
                {
                    if (line.StartsWith(pastePrefix))
                        break;
                    paste.Add(line);
                }
            }

            string textA = string.Join("\n", orig.ToArray());
            string textB = string.Join("\n", paste.ToArray());
            List<string> html = Differences.DiffHtml(textA, textB);
            if(html.Count == 2)
            {
                Differences diff = new Differences(ActGlobals.oFormActMain, html[0], html[1]);
                if (treeViewZones.SelectedNode != null)
                {
                    if (treeViewZones.SelectedNode.Parent != null)
                        diff.Text = $"Differences: {treeViewZones.SelectedNode.Parent.Text} / {treeViewZones.SelectedNode.Text}";
                }
                diff.Show();
                PositionChildForm(diff, ActGlobals.oFormActMain.Location);
            }
        }

        private Zone ZoneFromSelectedNode()
        {
            Zone zone = null;
            if(treeViewZones.SelectedNode !=null)
            {
                if(treeViewZones.SelectedNode.Parent != null)
                    zone = treeViewZones.SelectedNode.Parent.Tag as Zone;
                else
                    zone = treeViewZones.SelectedNode.Tag as Zone;
            }
            return zone;
        }

        private void radioButtonAppend_Click(object sender, EventArgs e)
        {
            Zone zone = ZoneFromSelectedNode();
            if(zone != null)
                zone.Paste = Zone.PasteType.Append;
        }

        private void radioButtonReplace_Click(object sender, EventArgs e)
        {
            Zone zone = ZoneFromSelectedNode();
            if (zone != null)
                zone.Paste = Zone.PasteType.Replace;
        }

        private void radioButtonAsk_Click(object sender, EventArgs e)
        {
            Zone zone = ZoneFromSelectedNode();
            if (zone != null)
                zone.Paste = Zone.PasteType.Ask;
        }

        private void radioButtonAccept_Click(object sender, EventArgs e)
        {
            Zone zone = ZoneFromSelectedNode();
            if (zone != null)
                zone.Paste = Zone.PasteType.Accept;
        }

        #endregion Editor Panel
    }

}
