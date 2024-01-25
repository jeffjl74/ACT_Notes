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

            // for mob nodes, compare the kill order
            Mob mx = tx.Tag as Mob;
            Mob my = ty.Tag as Mob;
            if (mx != null && my != null)
                return mx.KillOrder.CompareTo(my.KillOrder);
            else // if these are not mobs, sort by text
                return string.Compare(tx.Text, ty.Text);
        }
    }
}
