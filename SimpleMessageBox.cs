using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace ACT_Notes
{
    /// <summary>
    /// Class similar to MessageBox.Show() except the dialog placement is controlled by the user,
    /// text is RTF, and non-modal versions are available. 
    /// <para>In keeping with MessageBox, .Show() methods are modal and block the calling thread.
    /// New .ShowDialog() methods are non-modal.</para>
    /// <para>This is opposite the behaviour of Form.Show() and Form.ShowDialog().</para>
    /// <para>Both approaches: 1) matching MessageBox.Show() or 2) matching Form.Show(),
    /// seem to be equally confusing, so we're going with MessageBox.Show() compatibility.</para>
    /// </summary>
    public partial class SimpleMessageBox : Form
    {

        /// <summary>
        /// EventArgs passed to the button event handler
        /// </summary>
        public class OkEventArgs : EventArgs
        {
            /// <summary>
            /// Which button was pressed.
            /// </summary>
            public DialogResult result;
            /// <summary>
            /// The size of the message box when the button was pressed.
            /// </summary>
            public Size formSize;
            /// <summary>
            /// The screen location of the message box when the button was pressed.
            /// </summary>
            public Point formLocation;

            public override string ToString()
            {
                return string.Format("button:{0} size:{1} location:{2}", result.ToString(), formSize, formLocation);
            }
        }

        #region --- Internal Class Data ---

        /// <summary>
        /// Sets the maximum size of the message box by divding the screen height by this number.
        /// e.g. <see cref="maxSizeDivisor"/> = 5 divides the screen height by 5 
        /// making the max size of the message box 20% of the screen height.
        /// </summary>
        int maxSizeDivisor = 5;

        /// <summary>
        /// Positioning info
        /// </summary>
        Point desiredLocation = new Point(-1, -1); //save the location for _Load()
        Control parentControl;

        /// <summary>
        /// Regular expression to find the font table in the rtf
        /// </summary>
        Regex reFonts = new Regex(@"({\\fonttbl({.*;})+})", RegexOptions.Compiled);

        /// <summary>
        /// Debug to Console.Writeline info during the richTextBox1_ContentsResized() method 
        /// </summary>
        int sizePassDebug = 0;

        /// <summary>
        /// Button press event handler
        /// </summary>
        event EventHandler buttonEvent;

        #endregion --- Internal Class Data ---

        #region --- Private Constructors ---

        /// <summary>
        /// Instantiate with all parameters
        /// </summary>
        /// <param name="text">Rich text message. Can be a simple string or or have rtf commands like "line 1\par line 2"</param>
        /// <param name="title">The title for the form.</param>
        /// <param name="parent">The parent control for positioning the message box. Null to disable.</param>
        /// <param name="location">Position for the top left corner of the form. Point(-1, -1) is ignored</param>
        /// <param name="eventHandler">Event hanlder to be called when the [OK] button is pressed</param>
        SimpleMessageBox(string text, string title, Control parent, Point location, EventHandler eventHandler)
        {
            InitializeComponent();
            this.Text = title;
            SetText(text);
            buttonEvent = eventHandler;
            desiredLocation = location;
            parentControl = parent;
        }

        /// <summary>
        /// Chain construtor with no location or event handler
        /// </summary>
        /// <param name="text">Rich text message. Can be a simple string or or have rtf commands like "line 1\par line 2"</param>
        /// <param name="title">The title for the form.</param>
        /// <param name="parent">The parent control</param>
        SimpleMessageBox(string text, string title, Control parent) : this(text, title, parent, new Point(-1, -1), null) { }

        /// <summary>
        /// Chain constructor with no parent, location or event handler
        /// </summary>
        /// <param name="text">Rich text message. Can be a simple string or or have rtf commands like "line 1\par line 2"</param>
        /// <param name="title">The title for the form.</param>
        SimpleMessageBox(string text, string title) : this(text, title, null, new Point(-1, -1), null) { }

        #endregion --- Private Contructors ---

        #region --- Public Show Methods ---

        #region -- Modal --

        /// <summary>
        /// Modal centered on the parent with choice of buttons and icon.
        /// </summary>
        /// <param name="parent">The control in which the popup will be centered</param>
        /// <param name="text">Rich text message. Can be a simple string or or have rtf commands like "line 1\par line 2"</param>
        /// <param name="title">The title for the form.</param>
        /// <param name="buttons"><see cref="MessageBoxButtons"/> to be displayed. Default is OK.</param>
        /// <param name="icon"><see cref="MessageBoxIcon"/> to be displayed in the dialog. Default is none.</param>
        /// <param name="icon"><see cref="MessageBoxIcon"/> to be displayed in the dialog. Default is none.</param>
        /// <param name="defaultButton"><see cref="MessageBoxDefaultButton"/> with the initial focus. 
        /// <see cref="MessageBoxDefaultButton.Button1"/> will be the first (leftmost) button.
        /// <see cref="MessageBoxDefaultButton.Button2"/> will be the second button.
        /// <see cref="MessageBoxDefaultButton.Button3"/> will be the third button.</param>
        /// <returns><see cref="DialogResult"/></returns>
        public static DialogResult Show(Control parent, string text, string title = "",
            MessageBoxButtons buttons = MessageBoxButtons.OK, 
            MessageBoxIcon icon = MessageBoxIcon.None,
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        {
            SimpleMessageBox form = new SimpleMessageBox(text, title, parent);

            SetButtons(form, buttons, defaultButton);

            SetIcon(form, icon);

            // modal show
            return form.ShowDialog();
        }

        /// <summary>
        /// Modal shown at the given location with optional buttons and icon
        /// </summary>
        /// <param name="location">Screen coordinates for the placement of the top left corner of the form.</param>
        /// <param name="text">Rich text message. Can be a simple string or or have rtf controls like "line 1\par line 2"</param>
        /// <param name="title">The title for the form.</param>
        /// <param name="buttons"></param>
        /// <param name="icon"><see cref="MessageBoxIcon"/> to be displayed in the dialog. Default is none.</param>
        /// <param name="defaultButton"><see cref="MessageBoxDefaultButton"/> with the initial focus. 
        /// <see cref="MessageBoxDefaultButton.Button1"/> will be the first (leftmost) button.
        /// <see cref="MessageBoxDefaultButton.Button2"/> will be the second button.
        /// <see cref="MessageBoxDefaultButton.Button3"/> will be the third button.</param>
        public static DialogResult Show(Point location, string text, string title,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.None,
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        {
            SimpleMessageBox form = new SimpleMessageBox(text, title, null, location, null);

            SetButtons(form, buttons, defaultButton);

            SetIcon(form, icon);

            // modal show
            return form.ShowDialog();
        }

        /// <summary>
        /// Modal "shortcut". Show in the middle of the screen with an OK button.
        /// </summary>
        /// <param name="text">Rich text message. Can be a simple string or or have rtf controls like "line 1\par line 2"</param>
        /// <param name="title">The title for the form.</param>
        public static DialogResult Show(string text, string title)
        {
            // goes to the Show(Control, ...) since Point() is not nullable
            return Show(null, text, title);
        }

        #endregion -- Modal --
        
        #region -- Non-Modal --

        /// <summary>
        /// Non-modal centered on the parent with choice of buttons, icon, and event.
        /// </summary>
        /// <param name="parent">The control in which the popup will be centered</param>
        /// <param name="text">Rich text message. Can be a simple string or or have rtf controls like "line 1\par line 2"</param>
        /// <param name="title">The title for the form.</param>
        /// <param name="buttons">Accepts <see cref="MessageBoxButtons"/>. Defaults to <see cref="MessageBoxButtons.OK"/></param>
        /// <param name="handler">Event handler to be called when a button is pressed</param>
        /// <param name="icon"><see cref="MessageBoxIcon"/> to be displayed in the dialog. Default is none.</param>
        /// <param name="defaultButton"><see cref="MessageBoxDefaultButton"/> with the initial focus. 
        /// <see cref="MessageBoxDefaultButton.Button1"/> will be the first (leftmost) button.
        /// <see cref="MessageBoxDefaultButton.Button2"/> will be the second button.
        /// <see cref="MessageBoxDefaultButton.Button3"/> will be the third button.</param>
        public static void ShowDialog(Control parent, string text, string title,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            EventHandler handler = null,
            MessageBoxIcon icon = MessageBoxIcon.None,
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        {
            // since we allow these to be initiated from any thread,
            // which can die at any time since we don't block,
            // and its death kills the form,
            // rather than make separate methods for on-UI-thread and off-UI-thread,
            // just always run on our own thread
            // which won't die until we are done
            var th = new Thread(() =>
            {
                SimpleMessageBox form = new SimpleMessageBox(text, title, parent, new Point(-1, -1), handler);
                form.FormClosing += (s, e) => Application.ExitThread();

                SetButtons(form, buttons, defaultButton);

                SetIcon(form, icon);

                form.Show();
                form.TopMost = true;
                Application.Run();
            });
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }

        /// <summary>
        /// Non-modal shown at the given location with optional buttons, icon, and event
        /// </summary>
        /// <param name="location">Screen coordinates for the placement of the top left corner of the form.</param>
        /// <param name="text">Rich text message. Can be a simple string or or have rtf controls like "line 1\par line 2"</param>
        /// <param name="title">The title for the form.</param>
        /// <param name="buttons"></param>
        /// <param name="handler">Event handler to be called when the [OK] button is pressed</param>
        /// <param name="icon"><see cref="MessageBoxIcon"/> to be displayed in the dialog. Default is none.</param>
        /// <param name="defaultButton"><see cref="MessageBoxDefaultButton"/> with the initial focus. 
        /// <see cref="MessageBoxDefaultButton.Button1"/> will be the first (leftmost) button.
        /// <see cref="MessageBoxDefaultButton.Button2"/> will be the second button.
        /// <see cref="MessageBoxDefaultButton.Button3"/> will be the third button.</param>
        public static void ShowDialog(Point location, string text, string title,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            EventHandler handler = null,
            MessageBoxIcon icon = MessageBoxIcon.None,
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        {
            var th = new Thread(() =>
            {
                SimpleMessageBox form = new SimpleMessageBox(text, title, null, location, handler);
                form.FormClosing += (s, e) => Application.ExitThread();

                SetButtons(form, buttons, defaultButton);

                SetIcon(form, icon);

                form.Show();
                form.TopMost = true;
                Application.Run();
            });
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }

        /// <summary>
        /// Non-modal "shortcut" show in the middle of the screen with an OK button.
        /// </summary>
        /// <param name="text">Rich text message. Can be a simple string or or have rtf controls like "line 1\par line 2"</param>
        /// <param name="title">The title for the form.</param>
        public static void ShowDialog(string text, string title)
        {
            // goes to the ShowDialog(Control, ...) since Point() is not nullable
            ShowDialog(null, text, title);
        }

        #endregion -- Non-Modal --

        #endregion --- Public Show Methods ---

        #region --- Private Methods ---

        /// <summary>
        /// Set the text for the pop up. The text will be horizontally centered in the form unless overridden.
        /// </summary>
        /// <param name="txt">Can be a simple string, rtf group(s), or the entire rtf document</param>
        void SetText(string txt)
        {
            // if the incoming looks like the entire document, just set it
            if(txt.StartsWith(@"{\rtf"))
            {
                richTextBox1.Rtf = txt;
                return;
            }

            // otherwise, merge the incoming with the default document

            //get the empty document already in the RichTextBox
            string rtf = richTextBox1.Rtf;

            //if txt contains a font table, need to merge it with the existing one
            if (txt.Contains(@"{\fonttbl"))
            {
                // find the font table in the richTextBox1.Rtf
                Match match = reFonts.Match(rtf);
                if (match.Success)
                {
                    string existingTable = match.Groups[1].Value;
                    string existingFont = match.Groups[2].Value;
                    //now find the one in txt
                    match = reFonts.Match(txt);
                    if (match.Success)
                    {
                        //update the table in the existing rtf to include fonts from txt
                        string fonts = existingFont + match.Groups[2].Value;
                        string table = @"{\fonttbl" + fonts + "}";
                        rtf = rtf.Replace(existingTable, table);

                        //then remove the table in the incoming txt
                        txt = txt.Replace(match.Groups[1].Value, "");
                    }
                }
            }

            //center the text by default
            string add = @"\qc " + txt + @"\par";

            //need to insert it between the outer {} of the default document
            int end = rtf.LastIndexOf('}');
            string text = rtf.Insert(end, add);
            richTextBox1.Rtf = text;
        }

        /// <summary>
        /// Sets the text and <see cref="DialogResult"/> for each button, and default button for the form.
        /// </summary>
        /// <param name="form"><see cref="SimpleMessageBox"/></param>
        /// <param name="buttons"><see cref="MessageBoxButtons"/> on the form</param>
        /// <param name="defaultButton"><see cref="MessageBoxDefaultButton"/> to set as the default</param>
        static void SetButtons(SimpleMessageBox form, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton)
        {
            switch (buttons)
            {
                case MessageBoxButtons.AbortRetryIgnore:
                    form.button3.Text = "Abort";
                    form.button3.DialogResult = DialogResult.Abort;
                    form.button3.Visible = true;
                    form.button1.Text = "Retry";
                    form.button1.DialogResult = DialogResult.Retry;
                    form.button2.Text = "Ignore";
                    form.button2.DialogResult = DialogResult.Ignore;
                    form.button2.Visible = true;
                    SetDefaultButton(form, defaultButton, 3);
                    break;
                case MessageBoxButtons.OK:
                    form.button1.Text = "OK";
                    form.button1.DialogResult = DialogResult.OK;
                    SetDefaultButton(form, defaultButton, 1);
                    break;
                case MessageBoxButtons.OKCancel:
                    form.button1.Text = "OK";
                    form.button1.DialogResult = DialogResult.OK;
                    form.button2.Text = "Cancel";
                    form.button2.DialogResult = DialogResult.Cancel;
                    form.button2.Visible = true;
                    SetDefaultButton(form, defaultButton, 2);
                    break;
                case MessageBoxButtons.RetryCancel:
                    form.button1.Text = "Retry";
                    form.button1.DialogResult = DialogResult.Retry;
                    form.button2.Text = "Cancel";
                    form.button2.DialogResult = DialogResult.Cancel;
                    form.button2.Visible = true;
                    SetDefaultButton(form, defaultButton, 2);
                    break;
                case MessageBoxButtons.YesNo:
                    form.button1.Text = "Yes";
                    form.button1.DialogResult = DialogResult.Yes;
                    form.button2.Text = "No";
                    form.button2.DialogResult = DialogResult.No;
                    form.button2.Visible = true;
                    SetDefaultButton(form, defaultButton, 2);
                    break;
                case MessageBoxButtons.YesNoCancel:
                    form.button3.Text = "Yes";
                    form.button3.DialogResult = DialogResult.Yes;
                    form.button3.Visible = true;
                    form.button1.Text = "No";
                    form.button1.DialogResult = DialogResult.No;
                    form.button2.Text = "Cancel";
                    form.button2.DialogResult = DialogResult.Cancel;
                    form.button2.Visible = true;
                    SetDefaultButton(form, defaultButton, 3);
                    break;
                default:
                    form.button1.Text = "OK";
                    form.button1.DialogResult = DialogResult.OK;
                    SetDefaultButton(form, defaultButton, 1);
                    break;
            }
        }

        /// <summary>
        /// Set the icon for the form.
        /// </summary>
        /// <param name="form"><see cref="SimpleMessageBox"/></param>
        /// <param name="icon"><see cref="MessageBoxIcon"/> for the form</param>
        static void SetIcon(SimpleMessageBox form, MessageBoxIcon icon)
        {
            if (icon != MessageBoxIcon.None)
            {
                //going to put the icon in cell 0,2 (col,row)
                // need to move the textbox to the right by one column to cell 1,0 (col, row)
                form.tableLayoutPanel1.Controls.Add(form.richTextBox1, 1, 0);
                form.tableLayoutPanel1.SetColumnSpan(form.richTextBox1, 3);

                //add the icon
                PictureBox pic = new PictureBox();
                switch (icon)
                {
                    case MessageBoxIcon.Question:
                        pic.Image = SystemIcons.Question.ToBitmap();
                        break;
                    case MessageBoxIcon.Warning:
                        pic.Image = SystemIcons.Warning.ToBitmap();
                        break;
                    case MessageBoxIcon.Error:
                        pic.Image = SystemIcons.Error.ToBitmap();
                        break;
                    default:
                        pic.Image = SystemIcons.Information.ToBitmap();
                        break;
                }
                pic.Anchor = AnchorStyles.None;
                form.tableLayoutPanel1.Controls.Add(pic, 0, 2);
            }
        }

        /// <summary>
        /// Sets the default button based on the number of buttons shown and the specified button
        /// </summary>
        /// <param name="form"><see cref="SimpleMessageBox"/> form containing the buttons</param>
        /// <param name="button"><see cref="MessageBoxDefaultButton"/> to set as default</param>
        /// <param name="buttonCount">Number of buttons visible on the form</param>
        static void SetDefaultButton(SimpleMessageBox form, MessageBoxDefaultButton button, int buttonCount)
        {
            if (buttonCount == 1)
                form.ActiveControl = form.button1;
            else if (buttonCount == 2)
            {
                if (button == MessageBoxDefaultButton.Button2)
                    form.ActiveControl = form.button2;
                else
                    form.ActiveControl = form.button1;
            }
            else
            {
                if (button == MessageBoxDefaultButton.Button1)
                    form.ActiveControl = form.button3;
                else if (button == MessageBoxDefaultButton.Button2)
                    form.ActiveControl = form.button1;
                else
                    form.ActiveControl = form.button2;
            }
        }

        /// <summary>
        /// Trigger the callback event, if available
        /// </summary>
        protected virtual void OnButtonPressed(EventArgs e)
        {
            if (buttonEvent != null)
            {
                buttonEvent.Invoke(this, e);
            }
        }

        /// <summary>
        /// When a button is clicked, start the event callback and close the form
        /// </summary>
        private void button_Click(object sender, EventArgs e)
        {
            DialogResult result = ((Button)sender).DialogResult;
            OkEventArgs arg = new OkEventArgs { result = result, formSize = this.Size, formLocation = this.Location };
            OnButtonPressed(arg);
            this.Close();
        }

        /// <summary>
        /// Positions the form to avoid the flicker at its default location.
        /// </summary>
        private void SimpleMessageBox_Load(object sender, EventArgs e)
        {
            bool done = false;
            if (parentControl != null)
            {
                // parent size will not be known if it has not been shown
                if (parentControl.IsHandleCreated)
                {
                    int x = parentControl.Location.X + parentControl.Width / 2 - this.Width / 2;
                    int y = parentControl.Location.Y + parentControl.Height / 2 - this.Height / 2;
                    Point screen = new Point(x, y);
                    this.Location = screen;
                    done = true;
                }
            }
            if (!done && desiredLocation.X >= 0 && desiredLocation.Y >= 0)
            {
                this.Location = desiredLocation;
                done = true;
            }
            if (!done)
            {
                //center screen by default
                Rectangle screen = Screen.FromControl(this).Bounds;
                int x = (screen.Width - this.Width) / 2;
                int y = (screen.Height - this.Height) / 2;
                Point client = new Point(x, y);
                this.Location = client;
            }

            this.TopMost = true;
        }

        /// <summary>
        /// Resize the text box to fit its contents, within limits
        /// </summary>
        /// <remarks>This gets called several times per form instantiation</remarks>
        private void richTextBox1_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            sizePassDebug++;            //debug
            //bool isAdjusted = false;    //debug

            int diff = 0;   //calculated size difference

            //since we are changing the rtf height indirectly
            // by changing the size of the form
            // which changes the tablelayout
            // which changes the richtextbox
            //add some hysteresis
            int pad = 7;

            //Divide the screen height to get a max form height.
            //e.g. divisor of 5 = 20% of the screen = height limit. The scroll bar will show up if needed.
            int maxHeight = Screen.FromControl(this).Bounds.Height / maxSizeDivisor;

            // we get serveral small, or small difference, height calls,
            // so use the pad to minimze "bouncing" between close values.
            if (e.NewRectangle.Height > richTextBox1.Height)
            {
                // change the height of the form, which will change the tablelayout cell sizes
                if (e.NewRectangle.Height < maxHeight)
                {
                    // grow if we have not reached max size
                    diff = (e.NewRectangle.Height - richTextBox1.Height) + pad;
                    if (diff > pad)
                    {
                        this.Height += diff;
                        //isAdjusted = true;  //debug
                    }
                }
                else
                {
                    // set to max size
                    diff = maxHeight - this.Height;
                    if (diff > pad)
                    {
                        this.Height = maxHeight;
                        //isAdjusted = true;  //debug
                    }
                }
            }

            // check for shrinking
            if (e.NewRectangle.Height + pad < richTextBox1.Height)
            {
                diff = richTextBox1.Height - e.NewRectangle.Height - pad;
                if (diff > pad)
                {
                    // minimum size is set in the designer
                    if (this.Height - diff > this.MinimumSize.Height)
                    {
                        this.Height -= diff;
                        //isAdjusted = true;  //debug
                    }
                }
            }

            // this method gets called a lot. Some debug to watch it.
            //Console.WriteLine(string.Format("Exit {sizePassDebug} - newRect height:{e.NewRectangle.Height}, form height:{this.Height}, rtf height:{richTextBox1.Height} Changed:{isAdjusted}, diff:{diff}"));

        }

        /// <summary>
        /// Process hyperlinks
        /// </summary>
        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.LinkText);
            }
            catch (Exception)
            {
                // leave it up to the user
                throw;
            }
        }

        #endregion --- Private Methods ---
    }
}
