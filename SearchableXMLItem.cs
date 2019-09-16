using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolReader
{
    public class SearchableXMLItem
    {
        public Policy ParentPolicy { get; set; }
        public String Root { get; set; }
        public String Type { get; set; }
        public String Path { get; set; }
        public String ResolvedPath { get; set; }
        public String Name { get; set; }
        public String Value { get; set; }
        public int Linkage { get; set; }
    }
}
