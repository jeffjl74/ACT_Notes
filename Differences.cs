using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ACT_Notes
{
    public partial class Differences : Form
    {
        public event EventHandler OnReplace;
        public event EventHandler OnDiscard;
        string _left;
        string _right;

        public Differences(Form owner, string left, string right)
        {
            InitializeComponent();

            Owner = owner;
            _left = left;
            _right = right;
        }

        private void Differences_Shown(object sender, EventArgs e)
        {
            webBrowser1.DocumentText = _left;
            webBrowser2.DocumentText = _right;
            if (Owner != null)
            {
                this.Width = Owner.Width;
                this.Height = Owner.Height / 2;
                Point p = new Point(Owner.Left, Owner.Top + Owner.Height / 2  - 38); //38 avoids the plugin's [Compare] panel
                this.Location = p;
                splitContainer1.SplitterDistance = this.Width / 2;
            }
            this.TopMost = true;
        }

        /// <summary>
        /// Find the difference in 2 texts, comparing by textlines, and build a visual
        /// representation of the changes.
        /// </summary>
        /// <param name="TextA">A-version of the text (usualy the old one)</param>
        /// <param name="TextB">B-version of the text (usualy the new one)</param>
        /// <returns>Returns a list of two strings of HTML highlighting the lines that are different.
        /// The first string represents TextA differences and the second string TextB.</returns>
        public static List<string> DiffHtml(string TextA, string TextB)
        {
            List<string> result = new List<string>();
            const string header = "<!DOCTYPE html><html><head><style>p{margin:2px;}</style></head><body>";
            const string empty = "<p style =\"background-color:#d0d0d0;height:20px;\"></p>";
            const string highlight = "<p style=\"background-color:#ffff29\">";

            StringBuilder sbA = new StringBuilder();
            sbA.Append(header);

            StringBuilder sbB = new StringBuilder();
            sbB.Append(header);

            string[] linesA = TextA.Split('\n');
            string[] linesB = TextB.Split('\n');
            {
                Diff.Item[] diffs = Diff.DiffText(TextA, TextB, true, true, true);
                int indexB = 0;
                int indexA = 0;
                int displayA = 0;
                int displayB = 0;
                for (int n = 0; n < diffs.Length; n++)
                {
                    Diff.Item it = diffs[n];

                    // write unchanged lines
                    while (indexA < it.StartA)
                    {
                        sbA.Append($"<p>{indexA + 1:000}| {linesA[indexA]}</p>");
                        indexA++;
                        displayA++;
                    }
                    while (indexB < it.StartB)
                    {
                        sbB.Append($"<p>{indexB + 1:000}| {linesB[indexB]}</p>");
                        indexB++;
                        displayB++;
                    }

                    // tag A deleted lines
                    while (indexA < it.StartA + it.deletedA)
                    {
                        sbA.Append($"{highlight}{indexA + 1:000}| {linesA[indexA]}</p>");
                        //Debug.WriteLine($"del A/{indexA}");
                        indexA++;
                        displayA++;
                    }

                    // highlight B inserted lines
                    while (indexB < it.StartB + it.insertedB)
                    {
                        sbB.Append($"{highlight}{indexB + 1:000}| {linesB[indexB]}</p>");
                        //Debug.WriteLine($"ins B/{indexB}");
                        indexB++;
                        displayB++;
                        if(displayA < displayB)
                        {
                            // flag missing spots in A
                            //Debug.WriteLine($"ins B/{indexB}, empty A/{indexA}");
                            sbA.Append(empty);
                            displayA++;
                        }
                    }

                    // align the displays
                    while(displayB < displayA)
                    {
                        //Debug.WriteLine($"catch up B/{displayB}");
                        sbB.Append(empty);
                        displayB++;
                    }

                }

                // write the rest of the unchanged lines
                while (indexA < linesA.Length)
                {
                    sbA.Append($"<p>{indexA + 1:000}| {linesA[indexA]}</p>");
                    indexA++;
                }
                while (indexB < linesB.Length)
                {
                    sbB.Append($"<p>{indexB + 1:000}| {linesB[indexB]}</p>");
                    indexB++;
                }

                sbA.Append("</body></html>");
                sbB.Append("</body></html>");
            }

            result.Add(sbA.ToString());
            result.Add(sbB.ToString());
            return result;
        }

        private void buttonRepalce_Click(object sender, EventArgs e)
        {
            if(OnReplace != null)
                OnReplace.Invoke(this, new EventArgs());
            Close();
        }

        private void buttonDiscard_Click(object sender, EventArgs e)
        {
            if(OnDiscard != null)
                OnDiscard.Invoke(this, new EventArgs());
            Close();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
