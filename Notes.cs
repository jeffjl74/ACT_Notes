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
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.IO.Compression;
using System.Xml.Linq;

[assembly: AssemblyTitle("Notes for zone mobs")]
[assembly: AssemblyDescription("Organize notes for mobs")]
[assembly: AssemblyCompany("Mineeme of Maj'Dul")]
[assembly: AssemblyVersion("1.5.0.0")]

namespace ACT_Notes
{
    public partial class Notes : UserControl, IActPluginV1
	{
        const string helpUlr = "https://github.com/jeffjl74/ACT_Notes#act-notes-plugin";
        
        readonly bool debugMode = false;  //true to test using imports, mouse selection alerts, and the game not running

        // the data
        ZoneList zoneList = new ZoneList();
        XmlSerializer xmlSerializer;
        char nameSeparator = ';';
        public static string DefaultGroupName = " Default Group";

        // zone change support
        System.Timers.Timer zoneTimer = new System.Timers.Timer();
        string currentZone = string.Empty;
        string cleanZone = string.Empty;
        //tree enemies for the selected zone
        List<string> enemies = new List<string>();
        bool skipKillCheck = true;
        Regex reCleanActZone = new Regex(@"(?::.+?:)?(?<decoration>\\#[0-9A-F]{6})?(?<zone>.+?)(?: \d+)?$", RegexOptions.Compiled);

        // alerts
        AlertForm alertForm;
        bool doNotAnnounce = true;  //user selection of a note does not trigger an audio announcement
        bool seenEq2 = false;

        WindowsFormsSynchronizationContext mUiContext = new WindowsFormsSynchronizationContext();
        
        // scroll bar control
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        UInt32 WM_VSCROLL = 277;

        // zip file support
        private static Guid FolderDownloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path);

        // tree nodes
        class NodeLevels { public const int Group = 0; public const int Zone = 1; public const int Mob = 2; };
        List<TreeNode> foundNodes = new List<TreeNode>();
        int foundNodesIndex = 0;
        int foundZoneIndex = 0;

        // text find in zone notes and mob notes
        //background for a found word
        //use a slightly off "Color.Yellow" so as to be unlikely to be chosen by the user
        //as a backgound color
        Color foundBackground = Color.FromArgb(0xfdfd00);
        List<HighLightRange> highLightRanges = new List<HighLightRange>();

        // new node support
        const string newGroup = "New Group";
        const string newMob = "New Mob";
        const string newZone = "New Zone";
        string autoNodeName = string.Empty;         //when selecting from the ACT main tab

        // xml share
        static public string XmlSnippetType = "Note";
        public enum XmlShareDepth { GroupOnly, GroupDeep, ZoneOnly, ZoneDeep, MobOnly };
        static string pastePrefix = @"--------- received ";
        List<string> whitelist = new List<string>();
        string[] xmlSections;
        bool[] packetTrack;
        internal class xmlQclass 
        {
            public bool Active = false;
            public Zone zone;
            public string group;
            public string data;
            public string player;
            public bool compressed;
            public int numSections;
            public int currentSection;
            public xmlQclass()
            {
                Active = false;
                zone = null;
                group = string.Empty;
                data = string.Empty;
                player = string.Empty;
                compressed = false;
                numSections = 0;
                currentSection = 0;
            }
        }
        xmlQclass rxXml = new xmlQclass();
        ConcurrentQueue<xmlQclass> xmlIncoming = new ConcurrentQueue<xmlQclass>();

        ImageList treeImages = new ImageList();     //tree folder images

        // context menu support
        TreeNode clickedZoneNode = null;
        Point clickedZonePoint;
        bool firstExport = true;

        bool neverBeenVisible = true;               //save the splitter location only if it has been initialized 

        Label lblStatus;                            // The status label that appears in ACT's Plugin tab

        string settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\Notes.config.xml");

        //in-game add mob monitoring
        static int logTimeStampLength = ActGlobals.oFormActMain.TimeStampLen;
        const string InGameNoteCmd = "Unknown command: 'note add'";
        Regex reConsider = new Regex(@"^\\#[0-9A-F]{6}You consider (?<mob>[^.]+)", RegexOptions.Compiled);
        enum AddNoteState { Idle, Consider }
        AddNoteState addNoteState = AddNoteState.Idle;
        internal class AddMobQclass { public string zone; public string mob; }
        ConcurrentQueue<AddMobQclass> addMobQ = new ConcurrentQueue<AddMobQclass>();

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
            richEditCtrl1.OnUserChangedText += RichEditCtrl1_OnUserChangedText;

            // check for a zone change once a second
            zoneTimer.Interval = 1000;
            zoneTimer.Enabled = true;
            zoneTimer.AutoReset = true;
            zoneTimer.SynchronizingObject = ActGlobals.oFormActMain;
            zoneTimer.Elapsed += ZoneTimer_Elapsed;
            zoneTimer.Start();

            ActGlobals.oFormActMain.XmlSnippetAdded += OFormActMain_XmlSnippetAdded;    //for incoming shared note
            ActGlobals.oFormActMain.OnCombatEnd += OFormActMain_OnCombatEnd;            //for tracking kills
            ActGlobals.oFormActMain.OnLogLineRead += OFormActMain_OnLogLineRead;        //for adding mobs

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
            ActGlobals.oFormActMain.OnCombatEnd -= OFormActMain_OnCombatEnd;
            ActGlobals.oFormActMain.OnLogLineRead -= OFormActMain_OnLogLineRead;

