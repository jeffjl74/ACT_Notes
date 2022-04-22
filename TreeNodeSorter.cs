using System.Collections;
using System.Windows.Forms;

namespace ACT_Notes
{
    public class TreeNodeSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            var tx = x as TreeNode;
            var ty = y as TreeNode;

            // if these are root nodes sort by name
            if (tx.Level == 0 && ty.Level == 0)
                return string.Compare(tx.Text, ty.Text);

            // for mob nodes, preserve the original order by comparing the kill order
            Mob mx = tx.Tag as Mob;
            Mob my = ty.Tag as Mob;
            if (mx != null && my != null)
                return mx.KillOrder.CompareTo(my.KillOrder);

            // shouldn't get here
            return 0;

        }
    }
}
