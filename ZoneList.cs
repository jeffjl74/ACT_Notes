using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;

namespace ACT_Notes
{
    [XmlRoot]
    public class ZoneList
    {
        [XmlAttribute]
        public int SplitterLoc { get; set; }
        [XmlAttribute]
        public bool CompressImages { get; set; }
        [XmlAttribute]
        public int AlertX { get; set; }
        [XmlAttribute]
        public int AlertY { get; set; }
        [XmlAttribute]
        public int AlertWidth { get; set; }
        public List<Zone> Zones { get; set; }

        public ZoneList()
        {
            Zones = new List<Zone>();
            SplitterLoc = -1;
            CompressImages = false;
        }

        public Zone this[string key]
        {
            get
            {
                return Zones.Find(x => x.ZoneName == key);
            }
        }
    }
}