            SaveSettings();
			lblStatus.Text = "Plugin Exited";
		}

        #endregion IActPluginV1 Members

        #region ACT Interaction

        private void OFormActMain_OnLogLineRead(bool isImport, LogLineEventArgs logInfo)
        {
            if( !isImport && logInfo.detectedType == 0 && logInfo.logLine.Length > logTimeStampLength)
            {
                string content = logInfo.logLine.Substring(logTimeStampLength);
                if (addNoteState == AddNoteState.Idle)
                {
                    if (content == InGameNoteCmd)
                        addNoteState = AddNoteState.Consider;
                }
                else if (addNoteState == AddNoteState.Consider)
                {
                    Match match = reConsider.Match(content);
                    if (match.Success)
                    {
                        addMobQ.Enqueue(new AddMobQclass { zone = cleanZone, mob = match.Groups["mob"].Value });
                        mUiContext.Post(AddMobProc, null);
                        addNoteState = AddNoteState.Idle;
                    }
                }
            }
        }

        private void OFormActMain_OnCombatEnd(bool isImport, CombatToggleEventArgs encounterInfo)
        {
            if (isImport && debugMode)
            {
                if (currentZone != ActGlobals.oFormActMain.CurrentZone)
                    ZoneTimer_Elapsed(null, null); //for debug while importing since we can outrun the timer
            }

            if (enemies.Count > 0 && (!isImport || debugMode))
            {
                int sucessLevel = encounterInfo.encounter.GetEncounterSuccessLevel();
                // if we won, see if the mob is in our list so we can move to the next note
                bool found = false;
                if (sucessLevel == 1 || skipKillCheck)
                {
                    // what we're looking for should be the strongest
                    string strongest = encounterInfo.encounter.GetStrongestEnemy(ActGlobals.charName);
                    if (enemies.Contains(strongest.ToLower()))
                    {
                        mUiContext.Post(KillProc, strongest);
                        Debug.WriteLineIf(debugMode, $"**CombatEnd(strongest): Post next name after {strongest}");
                        found = true;
                    }
                    if(!found)
                    {
                        // no match on the strongest, just look at every combatant
                        foreach (CombatantData d in encounterInfo.encounter.Items.Values)
                        {
                            if (enemies.Contains(d.Name.ToLower()))
                            {
                                mUiContext.Post(KillProc, d.Name);
                                Debug.WriteLineIf(debugMode, $"**CombatEnd(success): Post next name after {d.Name}");
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                        Debug.WriteLineIf(debugMode, $"CombatEnd(success): no matching enemy");
                }
            }
        }

        private void ZoneTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //look for zone change
            if (currentZone != ActGlobals.oFormActMain.CurrentZone)
            {
                currentZone = cleanZone = ActGlobals.oFormActMain.CurrentZone;
                Match match = reCleanActZone.Match(currentZone);
                if (match.Success)
                    cleanZone = match.Groups["zone"].Value.Trim();
                Zone zone = zoneList[currentZone];
                if(zone == null)
                    zone = zoneList[cleanZone];
                if (zone != null)
                    skipKillCheck = zone.SkipKillCheck;
                else
                    skipKillCheck = true;
                TreeNode node = BuildEnemyList();
                if (node != null)
                {
                    doNotAnnounce = false;
                    treeViewZones.SelectedNode = node;
                    try
                    {
                        // have seen this still be null while importing/debug, so try to avoid an exception
                        if (treeViewZones.SelectedNode != null)
                            treeViewZones.SelectedNode.EnsureVisible();
                    }
                    catch
                    {
                        // race condition while importing. just ignore it. only impact is EnsureVisible() failed.
                    }
                    doNotAnnounce = true;
                }
            }

            // hide the alert?
            bool eqrunning = Process.GetProcessesByName("EverQuest2").Length > 0 ? true : false;
            if(eqrunning && !seenEq2)
                seenEq2 = true;
            if (alertForm != null && !eqrunning && seenEq2)
            {
                alertForm.Close();
                alertForm = null;
            }
        }

        private TreeNode BuildEnemyList()
        {
            enemies.Clear();
            TreeNode result = null;
            //TreeNode[] nodes = treeViewZones.Nodes.Find(cleanZone, true);
            //if (nodes.Length > 0)
            TreeNode zoneNode = FindZoneNode(cleanZone);
            if (zoneNode != null)
            {
                //TreeNode zoneNode = nodes[0];
                Zone zone = zoneNode.Tag as Zone;
                TreeNode mobNode = null;
                if (zoneNode.Nodes.Count > 0)
                {
                    mobNode = zoneNode.Nodes[0];
                    if (zone != null)
                    {
                        foreach (Mob m in zone.Mobs)
                        {
                            string[] names = m.MobName.Split(nameSeparator);
                            foreach (string name in names)
                            {
                                enemies.Add(name.Trim().ToLower());
                            }
                        }
                    }
                }
                if (mobNode == null)
                    result = zoneNode;
                else if (zone.Notes != null)
                {
                    // look for alert tags in the zone note
                    bool hasAlert = false;
                    using (RichTextBox rtb = new RichTextBox())
                    {
                        rtb.Rtf = zone.Notes;
                        string text = rtb.Text;
                        int stopAt = text.IndexOf(pastePrefix);
                        if (stopAt == -1)
                            stopAt = text.Length;
                        for (int i = 0; i < stopAt; i++)
                        {
                            rtb.Select(i, 1);
                            if (rtb.SelectionColor == EditCtrl.audioAlertColor
                                || rtb.SelectionColor == EditCtrl.visualAlertColor
                                || rtb.SelectionColor == EditCtrl.audioVisualAlertColor)
                            {
                                hasAlert = true;
                                break;
                            }
                        }
                    }
                    // show the zone note if it has alerts
                    if (hasAlert)
                        result = zoneNode;
                    else // otherwise, show the mob node
                        result = mobNode;
                }
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
                        + @"\line This update adds a top level folder to the zone tree. "
                        + @"\line\line Update it now?"
                        + @"\line If there is an update to ACT"
                        + @"\line you should click No and update ACT first."
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
                        int currentSection = -1;
                        int numSections = -1;
                        int.TryParse(counts[0], out currentSection);
                        int.TryParse(counts[1], out numSections);
                        //we have to get section 1 first
                        if (currentSection == 1)
                        {
                            rxXml = new xmlQclass();
                            rxXml.numSections = numSections;
                            xmlSections = new string[rxXml.numSections];
                            packetTrack = new bool[rxXml.numSections];
                            rxXml.Active = true;
                        }
                        if(rxXml.Active)
                        {
                            rxXml.currentSection = currentSection;
                            packetTrack[rxXml.currentSection - 1] = true;

                            string group;
                            if (e.XmlAttributes.TryGetValue("G", out group))
                            {
                                if (!string.IsNullOrEmpty(group))
                                    rxXml.group = XmlCopyForm.DecodeShare(group);
                            }

                            string zone = "";
                            if (e.XmlAttributes.TryGetValue("Z", out zone))
                            {
                                if (rxXml.zone == null)
                                    rxXml.zone = new Zone();
                                rxXml.zone.ZoneName = XmlCopyForm.DecodeShare(zone);
                            }

                            string data;
                            if (e.XmlAttributes.TryGetValue("N", out data))
                            {
                                if (xmlSections.Length > rxXml.currentSection - 1)
                                {
                                    xmlSections[rxXml.currentSection - 1] = data;
                                }
                            }

                            string mobName;
                            if (e.XmlAttributes.TryGetValue("M", out mobName))
                            {
                                if (rxXml.zone != null)
                                {
                                    Mob mob = new Mob { MobName = XmlCopyForm.DecodeShare(mobName) };
                                    rxXml.zone.Mobs.Add(mob);
                                }
                            }

                            string sendingPlayer;
                            if (e.XmlAttributes.TryGetValue("P", out sendingPlayer))
                                rxXml.player = sendingPlayer;

                            string isCompressed;
                            if (e.XmlAttributes.TryGetValue("C", out isCompressed))
                                rxXml.compressed = !string.IsNullOrEmpty(isCompressed);

                            // done if we have all sections
                            if (!packetTrack.Contains(false))
                            {
                                rxXml.data = string.Join("", xmlSections);
                                xmlIncoming.Enqueue(rxXml);
                                mUiContext.Post(XmlPasteProc, null);
                                rxXml = new xmlQclass(); // redudant when everything goes well, but good for error recovery/avoidance
                            }
                        } // active section gathering
                    } // found both section numbers
                } // found section ID

                e.Handled = true;
            }
        }

        // on the UI thread
        private void XmlPasteProc(object o)
        {
            xmlQclass incoming;
            while (xmlIncoming.TryDequeue(out incoming))
            {
                // incoming data has substitutions to work around EQII and ACT expectations
                string data = XmlCopyForm.DecodeShare(incoming.data);
                if (incoming.compressed)
                    data = XmlCopyForm.DecompressShare(data);
                // test for a good rtf
                try
                {
                    RichTextBox rtb = new RichTextBox();
                    rtb.Rtf = data;
                }
                catch (Exception tx)
                {
                    SimpleMessageBox.Show(ActGlobals.oFormActMain, "Illegal note format: " + tx.Message, "Note Share Error");
                    continue;
                }

                // group data
                NoteGroup ng = zoneList.NoteGroups[incoming.group];
                if (!string.IsNullOrEmpty(incoming.group) && incoming.zone == null)
                {
                    // incoming data is the note for the group

                    // we only save the group note if we already have that group
                    // (we don't create a new group just for a group note)
                    if (ng != null)
                    {
                        Zone.PasteType mergeOp = ng.Paste;
                        ng.Notes = ProcessMergeType(mergeOp, ng.GroupName, incoming.player, ng.Notes, data);
                    }
                    else
                    {
                        // we do not have this group
                        // so we don't know where this note goes
                        // so just ignore this note
                        continue;
                    }
                }
                string groupName = incoming.group;
                if (string.IsNullOrEmpty(groupName))
                {
                    groupName = DefaultGroupName;
                    ng = zoneList.NoteGroups[groupName];
                }
                if (ng == null)
                {
                    if(incoming.zone != null)
                    {
                        // see if this zone is already in a group with a different name
                        string existingGroup = zoneList.NoteGroups.GetGroupName(incoming.zone.ZoneName);
                        ng = zoneList.NoteGroups[existingGroup];
                        if (ng != null)
                            groupName = existingGroup;
                    }
                    if(ng == null)
                    {
                        ng = new NoteGroup { GroupName = groupName };
                        zoneList.NoteGroups.Add(ng);
                    }
                }

                // group tree
                TreeNode groupNode = FindGroupNode(groupName);
                if (groupNode == null)
                {
                    groupNode = new TreeNode(groupName);
                    groupNode.Name = groupName;
                    groupNode.Tag = ng;
                }

                if(incoming.zone != null)
                {
                    Zone incZone = incoming.zone;
                    Zone zone = zoneList.Zones.Find(z => z.ZoneName == incZone.ZoneName);
                    if (zone == null)
                    {
                        // new zone/mob
                        if (incZone.Mobs.Count == 0)
                        {
                            // no mob, just zone notes
                            incZone.Notes = data;
                        }
                        else
                        {
                            incZone.Mobs[0].Notes = data;
                            incZone.Mobs[0].KillOrder = 0;
                        }

                        zoneList.Zones.Add(incZone);

                        if (groupName == DefaultGroupName)
                            zoneList.NoteGroups.PutNoteInGroup(DefaultGroupName, incZone.ZoneName);
                        else
                            zoneList.NoteGroups.AddNoteIfNotGrouped(groupName, incZone.ZoneName);

                        TreeNode zn = groupNode.Nodes.Add(incZone.ZoneName);
                        zn.Tag = incZone;
                        zn.Name = incZone.ZoneName;
                        if (incZone.Mobs.Count > 0)
                        {
                            TreeNode mn = zn.Nodes.Add(incZone.Mobs[0].MobName);
                            mn.Tag = incZone.Mobs[0];
                            mn.Name = incZone.Mobs[0].MobName;
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

                        if (!string.IsNullOrEmpty(incoming.group))
                            zoneList.NoteGroups.AddNoteIfNotGrouped(incoming.group, incZone.ZoneName);

                        if (incZone.Mobs.Count > 0)
                        {
                            // existing mob note?
                            Mob mob = zone.Mobs.Find(m => m.MobName == incZone.Mobs[0].MobName);
                            if (mob != null)
                            {
                                //
                                // existing zone, existing mob
                                //
                                TreeNode destMobNode = null;
                                TreeNode zoneNode = FindZoneNode(incZone.ZoneName);
                                if (zoneNode != null)
                                {
                                    TreeNode[] mobNodes = zoneNode.Nodes.Find(mob.MobName, false);
                                    if (mobNodes.Length > 0)
                                    {
                                        destMobNode = mobNodes[0];
                                        if (treeViewZones.SelectedNode == destMobNode && richEditCtrl1.rtbDoc.Modified)
                                            SaveNote(destMobNode, true);
                                    }
                                }

                                mob.Notes = ProcessMergeType(mergeOp, mob.MobName, incoming.player, mob.Notes, data);

                                if (destMobNode != null)
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
                                incZone.Mobs[0].Notes = data;
                                incZone.Mobs[0].KillOrder = zone.Mobs.Count;
                                zone.Mobs.Add(incZone.Mobs[0]);
                                TreeNode node = FindZoneNode(incZone.ZoneName);
                                if (node != null)
                                {
                                    TreeNode mn = node.Nodes.Add(incZone.Mobs[0].MobName);
                                    mn.Tag = incZone.Mobs[0];
                                    mn.Name = incZone.Mobs[0].MobName;
                                    treeViewZones.Sort();
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
                            TreeNode node = FindZoneNode(zone.ZoneName);
                            if (node != null)
                            {
                                destZoneNode = node;
                                if (treeViewZones.SelectedNode == destZoneNode && richEditCtrl1.rtbDoc.Modified)
                                    SaveNote(destZoneNode, true);
                            }

                            zone.Notes = ProcessMergeType(mergeOp, zone.ZoneName, incoming.player, zone.Notes, data);

                            if (destZoneNode != null)
                            {
                                if (treeViewZones.SelectedNode == destZoneNode)
                                    treeViewZones.SelectedNode = null; //force an update
                                destZoneNode.EnsureVisible();
                                treeViewZones.SelectedNode = destZoneNode;
                            }
                        }
                    }
                }
            }
            PopulateZonesTree();
            SaveSettings();
        }

        private string ProcessMergeType(Zone.PasteType mergeOp, string prompt, string player, string existingNote, string update)
        {
            if (mergeOp == Zone.PasteType.Ask)
                mergeOp = AskPasteType(prompt);
            if (mergeOp == Zone.PasteType.Ignore)
                return existingNote;
            if (mergeOp == Zone.PasteType.Replace ||
               (mergeOp == Zone.PasteType.Accept && whitelist.Contains(player)))
            {
                return update;
            }
            else // append
            {
                // only need to append if the update is different from the existing note
                // and to compare, we need to remove line breaks the way XmlCopyForm does
                string noBreaks = XmlCopyForm.RemoveEndLines(existingNote);
                if (noBreaks != update)
                {
                    // use a RichTextBox to merge RTF docs
                    RichTextBox rtb = new RichTextBox();
                    if (!string.IsNullOrEmpty(existingNote))
                    {
                        rtb.Rtf = existingNote;
                        // delimiter
                        AppendDelimiter(rtb, player);
                    }
                    rtb.Select(rtb.TextLength, 0);
                    rtb.SelectedRtf = update;
                    return rtb.Rtf;
                }
                else
                    return existingNote;
            }
        }

        // on the UI thread
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

        private void AppendDelimiter(RichTextBox rtb, string player)
        {
            rtb.Select(rtb.TextLength, 0);
            string who = string.Empty;
            if(!string.IsNullOrEmpty(player))
                who = $" from {player}";
            string stats = $"{pastePrefix}{who} {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}";
            rtb.SelectedRtf = @"{\rtf1\ansi{\colortbl ;}\par\pard\cf0\b " + stats + @" ------------\b0\par}";
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

        private void AddMobProc(object o)
        {
            AddMobQclass incoming;
            while (addMobQ.TryDequeue(out incoming))
            {
                string groupName = zoneList.NoteGroups.GetGroupName(incoming.zone);
                if (string.IsNullOrEmpty(groupName))
                {
                    TreeNode tn = treeViewZones.SelectedNode;
                    if (tn != null)
                    {
                        switch(tn.Level)
                        {
                            case NodeLevels.Group:
                                groupName = tn.Text;
                                break;
                            case NodeLevels.Zone:
                                groupName = tn.Parent != null ? tn.Parent.Text : "";
                                break;
                            case NodeLevels.Mob:
                                groupName = tn.Parent != null && tn.Parent.Parent != null ? tn.Parent.Parent.Text : "";
                                break;
                        }
                    }
                    if (string.IsNullOrEmpty(groupName))
                        groupName = DefaultGroupName;
                }
                NoteGroup ng = zoneList.NoteGroups[groupName];
                if(ng != null)
                {
                    Zone zone = zoneList[incoming.zone];
                    if(zone == null)
                    {
                        zone = new Zone { ZoneName = incoming.zone};
                        zoneList.Zones.Add(zone);
                    }
                    if(!ng.Zones.Contains(incoming.zone))
                    {
                        ng.Zones.Add(incoming.zone);
                    }
                    Mob mob = zone.GetMob(incoming.mob);
                    if(mob == null)
                    {
                        mob = new Mob { MobName = incoming.mob, KillOrder = zone.Mobs.Count };
                        zone.Mobs.Add(mob);
                        PopulateZonesTree();
                        TreeNode zn = FindZoneNode(incoming.zone);
                        if (zn != null)
                        {
                            TreeNode[] addmobs = zn.Nodes.Find(incoming.mob, false);
                            if(addmobs != null && addmobs.Count() > 0)
                            {
                                addmobs[0].EnsureVisible();
                                treeViewZones.SelectedNode = addmobs[0];
                            }
                        }
                    }
                }
            }
            SaveSettings();
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
            string prevNode = string.Empty;
            TreeNode sel = treeViewZones.SelectedNode;
            if (sel != null)
            {
                prevNode = sel.Text;
            }

            treeViewZones.Nodes.Clear();
            treeViewZones.SuspendLayout();

            // first, make all of the group nodes
            foreach (NoteGroup ng in zoneList.NoteGroups)
            {
                TreeNode parent = new TreeNode(ng.GroupName);
                parent.Name = ng.GroupName;
                parent.Tag = ng;
                treeViewZones.Nodes.Add(parent);
            }

            // add zones & mobs
            foreach (Zone zone in zoneList.Zones)
            {
                string groupname = zoneList.NoteGroups.GetGroupName(zone.ZoneName);
                TreeNode groupNode = FindGroupNode(groupname);
                if (groupNode == null)
                {
                    // we get here when upgrading from plugin version without zone groups
                    // put the zone in the default group
                    groupNode = FindOrCreateDefaultGroupNode();
                    zoneList.NoteGroups.PutNoteInGroup(DefaultGroupName, zone.ZoneName);
                }
                TreeNode znode = new TreeNode(zone.ZoneName);
                znode.Name = zone.ZoneName; // so .Find() will work
                znode.Tag = zone;
                groupNode.Nodes.Add(znode);
                foreach(Mob mob in zone.Mobs)
                {
                    TreeNode mnode = new TreeNode(mob.MobName);
                    mnode.Name = mob.MobName;
                    mnode.Tag = mob;
                    znode.Nodes.Add(mnode);
                }
            }

            treeViewZones.Sort();

            // remove any orphan zones in the note groups
            // (shouldn't be any if everything else is working correctly)
            int gcount = zoneList.NoteGroups.Count;
            for (int i = gcount - 1; i >= 0; i--)
            {
                int zcount = zoneList.NoteGroups[i].Zones.Count;
                for(int j = zcount - 1; j>=0; j--)
                {
                    string nzone = zoneList.NoteGroups[i].Zones[j];
                    List<Zone> zl = zoneList.Zones.Where(z => z.ZoneName == nzone).ToList();
                    if (zl.Count == 0)
                        zoneList.NoteGroups[i].Zones.Remove(nzone);
                }
            }


            // use persistent setting to expand/collapse group nodes
            foreach (TreeNode ttn in treeViewZones.Nodes)
            {
                if (ttn.Level == NodeLevels.Group)
                {
                    NoteGroup ng = zoneList.NoteGroups[ttn.Text];
                    if(ng != null)
                    {
                        if (ng.Collapsed)
                            ttn.Collapse();
                        else
                            ttn.Expand();
                    }
                }
            }

            //try to restore previous selection
            bool repos = false;
            if (!string.IsNullOrEmpty(prevNode))
            {
                List<TreeNode> nodes = treeViewZones.FlattenTree().Where(n => n.Name == prevNode).ToList();
                if (nodes.Count > 0)
                {
                    treeViewZones.SelectedNode = nodes[0];
                    treeViewZones.SelectedNode.EnsureVisible();
                    repos = true;
                }
            }
            if(!repos)
            {
                if(treeViewZones.Nodes.Count > 0)
                    treeViewZones.SelectedNode = treeViewZones.Nodes[0];
            }
            treeViewZones.ResumeLayout();
        }

        TreeNode FindGroupNode(string name)
        {
            if(!string.IsNullOrEmpty(name))
            {
                foreach (TreeNode treeNode in treeViewZones.Nodes)
                    if (treeNode.Name == name)
                        return treeNode;
            }
            return null;
        }

        TreeNode FindZoneNode(string name)
        {
            List<TreeNode> found = treeViewZones.Nodes.Find(name, true).ToList();
            foreach(TreeNode treeNode in found)
            {
                if (treeNode.Level == NodeLevels.Zone)
                {
                    return treeNode;
                }
            }
            return null;
        }

        private void treeViewZones_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node == null)
                return;

            if (!e.Node.IsVisible)
                return;

            var font = e.Node.NodeFont ?? e.Node.TreeView.Font;

            // if treeview's HideSelection property is "True", 
            // this will always return "False" on unfocused treeview
            var selected = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;

            // keep the focused highlight if selected
            // otherwise, default colors
            if (selected)
            {
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
                // use DrawString so an & isn't replaced with an underline
                e.Graphics.DrawString(e.Node.Text, font, Brushes.White, new Point(e.Bounds.X, e.Bounds.Y));
            }
            else
            {
                e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
                e.Graphics.DrawString(e.Node.Text, font, Brushes.Black, new Point(e.Bounds.X, e.Bounds.Y));
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
            UnHighlightFound();

            if (string.IsNullOrEmpty(textBoxZoneFind.Text))
            {
                buttonZoneFindNext.Enabled = false;
            }
            else
            {
                buttonZoneFindNext.Enabled = true;
                string finding = textBoxZoneFind.Text.ToLower();
                GetAllTreeNodes(treeViewZones); //populate foundNodes in treeview order (i.e. properly sorted)
                foundNodes = foundNodes.Where(n => n.Text.ToLower().Contains(finding)).ToList();
                if (foundNodes.Count > 0)
                {
                    foundNodesIndex = 0;
                    treeViewZones.SelectedNode = foundNodes[foundNodesIndex];
                    treeViewZones.SelectedNode.EnsureVisible();
                }
                else
                    SimpleMessageBox.ShowDialog(ActGlobals.oFormActMain, $"\\b {textBoxZoneFind.Text}\\b0  Not found", "Find");
            }
        }

        private void FindNextNode()
        {
            // remove any highlight from the current RTB before we move on
            UnHighlightFound();

            if (foundNodesIndex < foundNodes.Count - 1)
            {
                foundNodesIndex++;
                treeViewZones.SelectedNode = foundNodes[foundNodesIndex];
                treeViewZones.SelectedNode.EnsureVisible();
            }
            else
                SimpleMessageBox.ShowDialog(ActGlobals.oFormActMain, $"\\b {textBoxZoneFind.Text}\\b0  Not found", "Find");

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

        private void buttonAddGroup_Click(object sender, EventArgs e)
        {
            TreeNode add = new TreeNode(newGroup);
            treeViewZones.Nodes.Add(add);
            treeViewZones.SelectedNode = add;
            add.BeginEdit();
        }

        private void buttonAddZone_Click(object sender, EventArgs e)
        {
            autoNodeName = string.Empty;
            string name = newZone;

            TreeNode grpNode = treeViewZones.SelectedNode;
            if(grpNode == null)
                grpNode = FindOrCreateDefaultGroupNode();
            else if (grpNode.Level == NodeLevels.Mob)
                grpNode = grpNode.Parent.Parent;
            else if (grpNode.Level == NodeLevels.Zone)
                grpNode = grpNode.Parent;
            if (grpNode != null && grpNode.Level == NodeLevels.Group)
            {
                // if there is a zone selection on the ACT Main tab, use it as the default
                TreeNode actNode = ActGlobals.oFormActMain.MainTreeView.SelectedNode;
                if (actNode != null)
                {
                    if (actNode.Level == NodeLevels.Group)
                    {
                        string fight = actNode.Text;
                        int dash = fight.IndexOf(" - ");
                        name = fight.Substring(0, dash);
                        Match match = reCleanActZone.Match(name);
                        if (match.Success)
                            name = match.Groups["zone"].Value.Trim();
                        autoNodeName = name; //if the user just accepts this, this is how .AfterLabledit() figures it out
                    }
                }
                TreeNode add = new TreeNode(name);
                grpNode.Nodes.Add(add);
                treeViewZones.SelectedNode = add;
                add.BeginEdit();
            }
            else
                SimpleMessageBox.Show(ActGlobals.oFormActMain, "Select a top-level Group node as the parent for the new zone.", "Select Parent");
        }

        private void buttonAddMob_Click(object sender, EventArgs e)
        {
            TreeNode zoneNode = treeViewZones.SelectedNode;
            if (zoneNode != null && zoneNode.Level >= NodeLevels.Zone)
            {
                if (zoneNode.Level == NodeLevels.Mob)
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
                        if (treeViewZones.SelectedNode.Level == NodeLevels.Mob)
                        {
                            // plugin mob is selected
                            DialogResult result = SimpleMessageBox.Show(ActGlobals.oFormActMain, $"Append \\b {autoNodeName}\\b0\\line to \\b {treeViewZones.SelectedNode.Text}\\b0 ?", "Append",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                            if (result == DialogResult.Yes)
                            {
                                Mob mob = treeViewZones.SelectedNode.Tag as Mob;
                                if(mob != null)
                                {
                                    mob.MobName += $"{nameSeparator} {autoNodeName}";
                                    treeViewZones.SelectedNode.Text = mob.MobName;
                                    treeViewZones.SelectedNode.Name = mob.MobName;
                                    BuildEnemyList();
                                    return;
                                }
                            }
                        }
                    }
                }
                
                TreeNode add = new TreeNode(name);
                // need to add the mob tag before adding the node so tree sort will work
                add.Tag = new Mob { MobName = name, KillOrder = zoneNode.Nodes.Count };
                zoneNode.Nodes.Add(add);
                treeViewZones.SelectedNode = add;
                zoneNode.Expand();
                add.BeginEdit();
            }
            else
                SimpleMessageBox.Show(ActGlobals.oFormActMain, "Select a zone node as the parent for the new mob.", "Select Zone");

        }

        private void treeViewZones_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (richEditCtrl1.rtbDoc.Modified)
            {
                if (treeViewZones.SelectedNode != null)
                {
                    // remove any highlight from the current RTB before we move on
                    UnHighlightFound();

                    // save the note for the current (about to be un-selected) node
                    SaveNote(treeViewZones.SelectedNode, true);
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
                Mob mob = null;
                NoteGroup noteGroup = null;
                int audioDelay = 5;
                int visualDelay = 5;
                TreeNode sel = treeViewZones.SelectedNode;
                if (sel != null)
                {
                    if(sel.Level == NodeLevels.Group)
                    {
                        noteGroup = sel.Tag as NoteGroup;
                        if(noteGroup != null)
                        {
                            if (!string.IsNullOrEmpty(noteGroup.Notes))
                            {
                                richEditCtrl1.rtbDoc.Rtf = noteGroup.Notes;
                                richEditCtrl1.rtbDoc.Modified = false;
                            }
                            audioDelay = noteGroup.AudioDelay;
                            visualDelay = noteGroup.VisualDelay;
                        }
                    }
                    else if (sel.Level == NodeLevels.Zone)
                    {
                        zone = sel.Tag as Zone;
                        if (zone != null)
                        {
                            if (!string.IsNullOrEmpty(zone.Notes))
                            {
                                richEditCtrl1.rtbDoc.Rtf = zone.Notes;
                                richEditCtrl1.rtbDoc.Modified = false;
                            }
                            audioDelay = zone.AudioDelay;
                            visualDelay = zone.VisualDelay;
                        }
                    }
                    else if (sel.Level == NodeLevels.Mob)
                    {
                        TreeNode parent = sel.Parent;
                        zone = parent.Tag as Zone;
                        if (zone != null)
                        {
                            mob = sel.Tag as Mob;
                            if (mob != null)
                            {
                                if (!string.IsNullOrEmpty(mob.Notes))
                                {
                                    richEditCtrl1.rtbDoc.Rtf = mob.Notes;
                                    richEditCtrl1.rtbDoc.Modified = false;
                                }
                                audioDelay = mob.AudioDelay;
                                visualDelay = mob.VisualDelay;
                            }
                        }
                    }

                    Zone.PasteType pasteType = Zone.PasteType.Append;
                    if (zone != null)
                        pasteType = zone.Paste;
                    if (noteGroup != null)
                        pasteType = noteGroup.Paste;
                    if(zone != null || noteGroup != null)
                    {
                        switch(pasteType)
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
                    string text = richEditCtrl1.rtbDoc.Text;
                    int stopAt = text.IndexOf(pastePrefix);
                    if (stopAt == -1)
                        stopAt = text.Length;
                    else
                    {
                        hasPaste = true;
                        buttonCompare.Enabled = true;
                    }

                    if (!doNotAnnounce || debugMode)
                    {
                        // search for alert tags
                        string header = String.Empty;
                        if (mob != null)
                            header = mob.MobName;
                        else if(zone != null)
                            header = zone.ZoneName;
                        else if (noteGroup != null)
                            header = noteGroup.GroupName;
                        StringBuilder audioAlerts = new StringBuilder();
                        List<string> popAlerts = new List<string>();
                        // use a separate RTB to hide the color search process from view
                        using (RichTextBox rtb = new RichTextBox())
                        {
                            rtb.Rtf = richEditCtrl1.rtbDoc.Rtf;
                            int audioStart = -1;
                            int audioLen = 0;
                            int visualStart = -1;
                            int visualLen = 0;
                            bool isLastChar = false;
                            bool isAudioColor = false;
                            bool isVisualColor = false;
                            for (int i = 0; i < stopAt; i++)
                            {
                                rtb.Select(i, 1);
                                isLastChar = i == stopAt - 1;

                                if (rtb.SelectionColor == EditCtrl.audioAlertColor
                                    || rtb.SelectionColor == EditCtrl.audioVisualAlertColor)
                                {
                                    isAudioColor = true;
                                    if (audioStart < 0)
                                    {
                                        audioStart = i;
                                        audioLen = 1;
                                    }
                                    else
                                        audioLen++;
                                }
                                else
                                    isAudioColor = false;
                                if (audioLen > 0 && (!isAudioColor || isLastChar))
                                {
                                    // alert color has ended
                                    rtb.Select(audioStart, audioLen);
                                    string say = rtb.SelectedText.Trim().Trim('\n');
                                    if (!string.IsNullOrEmpty(say))
                                    {
                                        if (audioAlerts.Length == 0)
                                        {
                                            if (!string.IsNullOrEmpty(header))
                                                audioAlerts.Append(header + ", ");
                                        }
                                        else
                                            audioAlerts.Append(", ");
                                        audioAlerts.Append(say);
                                    }
                                    audioLen = 0;
                                    audioStart = -1;
                                }

                                rtb.Select(i, 1);
                                if (rtb.SelectionColor == EditCtrl.visualAlertColor
                                    || rtb.SelectionColor == EditCtrl.audioVisualAlertColor)
                                {
                                    isVisualColor = true;
                                    if (visualStart < 0)
                                    {
                                        visualStart = i;
                                        visualLen = 1;
                                    }
                                    else
                                        visualLen++;
                                }
                                else
                                    isVisualColor = false;
                                if (visualLen > 0 && (!isVisualColor || isLastChar))
                                {
                                    // we have reached the end of the alert color
                                    rtb.Select(visualStart, visualLen);
                                    string say = rtb.SelectedText.Trim().Trim('\n');
                                    if (!string.IsNullOrEmpty(say))
                                    {
                                        if (popAlerts.Count == 0)
                                        {
                                            if (!string.IsNullOrEmpty(header))
                                                popAlerts.Add(header);
                                        }
                                        popAlerts.Add(say);
                                    }
                                    visualLen = 0;
                                    visualStart = -1;
                                }
                            }
                        }

                        if (audioAlerts.Length > 0)
                        {
                            Task.Run(() =>
                            {
                                if (audioDelay > 0)
                                    Thread.Sleep(audioDelay * 1000);
                                mUiContext.Post(UiAudio, audioAlerts.ToString());
                            });
                        }
                        if (popAlerts.Count > 0)
                        {
                            Task.Run(() =>
                            {
                                if (visualDelay > 0)
                                    Thread.Sleep(visualDelay * 1000);
                                mUiContext.Post(UiPopup, popAlerts);
                            });
                        }
                        else
                        {
                            if (alertForm != null)
                            {
                                alertForm.Close();
                                alertForm = null;
                            }
                        }
                    }
                    if(doNotAnnounce && alertForm != null)
                    {
                        alertForm.Close();
                        alertForm = null;
                    }

                    if (!hasPaste)
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
                if (e.Label == null && !string.IsNullOrEmpty(autoNodeName) && e.Node.Level > NodeLevels.Group)
                    nodeName = autoNodeName;

                if (string.IsNullOrEmpty(nodeName))
                {
                    if ((e.Node.Level == NodeLevels.Group && e.Node.Text == newGroup)
                        || e.Node.Level == NodeLevels.Zone && e.Node.Text == newZone
                        || e.Node.Level == NodeLevels.Mob && e.Node.Text == newMob)
                    {
                        // we don't accept "New Group", "New Mob" or "New Zone". The user must enter something else
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

                    if (e.Node.Level == NodeLevels.Group)
                    {
                        NoteGroup newGroup = new NoteGroup { GroupName = nodeName };
                        if (zoneList.NoteGroups.Contains(newGroup))
                        {
                            SimpleMessageBox.Show(ActGlobals.oFormActMain, "Duplicate Group Names are not allowed.", "Invalid entry",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Node.Remove();
                            autoNodeName = String.Empty;
                            removed = true;
                        }
                        else
                        {
                            // group node
                            if (e.Node.Tag == null)
                            {
                                // new group
                                zoneList.NoteGroups.Add(newGroup);
                                e.Node.Tag = newGroup;
                                e.Node.Name = nodeName;
                            }
                            else
                            {
                                // editing an existing group
                                NoteGroup noteGroup = e.Node.Tag as NoteGroup;
                                if (noteGroup != null)
                                    noteGroup.GroupName = nodeName;
                            }
                        }
                        e.Node.EndEdit(true);
                    }
                    else if (e.Node.Level == NodeLevels.Zone)
                    {
                        NoteGroup ng = e.Node.Parent.Tag as NoteGroup;
                        Zone newZone = new Zone { ZoneName = nodeName };
                        if (zoneList.Zones.Contains(newZone))
                        {
                            SimpleMessageBox.Show(ActGlobals.oFormActMain, "Duplicate Zone Names are not allowed.", "Invalid entry",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                                if (ng != null)
                                    ng.Zones.Add(nodeName);
                            }
                            else
                            {
                                // editing an existing zone
                                Zone zone = e.Node.Tag as Zone;
                                if (zone != null)
                                {
                                    if (ng != null)
                                    {
                                        ng.Zones.Remove(zone.ZoneName);
                                        ng.Zones.Add(nodeName);
                                    }
                                    zone.ZoneName = nodeName;
                                }
                            }
                        }
                        e.Node.EndEdit(true);
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
                                e.Node.Remove();
                                autoNodeName = String.Empty;
                                removed = true;
                            }
                            else
                            {
                                Mob existing = zone.GetMob(e.Node.Text);
                                if (existing != null)
                                {
                                    //renamed node
                                    existing.MobName = nodeName;
                                }
                                else
                                {
                                    // new mob
                                    zone.Mobs.Add(newMob);
                                    e.Node.Tag = newMob;
                                    e.Node.Name = nodeName;
                                }
                            }
                        }
                        e.Node.EndEdit(true);
                    }
                    if (!removed)
                    {
                        treeViewZones.SelectedNode = e.Node;
                        BuildEnemyList();
                    }
                }
                SaveSettings();
            }
        }

        void UiAudio(object o)
        {
            string say = o as string;
            if(!string.IsNullOrEmpty(say))
            {
                ActGlobals.oFormActMain.TTS(say);
            }
        }

        void UiPopup(object o)
        {
            List<string> popAlerts = o as List<string>;
            if(popAlerts != null)
            {
                if (alertForm != null)
                {
                    alertForm.Close();
                    alertForm = null;
                }
                alertForm = new AlertForm();
                alertForm.Alerts = popAlerts;
                if (zoneList.AlertX == 0 && zoneList.AlertY == 0)
                {
                    // if it has never been positioned, put it center screen
                    Size size = Screen.PrimaryScreen.WorkingArea.Size;
                    zoneList.AlertX = size.Width / 2 - alertForm.Width / 2;
                    zoneList.AlertY = size.Height / 2 - alertForm.Height / 2;
                }
                if (zoneList.AlertWidth != 0)
                    alertForm.Width = zoneList.AlertWidth;
                alertForm.Location = new Point(zoneList.AlertX, zoneList.AlertY);
                alertForm.FormMoved += AlertForm_FormMoved;
                alertForm.Show();
            }
        }

        private void AlertForm_FormMoved(object sender, EventArgs e)
        {
            zoneList.AlertX = alertForm.Location.X;
            zoneList.AlertY = alertForm.Location.Y;
            zoneList.AlertWidth = alertForm.Width;
        }

        private void treeViewZones_ItemDrag(object sender, ItemDragEventArgs e)
        {
            TreeNode node = e.Item as TreeNode;
            if (node != null)
            {
                if (node.Level == NodeLevels.Group)
                    return; // can't drag groups

                if (treeViewZones.SelectedNode == node && richEditCtrl1.rtbDoc.Modified)
                {
                    SaveNote(node, false);
                }
            }
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treeViewZones_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
        }

        private void treeViewZones_DragOver(object sender, DragEventArgs e)
        {
            TreeView tv = sender as TreeView;

            // Retrieve the client coordinates of the drop location.
            Point targetPoint = tv.PointToClient(new Point(e.X, e.Y));

            // Retrieve the node at the drop location.
            TreeNode targetNode = tv.GetNodeAt(targetPoint);

            // Retrieve the node that was dragged.
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            // need both nodes and they can't be the same
            if (draggedNode == null || targetNode == null || draggedNode.Equals(targetNode))
                e.Effect = DragDropEffects.None;

            // ok if dragging a zone to a different group
            else if (targetNode.Level == NodeLevels.Group && draggedNode.Level == NodeLevels.Zone && !draggedNode.Parent.Equals(targetNode))
                e.Effect = DragDropEffects.Move;

            // ok if dragging a mob within a zone
            else if ((targetNode.Level == NodeLevels.Mob && draggedNode.Level == NodeLevels.Mob && draggedNode.Parent.Equals(targetNode.Parent))
                || (targetNode.Level == NodeLevels.Zone && draggedNode.Level == NodeLevels.Mob && draggedNode.Parent.Equals(targetNode)))
                e.Effect = DragDropEffects.Move;

            else
                e.Effect = DragDropEffects.None;

            if ((targetPoint.Y + 20) > tv.Height)
            {
                // scroll down
                SendMessage(tv.Handle, WM_VSCROLL, (IntPtr)1, (IntPtr)0);
            }
            else if (targetPoint.Y < 20)
            {
                // scroll up
                SendMessage(tv.Handle, WM_VSCROLL, (IntPtr)0, (IntPtr)0);
            }

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

            // dragging a zone to a different group
            if (targetNode.Level == NodeLevels.Group && draggedNode.Level == NodeLevels.Zone && !draggedNode.Parent.Equals(targetNode))
            {
                Zone zone = draggedNode.Tag as Zone;
                NoteGroup ng = targetNode.Tag as NoteGroup;
                if(zone != null && ng != null)
                {
                    NoteGroup oldGrp = zoneList.NoteGroups[zoneList.NoteGroups.GetGroupName(zone.ZoneName)];
                    oldGrp.Zones.Remove(zone.ZoneName);

                    ng.Zones.Add(zone.ZoneName);

                    // Remove the node from its current location in the tree
                    draggedNode.Remove();
                    // and add it to the drop location.
                    targetNode.Nodes.Add(draggedNode);
                }
            }

            // dragging mob within the same zone
            else if ((targetNode.Level == NodeLevels.Mob && draggedNode.Level == NodeLevels.Mob && draggedNode.Parent.Equals(targetNode.Parent))
            || (targetNode.Level == NodeLevels.Zone && draggedNode.Level == NodeLevels.Mob && draggedNode.Parent.Equals(targetNode)))
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
                            treeViewZones.Sort();
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
                TreeNode zoneNode = FindZoneNode(cleanZone);
                if(zoneNode != null)
                {
                    //Zone zone = zoneNodes[0].Tag as Zone;
                    Zone zone = zoneNode.Tag as Zone;
                    if(zone != null)
                    {
                        // see if we have a mob that matches the passed name
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
                                TreeNode[] mobNodes = zoneNode.Nodes.Find(next.MobName, false);
                                if (mobNodes.Length > 0)
                                {
                                    // select the next mob in the kill order
                                    doNotAnnounce = false;
                                    treeViewZones.SelectedNode = mobNodes[0];
                                    treeViewZones.SelectedNode.EnsureVisible();
                                    doNotAnnounce = true;
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

        private void treeViewZones_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if(e.Node.Level == NodeLevels.Group)
            {
                NoteGroup grp = zoneList.NoteGroups[e.Node.Text];
                if(grp != null)
                {
                    grp.Collapsed = true;
                }
            }
        }

        private void treeViewZones_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level == NodeLevels.Group)
            {
                NoteGroup grp = zoneList.NoteGroups[e.Node.Text];
                if (grp != null)
                {
                    grp.Collapsed = false;
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
            if (clickedZoneNode != null)
            {
                if(clickedZoneNode.Level == NodeLevels.Group)
                {
                    deleteToolStripMenuItem.Text = "Delete Group";
                    copyEntireZoneToXMLToolStripMenuItem.Visible = true;
                    copyEntireZoneToXMLToolStripMenuItem.Text = "Copy Entire Group to XML";
                    skipKillCheckToolStripMenuItem.Visible = false;
                }
                else if (clickedZoneNode.Level == NodeLevels.Mob)
                {
                    // mob menu
                    deleteToolStripMenuItem.Text = "Delete Mob";
                    copyEntireZoneToXMLToolStripMenuItem.Visible = false;
                    skipKillCheckToolStripMenuItem.Visible = false;
                }
                else
                {
                    // zone menu
                    deleteToolStripMenuItem.Text = "Delete Zone";
                    copyEntireZoneToXMLToolStripMenuItem.Visible = true;
                    copyEntireZoneToXMLToolStripMenuItem.Text = "Copy Entire Zone to XML";
                    skipKillCheckToolStripMenuItem.Visible = true;
                    Zone zone = clickedZoneNode.Tag as Zone;
                    if (zone != null)
                        skipKillCheckToolStripMenuItem.Checked = zone.SkipKillCheck;
                }
            }
        }

        private void copyToXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            xmlShareMenuClick(false);
        }

        private void copyEntireZoneToXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            xmlShareMenuClick(true);
        }

        private XmlCopyForm xmlShareMenuClick(bool deep, bool showForm=true)
        {
            if (clickedZoneNode != null)
            {
                XmlShareDepth depth = XmlShareDepth.MobOnly; // set default just to satisfy C# lint

                if (clickedZoneNode.Equals(treeViewZones.SelectedNode))
                {
                    SaveNote(clickedZoneNode, true);
                }

                NoteGroup noteGroup = null;
                List<Zone> zones = new List<Zone>();
                Mob mob = null;
                bool haveNote = false;

                if(clickedZoneNode.Level == NodeLevels.Group)
                {
                    noteGroup = clickedZoneNode.Tag as NoteGroup;
                    haveNote = noteGroup.Notes != null;
                    depth = deep ? XmlShareDepth.GroupDeep : XmlShareDepth.GroupOnly;
                    if (deep)
                    {
                        depth = XmlShareDepth.GroupDeep;
                        foreach (TreeNode treeNode in clickedZoneNode.Nodes)
                        {
                            Zone zone = treeNode.Tag as Zone;
                            if (zone != null)
                            {
                                bool zoneNote = ZoneHasNotes(zone);
                                haveNote |= zoneNote;
                                if (zoneNote)
                                    zones.Add(zone);
                            }
                        }
                    }
                }
                else if (clickedZoneNode.Level == NodeLevels.Mob)
                {
                    noteGroup = clickedZoneNode.Parent.Parent.Tag as NoteGroup;
                    Zone zone = clickedZoneNode.Parent.Tag as Zone;
                    depth = XmlShareDepth.MobOnly;
                    zones.Add(zone);
                    mob = clickedZoneNode.Tag as Mob;
                    haveNote = mob.Notes != null;
                }
                else if (clickedZoneNode.Level == NodeLevels.Zone)
                {
                    noteGroup = clickedZoneNode.Parent.Tag as NoteGroup;
                    Zone zone = clickedZoneNode.Tag as Zone;
                    depth = deep ? XmlShareDepth.ZoneDeep : XmlShareDepth.ZoneOnly;
                    zones.Add(zone);
                    if (deep)
                        haveNote = ZoneHasNotes(zone);
                    else
                        haveNote = zone.Notes != null;
                }

                if (!haveNote)
                    SimpleMessageBox.Show(ActGlobals.oFormActMain, "Selected item does not have any notes to copy.",
                        "Empty note", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    XmlCopyForm form = new XmlCopyForm(noteGroup, zones, mob, zoneList.CompressImages, depth);
                    form.CompressCheckChanged += Form_CompressCheckChanged;
                    if (showForm)
                    {
                        form.Show();
                        PositionChildForm(form, clickedZonePoint);
                        form.TopMost = true;
                    }
                    return form;
                }
            }
            return null;
        }

        private bool ZoneHasNotes(Zone zone)
        {
            bool haveNote = zone.Notes != null;
            if (!haveNote)
            {
                for (int i = 0; i < zone.Mobs.Count; i++)
                {
                    if (zone.Mobs[i].Notes != null)
                    {
                        if (zone.Mobs[i].Notes.Length > 0)
                        {
                            haveNote = true;
                            break;
                        }
                    }
                }
            }
            return haveNote;
        }

        private void Form_CompressCheckChanged(object sender, EventArgs e)
        {
            CompressCheckChangedEventArgs arg = e as CompressCheckChangedEventArgs;
            if(arg != null)
            {
                zoneList.CompressImages = arg.isChecked;
            }
        }

        private TreeNode FindOrCreateDefaultGroupNode()
        {
            NoteGroup dng = zoneList.NoteGroups[DefaultGroupName];
            TreeNode tgn = FindGroupNode(DefaultGroupName);
            if (dng == null)
            {
                dng = zoneList.NoteGroups.AddGroup(DefaultGroupName);
            }
            if (tgn == null)
            {
                tgn = new TreeNode(DefaultGroupName);
                tgn.Name = DefaultGroupName;
                treeViewZones.Nodes.Add(tgn);
            }
            tgn.Tag = dng;
            return tgn;
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (clickedZoneNode != null)
                {
                    if (clickedZoneNode.Level == NodeLevels.Group)
                    {
                        // delete group, move the zones to the default group
                        NoteGroup ng = clickedZoneNode.Tag as NoteGroup;
                        if (ng != null)
                        {
                            DialogResult result = DialogResult.Yes;
                            if (ng.Zones.Count > 0)
                                result = SimpleMessageBox.Show(ActGlobals.oFormActMain, "Detete group \\b\\line " + ng.GroupName + "\\b0\\line and move all its zones to " + DefaultGroupName, "Delete Group",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                            if (result == DialogResult.Yes)
                            {
                                if(ng.Zones.Count > 0)
                                {
                                    // move the zones to the default group
                                    TreeNode tgn = FindOrCreateDefaultGroupNode();
                                    NoteGroup dng = tgn.Tag as NoteGroup;
                                    dng.Zones.AddRange(ng.Zones);
                                    if (tgn != null)
                                    {
                                        int zcount = clickedZoneNode.Nodes.Count;
                                        for (int i = zcount - 1; i >= 0; i--)
                                        {
                                            TreeNode tn = clickedZoneNode.Nodes[i];
                                            treeViewZones.Nodes.Remove(tn);
                                            tgn.Nodes.Add(tn);
                                        }
                                    }
                                }
                                // remove the old group
                                ng.Zones.Clear();
                                zoneList.NoteGroups.Remove(ng);
                                treeViewZones.Nodes.Remove(clickedZoneNode);
                            }
                        }
                    }
                    else if (clickedZoneNode.Level == NodeLevels.Zone)
                    {
                        // delete zone
                        string category = clickedZoneNode.Text;
                        if (SimpleMessageBox.Show(ActGlobals.oFormActMain, "Delete zone \\b\\line " + category + "\\b0\\line  and all its mobs and notes?", "Are you sure?",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                        {
                            Zone zone = clickedZoneNode.Tag as Zone;
                            NoteGroup ng = clickedZoneNode.Parent.Tag as NoteGroup;
                            if (zone != null)
                            {
                                if (ng != null)
                                    ng.Zones.Remove(zone.ZoneName);
                                zoneList.Zones.Remove(zone);
                                treeViewZones.Nodes.Remove(clickedZoneNode);
                            }
                        }
                    }
                    else if (clickedZoneNode.Level == NodeLevels.Mob)
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
                if (firstExport)
                {
                    saveFileDialogRtf.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    firstExport = false;
                }
                else
                    saveFileDialogRtf.InitialDirectory = null; // will use "recents"
                saveFileDialogRtf.FileName = clickedZoneNode.Text.Replace(":", String.Empty);
                saveFileDialogRtf.RestoreDirectory = true;
                if (saveFileDialogRtf.ShowDialog() == DialogResult.OK)
                {
                    RichTextBox rtb = new RichTextBox();

                    // header
                    if (clickedZoneNode.Level == NodeLevels.Group)
                        rtb.Text = clickedZoneNode.Text;
                    else if(clickedZoneNode.Level == NodeLevels.Zone)
                        rtb.Text = clickedZoneNode.Text + "\n\n";
                    else if(clickedZoneNode.Level == NodeLevels.Mob)
                        rtb.Text = clickedZoneNode.Parent.Text + "\n\n";
                    rtb.Select(0, rtb.TextLength);
                    Font currentFont = rtb.SelectionFont;
                    FontStyle newFontStyle;
                    newFontStyle = rtb.SelectionFont.Style | FontStyle.Bold | FontStyle.Underline;
                    rtb.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newFontStyle);

                    if(clickedZoneNode.Level == NodeLevels.Group)
                    {
                        // export the group and its children
                        ExportGroupRtf(rtb, clickedZoneNode);
                    }
                    else if (clickedZoneNode.Level == NodeLevels.Zone)
                    {
                        // export the zone and its mobs
                        Zone zone = clickedZoneNode.Tag as Zone;
                        if (zone != null)
                        {
                            ExportZoneRtf(rtb, zone);
                        }
                    }
                    else if (clickedZoneNode.Level == NodeLevels.Mob)
                    {
                        // mob note
                        Mob mob = clickedZoneNode.Tag as Mob;
                        if (mob != null)
                        {
                            ExportMobRtf(rtb, mob);
                        }
                    }
                    try
                    {
                        File.WriteAllText(saveFileDialogRtf.FileName, rtb.Rtf);
                    }
                    catch (Exception exrtfex)
                    {
                        SimpleMessageBox.Show(ActGlobals.oFormActMain, $"Could not write file {saveFileDialogRtf.FileName}: {exrtfex.Message}");
                    }
                }
            }
        }

        private void ExportMobRtf(RichTextBox rtb, Mob mob)
        {
            if (mob != null)
            {
                rtb.Select(rtb.TextLength, 0);
                rtb.SelectedRtf = @"{\rtf1\ansi\pard\b " + mob.MobName + @"\b0\par}";

                rtb.Select(rtb.TextLength, 0);
                rtb.SelectedRtf = @"{\rtf1\ansi\par " + mob.Notes + @"\par}";
            }
        }

        private void ExportZoneRtf(RichTextBox rtb, Zone zone)
        {
            if (zone != null)
            {
                rtb.Select(rtb.TextLength, 0);
                rtb.SelectedRtf = @"{\rtf1\ansi\pard\b " + zone.ZoneName + @"\b0\par}";

                // zone note
                rtb.Select(rtb.TextLength, 0);
                if (zone.Notes != null)
                    rtb.SelectedRtf = @"{\rtf1\ansi\par\pard " + zone.Notes + @"\par}";
                else
                    rtb.SelectedRtf = @"{\rtf1\ansi\par}";

                // mobs
                foreach (Mob mob in zone.Mobs)
                {
                    ExportMobRtf(rtb, mob);
                }
            }

        }

        private void ExportGroupRtf(RichTextBox rtb, TreeNode groupNode)
        {
            // group note
            NoteGroup ng = groupNode.Tag as NoteGroup;
            if (ng != null)
            {
                rtb.Select(rtb.TextLength, 0);
                rtb.SelectedRtf = @"{\rtf1\ansi\par\pard " + ng.Notes + @"\par}";
            }

            foreach (TreeNode tn in groupNode.Nodes)
            {
                Zone zone = tn.Tag as Zone;
                if(zone != null)
                    ExportZoneRtf(rtb, zone);
            }
        }

        private void importRTFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (clickedZoneNode != null)
            {
                if (firstExport)
                {
                    saveFileDialogRtf.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    firstExport = false;
                }
                else
                    saveFileDialogRtf.InitialDirectory = null; // will use "recents"

                if (openFileDialogRtf.ShowDialog() == DialogResult.OK)
                {
                    RichTextBox rtb = new RichTextBox();
                    string existingNote = string.Empty;
                    NoteGroup ng = null;
                    Zone zone = null;
                    Mob mob = null;
                    if (clickedZoneNode.Level == NodeLevels.Group)
                    {
                        ng = clickedZoneNode.Tag as NoteGroup;
                        existingNote = ng.Notes;
                    }
                    else if(clickedZoneNode.Level == NodeLevels.Zone)
                    {
                        zone = clickedZoneNode.Tag as Zone;
                        existingNote = zone.Notes;
                    }
                    else if(clickedZoneNode.Level == NodeLevels.Mob)
                    {
                        mob = clickedZoneNode.Tag as Mob;
                        existingNote = mob.Notes;
                    }

                    try
                    {
                        string incomingNote = File.ReadAllText(openFileDialogRtf.FileName);
                        if (!string.IsNullOrEmpty(existingNote))
                        {
                            rtb.Rtf = existingNote;
                        }
                        rtb.Select(rtb.TextLength, 0);
                        rtb.SelectedRtf = incomingNote;
                    }
                    catch (Exception rdrtfex)
                    {
                        SimpleMessageBox.Show(ActGlobals.oFormActMain, $"Could not read {openFileDialogRtf.FileName}: {rdrtfex.Message}", "File Error");
                    }

                    if (ng != null)
                        ng.Notes = rtb.Rtf;
                    else if (zone != null)
                        zone.Notes = rtb.Rtf;
                    else if (mob != null)
                        mob.Notes = rtb.Rtf;

                    if (treeViewZones.SelectedNode == clickedZoneNode)
                        treeViewZones.SelectedNode = null; //force an update
                    clickedZoneNode.EnsureVisible();
                    treeViewZones.SelectedNode = clickedZoneNode;
                }
            }
        }

        private void setAlertDelaysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (clickedZoneNode != null)
            {
                DelaysForm delaysForm = new DelaysForm();
                if (clickedZoneNode.Level == NodeLevels.Group)
                {
                    delaysForm.Text = "Zone Group Alert Delays";
                    NoteGroup ng = clickedZoneNode.Tag as NoteGroup;
                    if (ng != null)
                    {
                        delaysForm.audioDelay = ng.AudioDelay;
                        delaysForm.visualDelay = ng.VisualDelay;
                        if (delaysForm.ShowDialog() == DialogResult.OK)
                        {
                            ng.AudioDelay = delaysForm.audioDelay;
                            ng.VisualDelay = delaysForm.visualDelay;
                        }
                    }
                }
                if (clickedZoneNode.Level == NodeLevels.Zone)
                {
                    delaysForm.Text = "Zone Note Alert Delays";
                    Zone zone = clickedZoneNode.Tag as Zone;
                    if(zone != null)
                    {
                        delaysForm.audioDelay = zone.AudioDelay;
                        delaysForm.visualDelay = zone.VisualDelay;
                        if(delaysForm.ShowDialog() == DialogResult.OK)
                        {
                            zone.AudioDelay = delaysForm.audioDelay;
                            zone.VisualDelay = delaysForm.visualDelay;
                        }
                    }
                }
                else if (clickedZoneNode.Level == NodeLevels.Mob)
                {
                    delaysForm.Text = "Mob Note Alert Delays";
                    Mob mob = clickedZoneNode.Tag as Mob;
                    if(mob != null)
                    {
                        delaysForm.audioDelay = mob.AudioDelay;
                        delaysForm.visualDelay = mob.VisualDelay;
                        if(delaysForm.ShowDialog() == DialogResult.OK)
                        {
                            mob.AudioDelay = delaysForm.audioDelay;
                            mob.VisualDelay = delaysForm.visualDelay;
                        }
                    }
                }
            }

        }

        private void skipKillCheckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Zone zone = clickedZoneNode.Tag as Zone;
            if (zone != null)
            {
                bool now = !zone.SkipKillCheck;
                zone.SkipKillCheck = now;
                skipKillCheckToolStripMenuItem.Checked = now;
                skipKillCheck = now;
            }
        }

        private void exportZIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XmlCopyForm form = xmlShareMenuClick(true, false);
            if (form != null)
            {
                int files = form.BuildMacroFiles();
                if(files > 0)
                {
                    List<string> fileNames = new List<string>();
                    string gamePath = ActGlobals.oFormActMain.GameMacroFolder;
                    for (int i = 1; i <= files; i++)
                    {
                        string file = string.Format(XmlCopyForm.doFileName, i);
                        fileNames.Add(Path.Combine(gamePath, file));
                    }
                    if(fileNames.Count > 0)
                    {
                        saveFileDialogZip.InitialDirectory = GetDownloadsPath();
                        saveFileDialogZip.FileName = string.Empty;
                        if (saveFileDialogZip.ShowDialog() == DialogResult.OK)
                        {
                            string zipFilePath = saveFileDialogZip.FileName;
                            AddFilesToZip(zipFilePath, fileNames);
                        }
                    }
                }
            }
        }

        private static string GetDownloadsPath()
        {
            if (Environment.OSVersion.Version.Major < 6)
            {
                return "";
            }

            IntPtr pathPtr = IntPtr.Zero;

            try
            {
                SHGetKnownFolderPath(ref FolderDownloads, 0, IntPtr.Zero, out pathPtr);
                return Marshal.PtrToStringUni(pathPtr);
            }
            finally
            {
                Marshal.FreeCoTaskMem(pathPtr);
            }
        }

        private static void AddFilesToZip(string zipFilePath, List<string> fileNames)
        {
            try
            {
                using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.OpenOrCreate))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        foreach (var fileName in fileNames)
                        {
                            if (File.Exists(fileName))
                            {
                                var entryName = Path.GetFileName(fileName);
                                var entry = archive.CreateEntry(entryName);
                                using (var entryStream = entry.Open())
                                {
                                    using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }
                                }
                                Debug.WriteLine($"Added {fileName} to {zipFilePath}");
                            }
                            else
                            {
                                SimpleMessageBox.Show(ActGlobals.oFormActMain, $"File not found: {fileName}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                SimpleMessageBox.Show(ActGlobals.oFormActMain, "Zip file exception:\\line" + ex.Message);
            }
        }

        private void importZIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialogZip.InitialDirectory = GetDownloadsPath();
            openFileDialogZip.FileName = string.Empty;
            if(openFileDialogZip.ShowDialog() == DialogResult.OK)
            {
                string zipFilePath = openFileDialogZip.FileName;
                if (IsZipValid(zipFilePath))
                {
                    Dictionary<string, string> fileContents = DecompressZipToStrings(zipFilePath);
                    foreach (var entry in fileContents)
                    {
                        string[] lines = entry.Value.Split(
                            new string[] { "\r\n", "\r", "\n" },
                            StringSplitOptions.None
                        );
                        foreach (string line in lines)
                        {
                            if (string.IsNullOrEmpty(line)) continue;

                            try
                            {
                                string xline = "<root>\n" + line + "\n</root>";
                                Dictionary<string, string> elems = ConvertXmlToDictionary(xline);
                                XmlSnippetEventArgs args = new XmlSnippetEventArgs(XmlSnippetType, elems, line);
                                OFormActMain_XmlSnippetAdded(null, args);
                            }
                            catch
                            {
                                SimpleMessageBox.Show(ActGlobals.oFormActMain, "Problem unzipping the file");
                            }
                        }
                    }
                }
                else
                    SimpleMessageBox.Show(ActGlobals.oFormActMain, $"{zipFilePath} is not a Notes zip file");
            }
        }

        private static bool IsZipValid(string path)
        {
            try
            {
                using (var zipFile = ZipFile.OpenRead(path))
                {
                    if(zipFile.Entries.Count > 0)
                    {
                        if (zipFile.Entries[0].FullName.StartsWith("note-macro"))
                            return true;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static Dictionary<string, string> ConvertXmlToDictionary(string xmlString)
        {
            var dictionary = new Dictionary<string, string>();
            XElement root = XElement.Parse(xmlString);

            foreach (XElement element in root.Elements())
            {
                foreach (XAttribute attribute in element.Attributes())
                {
                    dictionary[attribute.Name.ToString()] = attribute.Value;
                }
            }

            return dictionary;
        }

        private static Dictionary<string, string> DecompressZipToStrings(string zipFilePath)
        {
            var fileContents = new Dictionary<string, string>();

            try
            {
                using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Open))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            using (StreamReader reader = new StreamReader(entry.Open()))
                            {
                                string content = reader.ReadToEnd();
                                fileContents[entry.FullName] = content;
                                Debug.WriteLine($"Read contents of {entry.FullName}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SimpleMessageBox.Show(ActGlobals.oFormActMain, "File exepction:\\line " + ex.Message);
            }
            return fileContents;
        }

        #endregion Tree Context Menu


        #endregion Tree Panel

        #region Editor Panel

        private void Plugin_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                if (currentZone != ActGlobals.oFormActMain.CurrentZone)
                    ZoneTimer_Elapsed(null, null); // unlikely to get here, but just in case

                GetWhiteList(); // to show/hide the Accept radio button

                if (neverBeenVisible)
                {
                    //set the splitter only on the first time shown
                    if (zoneList.SplitterLoc > 0)
                        splitContainer1.SplitterDistance = zoneList.SplitterLoc;

                    VersionUpdateNotice();

                    neverBeenVisible = false;
                }
            }
        }

        private void VersionUpdateNotice()
        {
            Version configVersion = new Version(zoneList.Version);
            bool needSep = configVersion <= new Version("1.3.0.0");
            CommaNamesForm commaNamesForm = new CommaNamesForm();
            bool needForm = false;
            foreach (Zone zone in zoneList.Zones)
            {
                if (needSep)
                {
                    string groupname = zoneList.NoteGroups.GetGroupName(zone.ZoneName);
                    // find the old name separator
                    foreach (Mob mob in zone.Mobs)
                    {
                        if (mob.MobName.Contains(","))
                        {
                            commaNamesForm.AddName(groupname, zone.ZoneName, mob.MobName);
                            needForm = true;
                        }
                    }
                }
            }
            if (needSep)
            {
                Version localVersion = this.GetType().Assembly.GetName().Version;
                zoneList.Version = localVersion.ToString();
                if (needForm)
                {
                    commaNamesForm.Show(ActGlobals.oFormActMain);
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
                        string searchText = textBoxEditFind.Text.ToLower();
                        bool found = FindInNote(treeViewZones.SelectedNode, searchText, richEditCtrl1.rtbDoc.Rtf);
                        if (!found)
                        {
                            UnHighlightFound();
                            SimpleMessageBox.ShowDialog(ActGlobals.oFormActMain, $"\\b {searchText}\\b0  Not found", "Find Text");
                        }
                    }
                }
                else
                {
                    buttonFindNext.Enabled = true;
                    foundZoneIndex = 0;
                    GetAllTreeNodes(treeViewZones); //populate foundNodes in treeview order (i.e. properly sorted)
                    FindNextText();
                }
            }
            else
            {
                buttonFindNext.Enabled = false;
                UnHighlightFound();
            }
        }

        private void AddNodeRecursive(TreeNode treeNode)
        {
            foundNodes.Add(treeNode);

            // Visit each node recursively.  
            foreach (TreeNode tn in treeNode.Nodes)
            {
                AddNodeRecursive(tn);
            }
        }

        private void GetAllTreeNodes(TreeView treeView)
        {
            foundNodes.Clear();
            foreach (TreeNode n in treeView.Nodes)
            {
                AddNodeRecursive(n);
            }
        }

        private void FindNextText()
        {
            string searchText = textBoxEditFind.Text.ToLower();
            bool found = false;
            for (; foundZoneIndex < foundNodes.Count && !found; foundZoneIndex++)
            {
                TreeNode node = foundNodes[foundZoneIndex];

                if(node.Level == NodeLevels.Group)
                {
                    NoteGroup ng = node.Tag as NoteGroup;
                    if(ng != null && !string.IsNullOrEmpty(ng.Notes))
                    {
                        found = FindInNote(node, searchText, ng.Notes);
                    }
                }
                else if(node.Level == NodeLevels.Zone)
                {
                    Zone zone = node.Tag as Zone;
                    // search in the zone note
                    if(zone != null && !string.IsNullOrEmpty(zone.Notes))
                    {
                        found = FindInNote(node, searchText, zone.Notes);
                    }
                }
                else if(node.Level == NodeLevels.Mob)
                {
                    // search in this zone's mob notes
                    Mob mob = node.Tag as Mob;
                    if(mob != null && !string.IsNullOrEmpty(mob.Notes))
                    {
                        found = FindInNote(node, searchText, mob.Notes);
                    }
                }
            }
            if (!found)
            {
                UnHighlightFound();
                SimpleMessageBox.ShowDialog(ActGlobals.oFormActMain, $"\\b {searchText}\\b0  Not found", "Find");
            }
        }

        private bool FindInNote(TreeNode node, string searchText, string note)
        {
            bool found = false;
            int startIndex = 0;

            // use a local RTB since the note we are searching may not be visible (yet)
            RichTextBox rtb = new RichTextBox();
            rtb.Rtf = note;

            UnHighlightFound();

            //we only need to continue if the text itself contains the search term
            if (rtb.Find(searchText, 0, RichTextBoxFinds.None) == -1)
                return false;

            bool sameNode = treeViewZones.SelectedNode == node;
            if (sameNode)
            {
                // load the un-highlighted
                rtb.Rtf = richEditCtrl1.rtbDoc.Rtf;
            }

            CatalogHighlights(rtb);

            while (startIndex < rtb.TextLength)
            {
                int wordstartIndex = rtb.Find(searchText, startIndex, RichTextBoxFinds.None);
                if (wordstartIndex != -1)
                {
                    found = true;
                    rtb.SelectionStart = wordstartIndex;
                    rtb.SelectionLength = searchText.Length;
                    rtb.SelectionBackColor = foundBackground;
                }
                else
                    break;
                startIndex = wordstartIndex + searchText.Length;
            }

            if (found)
            {
                if(sameNode)
                {
                    richEditCtrl1.rtbDoc.SuspendPainting();
                    richEditCtrl1.rtbDoc.Rtf = rtb.Rtf;
                    richEditCtrl1.rtbDoc.ResumePainting();
                }
                else
                {
                    UnHighlightFound(); //previous node
                    richEditCtrl1.rtbDoc.SuspendPainting();
                    treeViewZones.SelectedNode = node;
                    treeViewZones.SelectedNode.EnsureVisible();
                    CatalogHighlights(richEditCtrl1.rtbDoc); // new node
                    richEditCtrl1.rtbDoc.Rtf = rtb.Rtf;
                    richEditCtrl1.rtbDoc.ResumePainting();
                    // just scroll to the top
                    richEditCtrl1.rtbDoc.SelectionStart = 0;
                    richEditCtrl1.rtbDoc.SelectionLength = 0;
                    richEditCtrl1.rtbDoc.ScrollToCaret();
                }
            }

            return found;
        }

        private void CatalogHighlights(RichTextBox rtb)
        {
            // catalog any non-white backgrounds
            highLightRanges.Clear();
            int i = 0;
            rtb.SelectionLength = 1;
            while (i < rtb.Text.Length)
            {
                rtb.SelectionStart = i;
                if (rtb.SelectionBackColor != Color.White)
                {
                    Color backColor = rtb.SelectionBackColor;
                    int j = i + 1;
                    while (j < rtb.Text.Length)
                    {
                        rtb.SelectionStart = j;
                        if (rtb.SelectionBackColor != backColor)
                            break;
                        j++;
                    }
                    highLightRanges.Add(new HighLightRange { index = i, color = backColor, length = j - i });
                    i = j;
                }
                else
                    i++;
            }
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
                UnHighlightFound();

                Zone zone = null;
                NoteGroup noteGroup = null;
                if(node.Level == NodeLevels.Group)
                {
                    noteGroup = zoneList.NoteGroups[node.Text];
                    if(noteGroup != null)
                    {
                        if (richEditCtrl1.rtbDoc.Text.Length > 0)
                            noteGroup.Notes = richEditCtrl1.rtbDoc.Rtf;
                        else
                            noteGroup.Notes = null;
                    }
                }
                else if (node.Level == NodeLevels.Zone)
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

                if(zone != null || noteGroup != null)
                {
                    if(zone != null)
                        zone.Paste = GetPasteButton();
                    if (noteGroup != null)
                        noteGroup.Paste = GetPasteButton();
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

        private void UnHighlightFound()
        {
            if(richEditCtrl1.rtbDoc.Text.Length > 0)
            {
                // remove any "find" highlights
                string highlight = GetFindHighlightNumber(richEditCtrl1.rtbDoc.Rtf);
                if (!string.IsNullOrEmpty(highlight))
                {
                    // replace \highlightX ...\highlightY with only whatever is between them (represented by the ...)
                    // where X is the find highlight number and Y is any number
                    string pattern = $@"\{highlight} ?(.+?)\\highlight\d+ ?";
                    Regex re = new Regex(pattern);
                    // can't manipulate the RichTextBox RTF directly - it messes up. so work on a copy of the RTF
                    string source = richEditCtrl1.rtbDoc.Rtf;
                    bool modified = false;
                    Match match = re.Match(source);
                    while(match.Success)
                    {
                        int end = match.Index + match.Groups[0].Length;
                        string left = source.Substring(0, match.Index);
                        string right = source.Substring(end, source.Length - end);
                        // backtrack to see if we need to keep the control word terminating space
                        bool done = false;
                        for (int i = match.Index - 1; i >= 0; i--)
                        {
                            char c = source[i]; //shorthand
                            if (c == ' ' || c == '\r' || c == '\n' || c == '}')
                            {
                                // no immediately preceeding RTF control word
                                // so we need to remove the possible space after the starting \highlight
                                // which the regular expression already removed
                                // so just remove the old string and insert the regex group
                                source = left + match.Groups[1].Value + right;
                                done = true;
                                modified = true;
                                break;
                            }
                            else if (c == '\\')
                            {
                                // there is a preceeding unterminated control word
                                // need to retain the space after the starting \highlight
                                // which the regex removed, so add a space on the replacement
                                source = left + " " + match.Groups[1].Value + right;
                                done = true;
                                modified = true;
                                break;
                            }
                        }
                        if (!done)
                            break; // should never get here. there is always a \rtf at the start of a valid note

                        int start = match.Index;
                        if (start > source.Length)
                            break;
                        match = re.Match(source, start);
                    }
                    if (modified)
                    {
                        richEditCtrl1.rtbDoc.SuspendPainting();
                        richEditCtrl1.rtbDoc.Rtf = source;
                        // re-apply any user-set background that we might have disturbed with the find background
                        foreach(HighLightRange range in highLightRanges)
                        {
                            richEditCtrl1.rtbDoc.SelectionStart = range.index;
                            richEditCtrl1.rtbDoc.SelectionLength = range.length;
                            richEditCtrl1.rtbDoc.SelectionBackColor = range.color;
                            Debug.WriteLineIf(debugMode, $"recolor {treeViewZones.SelectedNode.Text}: {range.index}, {range.length}, {range.color}");
                        }
                        richEditCtrl1.rtbDoc.ResumePainting();
                    }
                }
            }
        }

        private void RichEditCtrl1_OnUserChangedText(object sender, EventArgs e)
        {
            UnHighlightFound();
        }

        private string GetFindHighlightNumber(string rtf)
        {
            // search the RTF color table for the color we are using to highlight "found text"
            string result = string.Empty;
            int start = rtf.IndexOf(@"{\colortbl");
            if(start >= 0)
            {
                int end = rtf.IndexOf('}', start);
                if(end >= 0)
                {
                    string colortbl = rtf.Substring(start, end-start+1);
                    string[] colors = colortbl.Split(';');
                    string match = $@"\red{foundBackground.R}\green{foundBackground.G}\blue{foundBackground.B}";
                    for(int i=0; i<colors.Length; i++)
                    {
                        if (colors[i] == match)
                        {
                            result = $"\\highlight{i}";
                            break;
                        }
                    }
                }
            }
            return result;
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

            if(orig.Count > 0 && paste.Count > 0)
            {
                string textA = string.Join("\n", orig.ToArray());
                string textB = string.Join("\n", paste.ToArray());
                List<string> html = Differences.DiffHtml(textA, textB);
                if (html.Count == 2)
                {
                    Differences diff = new Differences(ActGlobals.oFormActMain, html[0], html[1]);
                    if (treeViewZones.SelectedNode != null)
                    {
                        if (treeViewZones.SelectedNode.Parent != null)
                            diff.Text = $"Differences: {treeViewZones.SelectedNode.Parent.Text} / {treeViewZones.SelectedNode.Text}";
                    }
                    diff.OnReplace += Diff_OnReplace;
                    diff.OnDiscard += Diff_OnDiscard;
                    diff.Show();
                }
            }
        }

        private void Diff_OnReplace(object sender, EventArgs e)
        {
            RichTextBox rtbOrig = new RichTextBox();
            rtbOrig.Rtf = richEditCtrl1.rtbDoc.Rtf;
            int index = rtbOrig.Find(pastePrefix);
            if (index != -1)
            {
                rtbOrig.Select(index, rtbOrig.Text.Length - index);
                string save = rtbOrig.SelectedRtf;
                rtbOrig.Rtf = save;
                // delete the first line, the delimeter
                rtbOrig.SelectionStart = 0;
                rtbOrig.SelectionLength = rtbOrig.Lines[0].Length + 1; // +1 for the \n
                rtbOrig.SelectedText = "";
                // copy the result to the displayed editor
                richEditCtrl1.rtbDoc.Rtf = rtbOrig.Rtf;

                if (rtbOrig.Find(pastePrefix) == -1)
                    buttonCompare.Enabled = false;
            }
        }

        private void Diff_OnDiscard(object sender, EventArgs e)
        {
            RichTextBox rtbOrig = new RichTextBox();
            rtbOrig.Rtf = richEditCtrl1.rtbDoc.Rtf;
            // find the first delimeter
            int index1 = rtbOrig.Find(pastePrefix);
            if(index1 != -1)
            {
                int end = rtbOrig.Text.Length - index1;
                // is there a second delimeter?
                int index2 = rtbOrig.Find(pastePrefix, index1 + 1, RichTextBoxFinds.None);
                if (index2 != -1)
                    end = index2 - index1;
                // select the part to discard
                rtbOrig.Select(index1, end);
                rtbOrig.SelectedRtf = "";

                // copy the result to the displayed editor
                richEditCtrl1.rtbDoc.Rtf = rtbOrig.Rtf;

                if (rtbOrig.Find(pastePrefix) == -1)
                    buttonCompare.Enabled = false;
            }
        }

        private void SavePasteChoice(Zone.PasteType zpt)
        {
            TreeNode tn = treeViewZones.SelectedNode;
            if(tn !=null)
            {
                if (tn.Level == NodeLevels.Group)
                {
                    NoteGroup ng = tn.Tag as NoteGroup;
                    if (ng != null)
                        ng.Paste = zpt;
                }
                else if (tn.Level == NodeLevels.Mob)
                {
                    Zone zone = tn.Parent.Tag as Zone;
                    if (zone != null)
                        zone.Paste = zpt;
                }
                else if (tn.Level == NodeLevels.Zone)
                {
                    Zone zone = tn.Tag as Zone;
                    if (zone != null)
                        zone.Paste = zpt;
                }
            }
        }

        private void radioButtonAppend_Click(object sender, EventArgs e)
        {
            SavePasteChoice(Zone.PasteType.Append);
        }

        private void radioButtonReplace_Click(object sender, EventArgs e)
        {
            SavePasteChoice(Zone.PasteType.Replace);
        }

        private void radioButtonAsk_Click(object sender, EventArgs e)
        {
            SavePasteChoice(Zone.PasteType.Ask);
        }

        private void radioButtonAccept_Click(object sender, EventArgs e)
        {
            SavePasteChoice(Zone.PasteType.Accept);
        }

        #endregion Editor Panel
    }

}
