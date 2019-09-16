using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolReader
{
    class RootNodePreferenceReportHolder
    {
        public List<PreferenceReportItem> Properties;

        public int ID;
        public string CLSID;
        public int XMLNodeID;
        public int Type;
        public string Name;
        public string NameProperty;
        public string AddedBy;
        public string Comment;

        public RootNodePreferenceReportHolder()
        {
            Properties = new List<PreferenceReportItem>();
            CLSID = "";           
            Name = "";
            NameProperty = "";
            AddedBy = "";
            Comment = "";
        }
    }
}
