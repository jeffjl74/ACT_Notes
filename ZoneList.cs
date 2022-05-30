using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public List<Zone> Zones { get; set; }

        public ZoneList()
        {
            Zones = new List<Zone>();
            SplitterLoc = -1;
            CompressImages = false;
        }
    }
}
