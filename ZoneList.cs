using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Serialization;
using static ACT_Notes.Zone;

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
        [XmlAttribute]
        public string Version { get; set; } = "1.3.0.0";

        public List<Zone> Zones { get; set; }

        public NoteGroupsList NoteGroups { get; set; }

        public ZoneList()
        {
            Zones = new List<Zone>();
            NoteGroups = new NoteGroupsList();
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


    public partial class NoteGroupsList : List<NoteGroup>, IComparer<NoteGroup>
    {
      public string GetGroupName(string zone)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Zones.Contains(zone))
                    return this[i].GroupName;
            }
            return string.Empty;
        }

        public NoteGroup AddGroup(string group)
        {
            NoteGroup result = null;
            NoteGroup grp = new NoteGroup { GroupName = group };
            if (!this.Contains(grp))
            {
                this.Add(grp);
                result = grp;
            }
            return result;
        }

        public void AddNoteIfNotGrouped(string group, string category)
        {
            string existingGrp = GetGroupName(category);
            // only need a new group if it doesn't have and existing one
            if (existingGrp == string.Empty || existingGrp == Notes.DefaultGroupName)
            {
                //Debug.WriteLine($"adding group {group} category {category}");
                NoteGroup grp = this[group];
                if (grp == null)
                {
                    //Debug.WriteLine($"** new group {group}");
                    AddGroup(group);
                    grp = this[group];
                }
                if (existingGrp == Notes.DefaultGroupName)
                {
                    //Debug.WriteLine($"** {category} removed from default");
                    NoteGroup defgrp = this[Notes.DefaultGroupName];
                    defgrp.Zones.Remove(category);
                }
                if (grp != null)
                {
                    if (!grp.Zones.Contains(category))
                    {
                        //Debug.WriteLine($"** added {category} to {group}");
                        grp.Zones.Add(category);
                    }
                }
            }
            else
            {
                //Debug.WriteLine($"not moving {category} from {existingGrp}");
            }
        }

        public void PutNoteInGroup(string group, string category)
        {
            string existingGrp = GetGroupName(category);
            //Debug.WriteLine($"Put in group {group} category {category}");
            NoteGroup grp = this[group];
            if (grp == null)
            {
                //Debug.WriteLine($"** new group {group}");
                AddGroup(group);
                grp = this[group];
            }
            if (grp != null && !string.IsNullOrEmpty(existingGrp) && !grp.GroupName.Equals(existingGrp))
            {
                //Debug.WriteLine($"** {category} removed from {existingGrp}");
                NoteGroup defgrp = this[existingGrp];
                defgrp.Zones.Remove(category);
            }
            if (grp != null)
            {
                if (!grp.Zones.Contains(category))
                {
                    //Debug.WriteLine($"** added {category} to {group}");
                    grp.Zones.Add(category);
                }
            }
        }

        public int Compare(NoteGroup x, NoteGroup y)
        {
            return x.GroupName.CompareTo(y.GroupName);
        }

        public NoteGroup this[string name]
        {
            get { return this.FirstOrDefault(t => t.GroupName == name); }
        }
    }

    public partial class NoteGroup : IEqualityComparer<NoteGroup>, IComparer<NoteGroup>
    {
        [XmlAttribute]
        public string GroupName;

        [XmlAttribute]
        public bool Collapsed = false;

        [XmlAttribute]
        public PasteType Paste { get; set; }

        [XmlAttribute]
        public string Prefix { get; set; }


        [XmlAttribute]
        public int AudioDelay { get; set; } = 5;

        [XmlAttribute]
        public int VisualDelay { get; set; } = 5;
        
        public string Notes { get; set; }

        public List<string> Zones = new List<string>();

        public int Compare(NoteGroup x, NoteGroup y)
        {
            return x.GroupName.CompareTo(y.GroupName);
        }

        public bool Equals(NoteGroup x, NoteGroup y)
        {
            return x.GroupName.Equals(y.GroupName);
        }

        public int GetHashCode(NoteGroup obj)
        {
            return GroupName.GetHashCode();
        }

        public override string ToString()
        {
            return GroupName;
        }
    }

}
