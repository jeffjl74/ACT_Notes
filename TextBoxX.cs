using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ACT_Notes
{
    public partial class TextBoxX : TextBox
    {
        private readonly Label lblClear;

        // new event handler for the X "button"
        [Browsable(true)]
        [Category("Action")]
        [Description("Invoked when user clicks X")]
        public event EventHandler ClickX;

        // required TextBox stuff
        public bool ButtonTextClear { get; set; }
        public AutoScaleMode AutoScaleMode;

        public TextBoxX()
        {
            InitializeComponent();

            ButtonTextClear = true;

            Resize += PositionX;

            lblClear = new Label()
            {
                Location = new Point(100, 0),
                AutoSize = true,
                Text = " X ",
                ForeColor = Color.Gray,
                Font = new Font("Tahoma", 8.25F),
                Cursor = Cursors.Arrow
            };

            Controls.Add(lblClear);
            lblClear.Click += LblClear_Click;
            lblClear.BringToFront();
        }

        private void LblClear_Click(object sender, EventArgs e)
        {
            Text = string.Empty; 
            ButtonX_Click(sender, e);
        }

        protected void ButtonX_Click(object sender, EventArgs e)
        {
            // report the event to the parent
            if (ClickX != null)
                ClickX(this, e);
        }

        private void PositionX(object sender, EventArgs e)
        { 
            lblClear.Location = new Point(Width - lblClear.Width, ((Height - lblClear.Height) / 2) - 1); 
        }
    }
}
