using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ACT_Notes
{
    public partial class DelaysForm : Form
    {
        public int audioDelay { get; set; }
        public int visualDelay { get; set; }

        public DelaysForm()
        {
            InitializeComponent();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            audioDelay = (int)numericUpDownAudio.Value;
            if (audioDelay < 0)
                audioDelay = 5;
            visualDelay = (int)numericUpDownVisual.Value;
            if (visualDelay < 0)
                visualDelay = 5;
        }

        private void DelaysForm_Shown(object sender, EventArgs e)
        {
            numericUpDownAudio.Value = audioDelay;
            numericUpDownVisual.Value = visualDelay;
        }
    }
}
