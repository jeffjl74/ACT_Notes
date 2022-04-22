using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ACT_Notes
{
    public class Zone : IEquatable<Zone>, IComparable<Zone>
    {
        [XmlAttribute]
        public string ZoneName { get; set; }
        [XmlAttribute]
        public PasteType Paste { get; set; }
        [XmlAttribute]
        public string Prefix { get; set; }
        public string Notes { get; set; }
        public List<Mob> Mobs { get; set; }

        public enum PasteType { Append, Replace, Accept, Ask, Ignore }

        public Zone()
        {
            Mobs = new List<Mob>();
            Paste = PasteType.Append;
        }

        public override bool Equals(object obj)
        {
            return obj is Zone zone &&
                   ZoneName == zone.ZoneName;
        }

        public override int GetHashCode()
        {
            return -67300124 + EqualityComparer<string>.Default.GetHashCode(ZoneName);
        }

        public bool Equals(Zone other)
        {
            return this.ZoneName == other.ZoneName;
        }

        public override string ToString()
        {
            return ZoneName;
        }

        public int CompareTo(Zone other)
        {
            return this.ZoneName.CompareTo(other.ZoneName);
        }
    }
}
