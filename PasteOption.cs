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
    public partial class PasteOption : Form
    {
        System.Timers.Timer timer;
        const int timeout = 30;
        int ticks = 0;
        Form _owner = null;

        public PasteOption(Form owner, string desc)
        {
            InitializeComponent();

            _owner = owner;
            Owner = owner;
            labelWhat.Text = desc;
        }

        private void PasteOption_Shown(object sender, EventArgs e)
        {
            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += Timer_Elapsed;
            timer.Stop();
            timer.Start();

            if (_owner != null)
            {
                Point p = new Point(_owner.Left + _owner.Width / 2 - this.Width / 2, _owner.Top + _owner.Height / 2 - this.Height / 2);
                this.Location = p;
            }
            this.TopMost = true;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ticks++;
            if (ticks > timeout)
            {
                timer.Stop();
                Debug.WriteLine("timeout append");
                DialogResult = DialogResult.Yes;
                Close();
            }
            else
            {
                labelCountdown.Text = $"({30 - ticks} seconds until it is appended)";
            }
        }

        private void PasteOption_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
        }
    }
}
