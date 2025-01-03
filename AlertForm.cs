using Advanced_Combat_Tracker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ACT_Notes
{
    public partial class AlertForm : Form
    {

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public List<string> Alerts = new List<string>();
        public int labelHeight = 17;
        public event EventHandler FormMoved; //callback

        System.Timers.Timer timer = new System.Timers.Timer();
        int timerTicks;

        private IntPtr previousWindowHandle = IntPtr.Zero;

        // form drag
        bool mouseDown;
        Point lastLocation;

        class AlertLabel : Label
        {
            public AlertLabel(AlertForm form, string text, bool isFirst)
            {
                if (isFirst)
                {
                    this.Font = new Font(this.Font.Name, this.Font.SizeInPoints, FontStyle.Underline);
                    this.ForeColor = Color.Blue;
                    this.Cursor = Cursors.Hand;
                    this.Tag = true;
                }
                else
                    this.Tag= false;
                this.Text = text;
                this.AutoSize = true;
                this.Location = new Point(5, form.labelHeight);
                this.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left;
                this.MouseDown += form.AlertForm_MouseDown;
                this.MouseUp += form.AlertForm_MouseUp;
                this.MouseMove += form.AlertForm_MouseMove;
            }
        }

        public AlertForm()
        {
            InitializeComponent();
        }

        // do not take the focus when the form is shown
        // but we do want topmost
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }
        private const int WS_EX_TOPMOST = 0x00000008;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= WS_EX_TOPMOST;
                return createParams;
            }
        }

        private void AlertForm_Load(object sender, EventArgs e)
        {
            if (Alerts.Count > 0)
            {
                labelHeight = 0;
                for (int i = 0; i < Alerts.Count; i++)
                {
                    AlertLabel alertLabel = new AlertLabel(this, Alerts[i], i==0);
                    this.Controls.Add(alertLabel);
                    // with autosize set, the height is adjusted when the control is added to the form
                    // so grab it now
                    labelHeight += alertLabel.Height;
                }
                this.Height = labelHeight + (this.Height - this.ClientRectangle.Height);
                timer.Interval = 1000;
                timer.SynchronizingObject = this;
                timerTicks = 0;
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
            }
        }

        private void AlertForm_Shown(object sender, EventArgs e)
        {
            // save whoever had focus before us (probably the game)
            previousWindowHandle = GetForegroundWindow();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(this.BackColor != Color.LawnGreen)
                this.BackColor = Color.LawnGreen;
            else
                this.BackColor = SystemColors.Control;
            if (++timerTicks >= 4)
                timer.Stop();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void AlertForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Clicks == 1)
            {
                mouseDown = true;
                lastLocation = e.Location;
            }
        }

        private void AlertForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void AlertForm_MouseUp(object sender, MouseEventArgs e)
        {
            Label label = sender as Label;
            if (label != null && (bool)label.Tag == true)
            {
                try
                {
                    // top label is a click-able ACT activator
                    List<ActPluginData> plugins = ActGlobals.oFormActMain.ActPlugins;
                    for (int i = 0; i < plugins.Count; i++)
                    {
                        ActPluginData plugin = plugins[i];
                        if (plugin.lblPluginTitle.Text != "Notes.dll")
                            continue;

                        //Debug.WriteLine("activating notes plugin tab");
                        Notes notes = (Notes)plugin.pluginObj;
                        TabControl pluginsTabControl = notes.Parent.Parent as TabControl;
                        if (pluginsTabControl == null)
                            break;

                        // select the Notes tab in the plugins tabs
                        TabPage myTab = notes.Parent as TabPage;
                        if (myTab == null)
                            break;
                        pluginsTabControl.SelectedTab = myTab;

                        TabControl mainTabControl = pluginsTabControl.Parent.Parent as TabControl;
                        if (mainTabControl != null)
                        {
                            for (int j = 0; j < mainTabControl.TabPages.Count; j++)
                            {
                                var tab = mainTabControl.TabPages[j];
                                if (tab.Text == "Plugins")
                                {
                                    // select the Plugins tab in ACT's tabs
                                    mainTabControl.SelectedTab = tab;
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
                catch { }
                if (previousWindowHandle != IntPtr.Zero)
                {
                    // restore focus to the previous window, likely the game
                    SetForegroundWindow(previousWindowHandle);
                }
            }

            mouseDown = false;
            OnMoveDone(e);
        }

        protected void OnMoveDone(EventArgs e)
        {
            if(FormMoved != null)
            {
                FormMoved.Invoke(this, e);
            }
        }

        private void AlertForm_ResizeEnd(object sender, EventArgs e)
        {
            OnMoveDone(e);
        }
    }
}
