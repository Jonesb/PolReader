using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolReader
{
    public class SearchablePolicyItem
    {
        public Policy ParentPolicy { get; set; }
        public int Linkage { get; set; }
        public String Key { get; set; }
        public String Value { get; set; }
        public String Context { get; set; }
        public String Data { get; set; }
        public String Type { get; set; } 

    }
}
