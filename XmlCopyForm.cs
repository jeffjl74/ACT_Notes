using Advanced_Combat_Tracker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace ACT_Notes
{

    public partial class XmlCopyForm : Form
    {
        const int maxChatLen = 240;
        const int maxMacroLen = 1000;
        const int encodeLen = 6;
        string xmlNote = string.Empty;
        NoteGroup _noteGroup;
        List<Zone> _zones;
        Mob _mob;
        List<string> chatSnippets;
        int nextSectionIndex = 0;
        public static string doFileName = "note-macro{0}.txt";
        bool loading = true;
        bool preIncremet = false;
        bool autoIncrementing = false;
        bool _compress = false;
        bool compressed = false;
        Notes.XmlShareDepth _depth;
        public event EventHandler CompressCheckChanged;
        enum ListMode { CopyList, FileList };
        ListMode dialogMode;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        List<IntPtr> _handles = new List<IntPtr>();

        // simple class to use as the listbox item
        enum ItemType { Section, Command }
        class ListItem
        {
            public string description;
            public ItemType type;
            public override string ToString()
            {
                return description;
            }
        }


        public XmlCopyForm(NoteGroup group, List<Zone> zones, Mob mob, bool compressChecked, Notes.XmlShareDepth depth)
        {
            InitializeComponent();

            _noteGroup = group;
            _zones = zones;
            _mob = mob;
            _compress = compressChecked;
            _depth = depth;
        }

        private void XmlCopyForm_Load(object sender, EventArgs e)
        {
            string sourcePrefix;
            if (_depth == Notes.XmlShareDepth.GroupOnly || _depth == Notes.XmlShareDepth.GroupDeep)
                sourcePrefix = _noteGroup.Prefix;
            else if (_depth == Notes.XmlShareDepth.ZoneOnly || _depth == Notes.XmlShareDepth.ZoneDeep)
                sourcePrefix = _zones[0].Prefix;
            else
                sourcePrefix = "/g";

            if (!string.IsNullOrEmpty(sourcePrefix))
            {
                if (sourcePrefix == "/g")
                    radioButtonG.Checked = true;
                else if (sourcePrefix == "/r")
                    radioButtonR.Checked = true;
                else
                {
                    radioButtonCustom.Checked = true;
                    textBoxCustom.Text = sourcePrefix;
                }
            }
            else
            {
                // guess at a prefix
                if (_zones.Count > 0 && _zones[0].ZoneName.Contains("["))
                {
                    radioButtonG.Checked = true;
                    if (_zones[0].ZoneName.Contains("Raid"))
                        radioButtonR.Checked = true;
                }
                else
                    radioButtonG.Checked = true; // default to /g
            }

            checkBoxCompress.Checked = _compress;

            // look for game instances
            Process[] processes = Process.GetProcessesByName("EverQuest2");
            if (processes.Length > 0)
            {
                _handles = new List<IntPtr>();
                foreach (Process p in processes)
                {
                    // only want the main window
                    if (p.MainWindowTitle.StartsWith("EverQuest II ("))
                    {
                        if (!_handles.Contains(p.MainWindowHandle))
                            _handles.Add(p.MainWindowHandle);
                    }
                }
                if (_handles.Count > 0)
                {
                    foreach (IntPtr intPtr in _handles)
                    {
                        comboBoxGame.Items.Add(intPtr);
                    }
                    comboBoxGame.Items.Add(""); // item to allow user to de-select game activation
                    comboBoxGame.SelectedIndex = 0;
                    // build the macro list
                    buttonMacro_Click(null, null);
                }
            }
            if(comboBoxGame.Items.Count == 0)
            {
                // no game running, use the copy list
                dialogMode = ListMode.CopyList;
                ReloadCopyList();
            }

            loading = false;
        }

        public int BuildMacroFiles()
        {
            buttonMacro_Click(null, null);
            return listBox1.Items.Count;
        }

        public static string RemoveEndLines(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input.Replace("\\par\r\n", @"\par ").Replace("\\par\r", @"\par ").Replace("\\par\n", @"\par ").Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
        }

        private List<string> Breakup(Zone zone, Mob mob, int maxLen)
        {
            List<string> snippets = new List<string>();

            string noBreaks;
            string orig;
            if (mob != null)
                orig = mob.Notes;
            else if (zone != null)
                orig = zone.Notes;
            else
                orig = _noteGroup.Notes;
            noBreaks = RemoveEndLines(orig);
            compressed = false;
            if(noBreaks.Contains(@"\pict") && checkBoxCompress.Checked)
            {
                // compress the note if it has any image(s)
                compressed = true;
                MemoryStream input = new MemoryStream(Encoding.UTF8.GetBytes(noBreaks));
                byte[] data = Compress(input);
                // change it to text
                StringBuilder sb = new StringBuilder(data.Length * 2);
                foreach(byte b in data)
                {
                    sb.Append($"{b:X2}");
                }
                noBreaks = sb.ToString();
            }

            int channelSize = 3;
            if (radioButtonCustom.Checked)
                channelSize = textBoxCustom.Text.Length + 2;
            int sectionSize = SnippetToXml(" ", 99, 99).Length;
            int overhead = channelSize + sectionSize;

            // alter the XML encoding to get past EQII & ACT expectations
            xmlNote = EncodeShare(noBreaks);

            string whole = MobToXML(_noteGroup, zone, mob, xmlNote, 1);
            if (whole.Length + channelSize < maxLen)
            {
                snippets.Add(whole);
            }
            else
            {
                // get the data for the first section
                nextSectionIndex = 0;
                string section1 = GetNextSection(maxLen - overhead);

                // calc number of sections
                int snippetLen = maxLen - overhead;
                int remainingLength = xmlNote.Length - (maxLen - overhead);
                int totalSections = remainingLength / snippetLen;
                if (remainingLength % snippetLen != 0)
                    totalSections++;
                totalSections++; //account for the first section
                int initalDataSectionNum = 2;

                // build the initial snippet(s) with the zone & mob names in it
                string s1 = MobToXML(_noteGroup, zone, mob, section1, totalSections);
                if(s1.Length > maxLen)
                {
                    //it's too long
                    //break out the group, zone, mob, player into a section
                    if (nextSectionIndex <= xmlNote.Length)
                        totalSections++;
                    string names = MobToXML(_noteGroup, zone, mob, "", totalSections);
                    if (names.Length < maxLen)
                    {
                        snippets.Add(names);
                        initalDataSectionNum = 2;
                    }
                    else
                    {
                        // still too long, just make a section for each name
                        totalSections += 2;
                        string groupSec = MobToXML(_noteGroup, null, null, "", totalSections, 1);
                        string zoneSec = MobToXML(null, zone, null, "", totalSections, 2);
                        string mobSec = MobToXML(null, null, mob, "", totalSections, 3);
                        if(groupSec.Length > maxLen || zoneSec.Length > maxLen || mobSec.Length > maxLen)
                        {
                            string groupShort = groupSec.Substring(groupSec.IndexOf("G=") + 2, 8);
                            string zoneShort = zoneSec.Substring(zoneSec.IndexOf("Z=") + 2, 8);
                            string mobShort = mobSec.Substring(mobSec.IndexOf("M=") + 2, 8);
                            snippets.Add($"The group ({groupShort}...), zone ({zoneShort}...), or mob ({mobShort}...)is too long to share.");
                            return snippets;
                        }
                        snippets.Add(groupSec);
                        snippets.Add(zoneSec);
                        snippets.Add(mobSec);
                        initalDataSectionNum = 4;
                    }
                    snippets.Add(SnippetToXml(section1, initalDataSectionNum, totalSections));
                    initalDataSectionNum++;
                }
                else
                    snippets.Add(s1);

                // build the rest of the snippets with just note data
                for(int j=initalDataSectionNum; nextSectionIndex<xmlNote.Length; j++)
                {
                    string section = GetNextSection(snippetLen);
                    snippets.Add(SnippetToXml(section, j, totalSections));
                }
            }
            return snippets;
        }

        private static byte[] Compress(Stream input)
        {
            using (var compressStream = new MemoryStream())
            using (var compressor = new DeflateStream(compressStream, CompressionMode.Compress))
            {
                input.CopyTo(compressor);
                compressor.Close();
                return compressStream.ToArray();
            }
        }

        private string GetNextSection(int maxLen)
        {
            StringBuilder sb = new StringBuilder();
            for(int i=nextSectionIndex, j=0; i<xmlNote.Length && j<maxLen; i++, j++)
            {
                sb.Append(xmlNote[i]);
            }
            nextSectionIndex += sb.Length;
            return sb.ToString();
        }

        private string MobToXML(NoteGroup noteGroup, Zone zone, Mob mob, string note, int sections, int currentSection = 1)
        {
            string result = string.Empty;
            if (zone != null || _noteGroup != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"<{Notes.XmlSnippetType}");
                sb.Append($" S='{currentSection}/{sections}'");
                if (noteGroup != null)
                    sb.Append($" G='{EncodeShare(noteGroup.GroupName)}'");
                if (zone != null)
                    sb.Append($" Z='{EncodeShare(zone.ZoneName)}'");
                if (mob != null)
                    sb.Append(string.Format(" M='{0}'", EncodeShare(mob.MobName)));
                if (currentSection == 1)
                {
                    sb.Append(string.Format(" P='{0}'", ActGlobals.charName));
                    if (compressed)
                        sb.Append(" C='T'");
                }
                if(!string.IsNullOrEmpty(note))
                    sb.Append(string.Format(" N='{0}'", note));
                sb.Append(" />");

                result = sb.ToString();
            }
            return result;
        }

        private string SnippetToXml(string note, int sectionNum, int sectionCount)
        {
            string result = string.Empty;
            if(!string.IsNullOrEmpty(note))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(($"<{Notes.XmlSnippetType} S='{sectionNum}/{sectionCount}'"));
                sb.Append(($" N='{note}'"));
                sb.Append(" />");

                result = sb.ToString();
            }
            return result;
        }

        public static string EncodeShare(string text)
        {
            // alter the XML encoding to get past EQII & ACT expectations
            return EncodeXml_ish(text);
        }

        public static string DecodeShare(string text)
        {
            // convert incoming string to encoded HTML, then decode the HTML
            return System.Net.WebUtility.HtmlDecode(text.Replace(':', ';').Replace('!', '&'));
        }
        
        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            //if (hexString.Length % 2 != 0)
            //{
            //    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The string cannot have an odd number of digits: {0}", hexString));
            //}

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }

        public static string DecompressShare(string compressed)
        {
            string result = string.Empty;
            try
            {
                MemoryStream input = new MemoryStream(ConvertHexStringToByteArray(compressed));
                input.Position = 0;

                using (MemoryStream decompressedFileStream = new MemoryStream())
                {
                    using (DeflateStream decompressionStream = new DeflateStream(input, CompressionMode.Decompress, true))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                    decompressedFileStream.Close();
                    byte[] data = decompressedFileStream.ToArray();
                    result = Encoding.Default.GetString(data);
                }
            }
            catch (Exception dex)
            {
                SimpleMessageBox.Show(ActGlobals.oFormActMain, "Note decompress failed: " + dex.Message);
            }
            return result;
        }

        // Encode with a scheme like HTML, but avoid characters that confuse or break
        // ACT XML handing, EQII chat pasting, and EQII macros.
        // Use ! to start and : to end a special charcter encode instead of & and ;
        private static string EncodeXml_ish(string text)
        {
            if (text == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            int len = text.Length;
            for (int i = 0; i < len; i++)
            {
                switch (text[i])
                {
                    case '<':
                        sb.Append("!lt:");
                        break;
                    case '>':
                        sb.Append("!gt:");
                        break;
                    case '"':
                        sb.Append("!quot:");
                        break;
                    case '&':
                        sb.Append("!amp:");
                        break;
                    case '\'':
                        sb.Append("!apos:");
                        break;
                    case '\\':
                        sb.Append("!#92:");
                        break;
                    //case '#':
                    //    sb.Append("!#35:");
                    //    break;
                    case ':':
                        sb.Append("!#58:");
                        break;
                    case '!':
                        sb.Append("!#33:");
                        break;
                    case ';':
                        sb.Append("!#59:");
                        break;
                    default:
                        sb.Append(text[i]);
                        break;
                }
            }
            return sb.ToString();
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            string prefix = string.Empty;
            if (radioButtonG.Checked)
                prefix = "/g ";
            else if (radioButtonR.Checked)
                prefix = "/r ";
            else if (!string.IsNullOrEmpty(textBoxCustom.Text))
            {
                prefix = textBoxCustom.Text;
                if (!prefix.EndsWith(" "))
                    prefix = prefix + " ";
            }

            try
            {
                if(dialogMode == ListMode.CopyList)
                {
                    if(preIncremet)
                    {
                        autoIncrementing = true;
                        if (listBox1.SelectedIndex < chatSnippets.Count - 1)
                            listBox1.SelectedIndex++;
                        else
                        {
                            listBox1.SelectedIndex = -1;
                            preIncremet = false;
                            toolStripStatusLabel1.Text = "No more items.";
                        }
                        autoIncrementing = false;
                    }
                    else
                    {
                        // first time through, we use the selected item
                        // next time, we will go to the next item
                        preIncremet = true;
                        toolTip1.SetToolTip(buttonCopy, "Press to copy the next XML item to the clipboard");
                    }
                    if (listBox1.SelectedIndex >= 0)
                    {
                        Clipboard.SetText(prefix + chatSnippets[listBox1.SelectedIndex]);

                        int lineNum = listBox1.SelectedIndex + 1;
                        bool gameActivated = false;
                        if (comboBoxGame.Items.Count > 0 && comboBoxGame.SelectedIndex >= 0)
                        {
                            // if we found an EQII game window, activate it
                            if (!string.IsNullOrEmpty(comboBoxGame.Items[comboBoxGame.SelectedIndex].ToString()))
                            {
                                toolStripStatusLabel1.Text = String.Format(@"<Enter><Ctrl-v> to paste sect {0}. [Copy] for next", lineNum);
                                IntPtr handle = (IntPtr)comboBoxGame.Items[comboBoxGame.SelectedIndex];
                                SetForegroundWindow(handle);
                                gameActivated = true;
                            }
                        }
                        if (!gameActivated)
                        {
                            toolStripStatusLabel1.Text = $"Section {lineNum} copied. [Copy] for next.";
                        }
                    }
                }
                else
                {
                    // switching from macro list to section list
                    dialogMode = ListMode.CopyList;
                    ReloadCopyList();
                }
            }
            catch (Exception)
            {
                SimpleMessageBox.Show(ActGlobals.oFormActMain, "Clipboard copy failed. Try again.", "Failed");
                preIncremet = false;
            }
        }

        private void ReloadCopyList()
        {
            listBox1.Items.Clear();

            if (_depth == Notes.XmlShareDepth.GroupDeep || _depth == Notes.XmlShareDepth.ZoneDeep)
            {
                chatSnippets = new List<string>();

                foreach (Zone zone in _zones)
                {
                    if (zone.Notes != null)
                    {
                        chatSnippets.AddRange(Breakup(zone, null, maxChatLen));
                    }
                    for (int i = 0; i < zone.Mobs.Count; i++)
                    {
                        if (zone.Mobs[i].Notes != null)
                            chatSnippets.AddRange(Breakup(zone, zone.Mobs[i], maxChatLen));
                    }
                }

                if (_depth == Notes.XmlShareDepth.GroupDeep && !string.IsNullOrEmpty(_noteGroup.Notes))
                    chatSnippets.AddRange(Breakup(null, null, maxChatLen));
            }
            else if (_depth == Notes.XmlShareDepth.ZoneOnly && _zones.Count > 0)
                chatSnippets = Breakup(_zones[0], null, maxChatLen);
            else if (_depth == Notes.XmlShareDepth.GroupOnly && !string.IsNullOrEmpty(_noteGroup.Notes))
                chatSnippets = Breakup(null, null, maxChatLen);
            else if (_depth == Notes.XmlShareDepth.MobOnly && _zones.Count > 0)
                chatSnippets = Breakup(_zones[0], _mob, maxChatLen);

            preIncremet = false;
            toolTip1.SetToolTip(buttonMacro, "Press to generate and list macro files");
            toolTip1.SetToolTip(buttonCopy, "Press to copy the selected XML item to the clipboard");
            for (int i = 1; i <= chatSnippets.Count; i++)
            {
                ListItem listItem = new ListItem { description = $"Copy section #{i} to clipboard", type = ItemType.Section };
                listBox1.Items.Add(listItem);
            }
            if (chatSnippets.Count > 0)
            {
                listBox1.SelectedIndex = 0;
                toolStripStatusLabel1.Text = "Press [Copy] to copy the selected line";
            }
            else
                toolStripStatusLabel1.Text = string.Empty;
        }

        private void buttonDone_Click(object sender, EventArgs e)
        {
            string prefix = string.Empty;
            if (radioButtonCustom.Checked)
            {
                prefix = textBoxCustom.Text;
                if (!string.IsNullOrEmpty(prefix))
                {
                    if (!prefix.StartsWith("/"))
                        prefix = "/" + prefix;
                }
            }
            else if (radioButtonG.Checked)
                prefix = "/g";
            else if (radioButtonR.Checked)
                prefix = "/r";

            if (_depth == Notes.XmlShareDepth.GroupDeep || _depth == Notes.XmlShareDepth.GroupOnly)
                _noteGroup.Prefix = prefix;
            else if(_zones.Count > 0)
                _zones[0].Prefix = prefix;

            Close();
        }

        private void buttonMacro_Click(object sender, EventArgs e)
        {
            if(dialogMode == ListMode.FileList)
            {
                if (listBox1.Items.Count > 0)
                {
                    // already have the macro list
                    // move to the next one?
                    if (preIncremet)
                    {
                        autoIncrementing = true;
                        if (listBox1.SelectedIndex < listBox1.Items.Count - 1)
                            listBox1.SelectedIndex++;
                        else
                        {
                            listBox1.SelectedIndex = -1;
                            toolStripStatusLabel1.Text = "No more items.";
                        }
                        autoIncrementing = false;
                    }
                    else
                    {
                        // first time through, we use the selected item
                        // next time, we will go to the next item
                        if(!loading)
                            preIncremet = true; // next time, we increment first
                        toolTip1.SetToolTip(buttonMacro, "Press to copy the next line to the clipboard");
                    }
                    if (listBox1.SelectedIndex >= 0 && !loading)
                    {
                        try
                        {
                            int lineNum = listBox1.SelectedIndex + 1;
                            string text = ((ListItem)listBox1.Items[listBox1.SelectedIndex]).description;
                            Clipboard.SetText(text);

                            bool gameActivated = false;
                            if (comboBoxGame.Items.Count > 0 && comboBoxGame.SelectedIndex >= 0)
                            {
                                // if we found an EQII game window, activate it
                                if (!string.IsNullOrEmpty(comboBoxGame.Items[comboBoxGame.SelectedIndex].ToString()))
                                {
                                    toolStripStatusLabel1.Text = String.Format(@"<Enter><Ctrl-v> to paste line {0}. [Macro] for next", lineNum);
                                    IntPtr handle = (IntPtr)comboBoxGame.Items[comboBoxGame.SelectedIndex];
                                    SetForegroundWindow(handle);
                                    gameActivated = true;
                                }
                            }
                            if(!gameActivated)
                            {
                                toolStripStatusLabel1.Text = $"Line {lineNum} copied. [Macro] for next.";
                            }
                        }
                        catch (Exception)
                        {
                            SimpleMessageBox.Show(ActGlobals.oFormActMain, "Clipboard copy failed. Try again.", "Failed");
                            preIncremet = false;
                        }
                    }
                }
            }
            else
            {
                dialogMode = ListMode.FileList;
                ReloadMacroList();
            }

        }

        private void ReloadMacroList()
        {
            string prefix = string.Empty;
            if (radioButtonG.Checked)
                prefix = "g ";
            else if (radioButtonR.Checked)
                prefix = "r ";
            else if (!string.IsNullOrEmpty(textBoxCustom.Text))
            {
                prefix = textBoxCustom.Text;
                if (prefix.StartsWith("/"))
                    prefix = prefix.Substring(1);
                if (!prefix.EndsWith(" "))
                    prefix = prefix + " ";
            }

            toolTip1.SetToolTip(buttonMacro, "Press to copy the selected line to the clipboard");
            toolTip1.SetToolTip(buttonCopy, "Press to generate and list XML sections");

            List<string> lines = new List<string>();

            if (_depth == Notes.XmlShareDepth.GroupDeep || _depth == Notes.XmlShareDepth.ZoneDeep)
            {
                foreach (Zone zone in _zones)
                {
                    if (zone.Notes != null)
                    {
                        lines.AddRange(Breakup(zone, null, maxMacroLen - prefix.Length));
                    }
                    for (int i = 0; i < zone.Mobs.Count; i++)
                    {
                        if (zone.Mobs[i].Notes != null)
                            lines.AddRange(Breakup(zone, zone.Mobs[i], maxMacroLen - prefix.Length));
                    }
                }
                if (_depth == Notes.XmlShareDepth.GroupDeep && !string.IsNullOrEmpty(_noteGroup.Notes))
                    lines.AddRange(Breakup(null, null, maxMacroLen - prefix.Length));
            }
            else if (_depth == Notes.XmlShareDepth.ZoneOnly && _zones.Count > 0)
                lines.AddRange(Breakup(_zones[0], null, maxMacroLen));
            else if (_depth == Notes.XmlShareDepth.GroupOnly && !string.IsNullOrEmpty(_noteGroup.Notes))
                lines.AddRange(Breakup(null, null, maxMacroLen));
            else if (_depth == Notes.XmlShareDepth.MobOnly && _zones.Count > 0)
                lines.AddRange(Breakup(_zones[0], _mob, maxMacroLen));

            int fileCount = 1;
            int lineCount = 1;
            int numFiles = 0;
            int snippetIndex = 0;
            do
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < 16 && snippetIndex < lines.Count; i++, lineCount++, snippetIndex++)
                {
                    if (!string.IsNullOrEmpty(prefix))
                        sb.Append(prefix);
                    sb.Append(lines[snippetIndex]);
                    sb.Append("\r\n");
                }
                string filePath = string.Format(doFileName, fileCount);
                if (ActGlobals.oFormActMain.SendToMacroFile(filePath, sb.ToString(), string.Empty))
                    numFiles++;
                else
                {
                    SimpleMessageBox.Show(ActGlobals.oFormActMain, "Macro file failed", "File Problem");
                    break;
                }
                fileCount++;
            } while (snippetIndex < lines.Count);

            listBox1.Items.Clear();
            for (int i = 0; i < numFiles; i++)
            {
                string file = string.Format(doFileName, i + 1);
                ListItem listItem = new ListItem { description = $"/do_file_commands {file}", type = ItemType.Command };
                listBox1.Items.Add(listItem);
            }
            if (listBox1.Items.Count > 0)
            {
                listBox1.SelectedIndex = 0;
                preIncremet = false;
                toolStripStatusLabel1.Text = "Press [Macro] to copy the selected line";
            }
        }
        
        private void radioButtonCustom_CheckedChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                ReloadList();
            }
        }

        private void ReloadList()
        {
            if (dialogMode == ListMode.CopyList)
                ReloadCopyList();
            else
                ReloadMacroList();
        }

        private void textBoxCustom_TextChanged(object sender, EventArgs e)
        {
            if (radioButtonCustom.Checked && !loading)
            {
                ReloadList();
            }
        }

        private void checkBoxCompress_CheckedChanged(object sender, EventArgs e)
        {
            if(!loading)
            {
                if (CompressCheckChanged != null)
                {
                    // notify our parent
                    CompressCheckChanged.Invoke(this, new CompressCheckChangedEventArgs { isChecked = checkBoxCompress.Checked });
                }
                ReloadList();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                if (listBox1.SelectedIndex >= 0)
                {
                    toolStripStatusLabel1.Text = String.Format("Press {0} to copy selection to clipboard", dialogMode == ListMode.FileList ? "[Macro]" : "[Copy]");
                    if (!autoIncrementing)
                        preIncremet = false;
                }
            }
        }

        private void radioButtonG_CheckedChanged(object sender, EventArgs e)
        {
            if (!loading && radioButtonG.Checked)
                ReloadList();
        }

        private void radioButtonR_CheckedChanged(object sender, EventArgs e)
        {
            if (!loading && radioButtonR.Checked)
                ReloadList();
        }
    }
}
