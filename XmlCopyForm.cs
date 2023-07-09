using Advanced_Combat_Tracker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Security.Policy;
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
        string xml = string.Empty;
        Zone _zone;
        Mob _mob;
        List<string> chatSnippets;
        int nextSectionIndex = 0;
        const string doFileName = "note-macro{0}.txt";
        bool loading = true;
        bool preIncremet = false;
        bool autoIncrementing = false;
        bool _compress = false;
        bool compressed = false;
        bool _doZone = false;
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


        public XmlCopyForm(Zone zone, Mob mob, bool compressChecked, bool enitreZone)
        {
            InitializeComponent();

            _zone = zone;
            _mob = mob;
            _compress = compressChecked;
            _doZone = enitreZone;
        }

        private void XmlCopyForm_Load(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(_zone.Prefix))
            {
                if (_zone.Prefix == "/g")
                    radioButtonG.Checked = true;
                else if (_zone.Prefix == "/r")
                    radioButtonR.Checked = true;
                else
                {
                    radioButtonCustom.Checked = true;
                    textBoxCustom.Text = _zone.Prefix;
                }
            }
            else
            {
                // guess at a good prefix
                if (_zone.ZoneName.Contains("["))
                {
                    radioButtonG.Checked = true;
                    if (_zone.ZoneName.Contains("Raid"))
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

        private List<string> Breakup(Zone zone, Mob mob, int maxLen)
        {
            List<string> snippets = new List<string>();

            int baseSize = MobToXML(zone, mob, " ", 10).Length;
            if (radioButtonCustom.Checked)
                baseSize += textBoxCustom.Text.Length;
            string noBreaks;
            string orig;
            if (mob != null)
                orig = mob.Notes;
            else
                orig = zone.Notes;
            noBreaks = orig.Replace("\\par\r\n","\\par ").Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
            compressed = false;
            if(noBreaks.Contains(@"\pict") && checkBoxCompress.Checked)
            {
                // compress the note if it has any image(s)
                compressed = true;
                baseSize += " C='T'".Length;
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
            // alter the XML encoding to get past EQII & ACT expectations
            xml = EncodeShare(noBreaks);
            if (baseSize + xml.Length < maxLen)
            {
                string whole = MobToXML(zone, mob, xml, 1);
                snippets.Add(whole);
            }
            else
            {
                // get the data for the first section
                nextSectionIndex = 0;
                string section1 = GetNextSection(maxLen - baseSize);

                // calc number of sections
                int baseSnippetSize = SnippetToXml(" ", 99, 99).Length;
                if (radioButtonCustom.Checked)
                    baseSnippetSize += textBoxCustom.Text.Length;
                int snippetLen = maxLen - baseSnippetSize;
                int remainingCount = xml.Length - (maxLen - baseSize);
                int sectionCount = remainingCount / snippetLen;
                if (remainingCount % snippetLen != 0)
                    sectionCount++;
                sectionCount++; //count the first section

                // build the initial snippet with the zone & mob names in it
                snippets.Add(MobToXML(zone, mob, section1, sectionCount));

                // build the rest of the snippets with just note data
                for(int j=2; nextSectionIndex<xml.Length; j++)
                {
                    string section = GetNextSection(snippetLen);
                    snippets.Add(SnippetToXml(section, j, sectionCount));
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
            for(int i=nextSectionIndex, j=0; i<xml.Length && j<maxLen; i++, j++)
            {
                sb.Append(xml[i]);
            }
            nextSectionIndex += sb.Length;
            return sb.ToString();
        }

        private string MobToXML(Zone zone, Mob mob, string note, int sections)
        {
            string result = string.Empty;
            if(zone != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"<{Notes.XmlSnippetType} Z='{EncodeShare(zone.ZoneName)}'");
                sb.Append($" S='1/{sections}'");
                if (mob != null)
                    sb.Append(string.Format(" M='{0}'", EncodeShare(mob.MobName)));
                sb.Append(string.Format(" P='{0}'", ActGlobals.charName));
                if (compressed)
                    sb.Append(" C='T'");
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
            if(_doZone)
            {
                chatSnippets = new List<string>();
                if(_zone.Notes != null)
                {
                    chatSnippets.AddRange(Breakup(_zone, null, maxChatLen));
                }
                for(int i= 0; i < _zone.Mobs.Count; i++)
                {
                    if(_zone.Mobs[i].Notes != null)
                        chatSnippets.AddRange(Breakup(_zone, _zone.Mobs[i], maxChatLen));
                }
            }
            else
                chatSnippets = Breakup(_zone, _mob, maxChatLen);

            listBox1.Items.Clear();
            preIncremet = false;
            toolTip1.SetToolTip(buttonMacro, "Press to generate and list macro files");
            toolTip1.SetToolTip(buttonCopy, "Press to copy the selected XML item to the clipboard");
            for (int i = 1; i <= chatSnippets.Count; i++)
            {
                ListItem listItem = new ListItem { description = $"Copy section #{i} to clipboard", type = ItemType.Section };
                listBox1.Items.Add(listItem);
            }
            if (chatSnippets.Count > 0)
                listBox1.SelectedIndex = 0;
            else
                toolStripStatusLabel1.Text = string.Empty;
        }

        private void buttonDone_Click(object sender, EventArgs e)
        {
            if (radioButtonCustom.Checked)
            {
                if (!string.IsNullOrEmpty(textBoxCustom.Text))
                {
                    if (textBoxCustom.Text.StartsWith("/"))
                        _zone.Prefix = textBoxCustom.Text;
                    else
                        _zone.Prefix = "/" + textBoxCustom.Text;
                }
                else
                    _zone.Prefix = null;
            }
            else if (radioButtonG.Checked)
                _zone.Prefix = "/g";
            else if (radioButtonR.Checked)
                _zone.Prefix = "/r";

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
            if (_doZone)
            {
                if (_zone.Notes != null)
                {
                    lines.AddRange(Breakup(_zone, null, maxMacroLen - prefix.Length));
                }
                for (int i = 0; i < _zone.Mobs.Count; i++)
                {
                    if (_zone.Mobs[i].Notes != null)
                        lines.AddRange(Breakup(_zone, _zone.Mobs[i], maxMacroLen - prefix.Length));
                }
            }
            else
                lines = Breakup(_zone, _mob, maxMacroLen - prefix.Length);

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
