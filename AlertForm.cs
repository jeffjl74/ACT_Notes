using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ACT_Notes
{
    public partial class AlertForm : Form
    {
        public List<string> Alerts = new List<string>();
        public int labelHeight = 17;
        public event EventHandler FormMoved; //callback

        // animation modes
        const int HorzPositive = 0X1;
        const int HorzNegative = 0X2;
        const int VertPositive = 0X4;
        const int VertNegative = 0X8;
        const int CENTER = 0X10;
        const int BLEND = 0X80000;
        System.Timers.Timer timer = new System.Timers.Timer();
        int timerTicks;

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
                }
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
