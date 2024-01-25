using System.Windows.Forms;

namespace ACT_Notes
{
    public partial class CommaNamesForm : Form
    {
        public CommaNamesForm()
        {
            InitializeComponent();
        }

        public void AddName(string group, string zone, string mob)
        {
            ListViewGroup lvg = new ListViewGroup(group, group);
            lvg.Name = group;

            bool exists = false;
            foreach (ListViewGroup grp in listView1.Groups)
            {
                if (grp.Name == group)
                {
                    lvg = grp;
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                listView1.Groups.Add(lvg);
            }

            ListViewItem item = new ListViewItem(zone, lvg);
            item.SubItems.Add(mob);
            listView1.Items.Add(item);
        }
    }
}
