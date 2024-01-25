using Advanced_Combat_Tracker;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
namespace ACT_Notes
{

    public partial class EditCtrl : UserControl
    {
        // constructor
        public EditCtrl()
        {
            InitializeComponent();
        }


        #region Declarations

        public event EventHandler OnSave;
        public event EventHandler OnUserChangedText;

        public static Color audioAlertColor = Color.BlueViolet;
        public static Color visualAlertColor = Color.MediumOrchid;
        public static Color audioVisualAlertColor = Color.MediumVioletRed;

        #endregion Declarations


        #region Toolbar Methods

        private void SaveToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            try
            {
                if (OnSave != null)
                    OnSave.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error");
            }
        }

        private void SelectFontToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            try
            {
                Point pt = PointToScreen(new Point(this.Left, this.Top));
                FontDialogEx fontDialog = new FontDialogEx(pt.X, pt.Y);
                if (!(rtbDoc.SelectionFont == null))
                {
                    fontDialog.Font = rtbDoc.SelectionFont;
                }
                else
                {
                    fontDialog.Font = null;
                }
                fontDialog.ShowApply = true;
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    rtbDoc.SelectionFont = fontDialog.Font;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error");
            }
        }

        private void BoldToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            try
            {
                if (!(rtbDoc.SelectionFont == null))
                {
                    System.Drawing.Font currentFont = rtbDoc.SelectionFont;
                    System.Drawing.FontStyle newFontStyle;

                    newFontStyle = rtbDoc.SelectionFont.Style ^ FontStyle.Bold;

                    rtbDoc.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newFontStyle);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error");
            }
        }

        private void ItalicToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            try
            {
                if (!(rtbDoc.SelectionFont == null))
                {
                    System.Drawing.Font currentFont = rtbDoc.SelectionFont;
                    System.Drawing.FontStyle newFontStyle;

                    newFontStyle = rtbDoc.SelectionFont.Style ^ FontStyle.Italic;

                    rtbDoc.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newFontStyle);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error");
            }
        }

        private void UnderlineToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            try
            {
                if (!(rtbDoc.SelectionFont == null))
                {
                    System.Drawing.Font currentFont = rtbDoc.SelectionFont;
                    System.Drawing.FontStyle newFontStyle;

                    newFontStyle = rtbDoc.SelectionFont.Style ^ FontStyle.Underline;

                    rtbDoc.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newFontStyle);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error");
            }
        }

        private void tbrStrikethrough_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(rtbDoc.SelectionFont == null))
                {
                    System.Drawing.Font currentFont = rtbDoc.SelectionFont;
                    System.Drawing.FontStyle newFontStyle;

                    newFontStyle = rtbDoc.SelectionFont.Style ^ FontStyle.Strikeout;

                    rtbDoc.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newFontStyle);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error");
            }
        }

        private void InsertImageToolStripMenuItem_Click(object sender, System.EventArgs e)
        {

            OpenFileDialog1.Title = "RTE - Insert Image File";
            OpenFileDialog1.DefaultExt = "rtf";
            OpenFileDialog1.Filter = "Image Files|*.bmp;*.jpg;*.gif;*.png;*.ico|Bitmap Files|*.bmp|JPEG Files|*.jpg|GIF Files|*.gif|PNG Files|*.png|Icon Files|*.ico|All Files|*.*";
            OpenFileDialog1.FilterIndex = 1;
            OpenFileDialog1.FileName = string.Empty;
            DialogResult result = OpenFileDialog1.ShowDialog();

            if (result != DialogResult.OK || OpenFileDialog1.FileName == "")
            {
                return;
            }

            try
            {
                string strImagePath = OpenFileDialog1.FileName;
                Image img;
                img = Image.FromFile(strImagePath);
                //img.Save(@"c:\temp\test.wmf", System.Drawing.Imaging.ImageFormat.Wmf);
                Clipboard.SetDataObject(img);
                DataFormats.Format df;
                df = DataFormats.GetFormat(DataFormats.Bitmap);
                if (this.rtbDoc.CanPaste(df))
                {
                    this.rtbDoc.Paste(df);
                }
                else
                    SimpleMessageBox.Show(ActGlobals.oFormActMain, "Unknown image format.", "Insert failed");

            }
            catch
            {
                SimpleMessageBox.Show(ActGlobals.oFormActMain, "Unable to insert image format selected.", "Insert Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tbrLeft_Click(object sender, System.EventArgs e)
        {
            rtbDoc.SelectionAlignment = HorizontalAlignment.Left;
        }
        
        private void tbrCenter_Click(object sender, System.EventArgs e)
        {
            rtbDoc.SelectionAlignment = HorizontalAlignment.Center;
        }
        
        private void tbrRight_Click(object sender, System.EventArgs e)
        {
            rtbDoc.SelectionAlignment = HorizontalAlignment.Right;
        }

        private void tbrBullets_Click(object sender, EventArgs e)
        {
            rtbDoc.BulletIndent = 10;
            rtbDoc.SelectionBullet = !rtbDoc.SelectionBullet;
            tbrBullets.Checked = rtbDoc.SelectionBullet;
            tbrNumbered.Checked = rtbDoc.SelectionNumbered; // in case we toggled between bullet & number
        }

        private void tbrNumbered_Click(object sender, EventArgs e)
        {
            rtbDoc.SelectionNumbered = !rtbDoc.SelectionNumbered;
            tbrNumbered.Checked = rtbDoc.SelectionNumbered;
            tbrBullets.Checked = rtbDoc.SelectionBullet; // in case we toggled between bullet & number
        }

        private void tbrIndent_Click(object sender, EventArgs e)
        {
            rtbDoc.SelectionIndent += rtbDoc.IndentPixels;
            if (rtbDoc.SelectionNumbered)
                rtbDoc.EnableListing();
        }

        private void tbrOutdent_Click(object sender, EventArgs e)
        {
            if (rtbDoc.SelectionIndent >= rtbDoc.IndentPixels)
                rtbDoc.SelectionIndent -= rtbDoc.IndentPixels;
            else
                rtbDoc.SelectionIndent = 0;

            if (rtbDoc.SelectionNumbered)
                rtbDoc.EnableListing();
        }

        private void colorStripDropDownFontColor_Click(object sender, ColorPickerEventArgs e)
        {
            rtbDoc.SelectionColor = colorStripDropDownFontColor.Value;
        }

        private void toolDropDownButtonBackground_Click(object sender, ColorPickerEventArgs e)
        {
            rtbDoc.SelectionBackColor = toolDropDownButtonBackground.Value;
        }

        private void tbrSpeaker_Click(object sender, EventArgs e)
        {

            if(rtbDoc.SelectionColor == audioAlertColor)
            {
                rtbDoc.SelectionColor = Color.Black;
            }
            else // add
            {
                rtbDoc.SelectionColor = audioAlertColor;
            }
            rtbDoc_SelectionChanged(null, null);
        }

        private void tbrPopup_Click(object sender, EventArgs e)
        {
            if (rtbDoc.SelectionColor == visualAlertColor)
            {
                rtbDoc.SelectionColor = Color.Black;
            }
            else
            {
                rtbDoc.SelectionColor = visualAlertColor;
            }
            rtbDoc_SelectionChanged(null, null);
        }

        private void tbrAudioVisual_Click(object sender, EventArgs e)
        {
            if (rtbDoc.SelectionColor == audioVisualAlertColor)
            {
                rtbDoc.SelectionColor = Color.Black;
            }
            else
            {
                rtbDoc.SelectionColor = audioVisualAlertColor;
            }
            rtbDoc_SelectionChanged(null, null);
        }

        #endregion Toolbar Methods


        #region Document

        private void rtbDoc_KeyDown(object sender, KeyEventArgs e)
        {
            if (OnUserChangedText != null)
                OnUserChangedText.Invoke(this, new EventArgs());

            if (e.KeyCode == Keys.V && e.Control)
            {
                if(Clipboard.ContainsImage())
                {
                    // richtextbox is particular about pasting images
                    DataFormats.Format df;
                    df = DataFormats.GetFormat(DataFormats.Bitmap);
                    if (this.rtbDoc.CanPaste(df))
                    {
                        // we need to get WMF format for the RichTextBox. Image class supports it. So get an Image
                        Image image = Clipboard.GetImage();
                        // then put it back in the clipboard
                        Clipboard.SetImage(image);
                        // then we can paste
                        this.rtbDoc.Paste(df);
                        e.Handled = true;
                    }
                }
                else if(Clipboard.ContainsText())
                {
                    // just to make things interesting, RTB strips a trailing closing parenthesis from a URL
                    // closing parenthesis is common for eq2 wiki URLs which end like "(Solo)"
                    // so replace trailing ) with its code, if we find any
                    string[] words = Clipboard.GetText().Split(new char[] { ' ', '\n' });
                    foreach (string word in words)
                    {
                        if(word.StartsWith("http"))
                        {
                            if(word.EndsWith(")"))
                            {
                                string url = word.TrimEnd(')') + "%29";
                                string fix = Clipboard.GetText().Replace(word, url);
                                Clipboard.SetText(fix);
                            }
                        }
                    }
                }
            }
        }

        private void rtbDoc_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.LinkText);
            }
            catch (Exception)
            {
                SimpleMessageBox.Show(ActGlobals.oFormActMain, "URL link failed", "Error");
            }
        }

        private void rtbDoc_SelectionChanged(object sender, EventArgs e)
        {
            RichTextBoxEx.RichTextBoxInfo info = rtbDoc.GetListingInfo();
            if (rtbDoc.SelectionFont != null)
            {
                tbrBold.Checked = rtbDoc.SelectionFont.Bold;
                tbrItalic.Checked = rtbDoc.SelectionFont.Italic;
                tbrUnderline.Checked = rtbDoc.SelectionFont.Underline;
                tbrStrikethrough.Checked = rtbDoc.SelectionFont.Strikeout;
                toolDropDownButtonBackground.Value = rtbDoc.SelectionBackColor;
                colorStripDropDownFontColor.Value = rtbDoc.SelectionColor;
            }
            tbrBullets.Checked = rtbDoc.SelectionBullet;
            tbrNumbered.Checked = rtbDoc.SelectionNumbered;
            tbrSpeaker.Checked = rtbDoc.SelectionColor == audioAlertColor;
            tbrPopup.Checked = rtbDoc.SelectionColor == visualAlertColor;
            tbrAudioVisual.Checked = rtbDoc.SelectionColor == audioVisualAlertColor;
        }

        #endregion Document
    }
}