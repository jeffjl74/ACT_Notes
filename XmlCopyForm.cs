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
        bool _compress = false;
        bool compressed = false;
        public event EventHandler CompressCheckChanged;

        public XmlCopyForm(Zone zone, Mob mob, bool compressChecked)
        {
            InitializeComponent();

            _zone = zone;
            _mob = mob;
            _compress = compressChecked;
            chatSnippets = Breakup(zone, mob, maxChatLen);
        }

        private void XmlCopyForm_Load(object sender, EventArgs e)
        {
            ReloadList();

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
                if (listBox1.SelectedIndex >= 0)
                {
                    Clipboard.SetText(prefix + chatSnippets[listBox1.SelectedIndex]);
                    labelReady.Text = $"Section {listBox1.SelectedIndex + 1} copied to clipboard";
                    if (listBox1.SelectedIndex < chatSnippets.Count - 1)
                        listBox1.SelectedIndex++;
                    else
                        listBox1.SelectedIndex = -1;
                }
                else
                {
                    SimpleMessageBox.Show(this, "Select a section to copy to the clipboard.", "Error");
                    labelReady.Text = string.Empty;
                }
            }
            catch (Exception)
            {
                SimpleMessageBox.Show(this, "Clipboard copy failed. Try again.", "Failed");
            }
        }

        private void ReloadList()
        {
            listBox1.Items.Clear();
            for (int i = 1; i <= chatSnippets.Count; i++)
            {
                listBox1.Items.Add($"Copy section #{i} to clipboard");
            }
            if (chatSnippets.Count > 0)
                listBox1.SelectedIndex = 0;
            labelReady.Text = string.Empty;
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

            List<string> lines = Breakup(_zone, _mob, maxMacroLen - prefix.Length);

            int fileCount = 1;
            int lineCount = 1;
            int numFiles = 0;
            int snippetIndex = 0;
            do
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < 16 && snippetIndex<lines.Count; i++, lineCount++, snippetIndex++)
                {
                    if(!string.IsNullOrEmpty(prefix))
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

            string firstFile = string.Format(doFileName, 1);
            string lastFile = string.Format(doFileName, numFiles);
            if (numFiles > 1)
                labelReady.Text = $"Wrote {firstFile} thru {lastFile}";
            else
                labelReady.Text = $"Wrote {firstFile}";
        }

        private void radioButtonCustom_CheckedChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                chatSnippets = Breakup(_zone, _mob, maxChatLen);
                ReloadList();
            }
        }

        private void textBoxCustom_TextChanged(object sender, EventArgs e)
        {
            if (radioButtonCustom.Checked)
            {
                chatSnippets = Breakup(_zone, _mob, maxChatLen);
                ReloadList();
            }
        }

        private void checkBoxCompress_CheckedChanged(object sender, EventArgs e)
        {
            if(CompressCheckChanged != null)
            {
                CompressCheckChanged.Invoke(this, new CompressCheckChangedEventArgs { isChecked = checkBoxCompress.Checked});
            }
            chatSnippets = Breakup(_zone, _mob, maxChatLen);
            ReloadList();
        }
    }
}
