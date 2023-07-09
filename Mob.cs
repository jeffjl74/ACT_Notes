using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ACT_Notes
{
    public class Mob :IEquatable<Mob>, IComparable<Mob>
    {
        [XmlAttribute]
        public string MobName { get; set; }
        [XmlAttribute]
        public int KillOrder { get; set; }
        [XmlAttribute]
        public int AudioDelay { get; set; } = 5;
        [XmlAttribute]
        public int VisualDelay { get; set; } = 5;
        public string Notes { get; set; }

        public int CompareTo(Mob other)
        {
            return this.KillOrder.CompareTo(other.KillOrder);
        }

        public override bool Equals(object obj)
        {
            return obj is Mob mob &&
                   MobName == mob.MobName;
        }

        public bool Equals(Mob other)
        {
            return this.MobName == other.MobName;
        }

        public override int GetHashCode()
        {
            return 1407740742 + EqualityComparer<string>.Default.GetHashCode(MobName);
        }

        public override string ToString()
        {
            return string.Format("{0}. {1}", KillOrder, MobName);
        }
    }
}
