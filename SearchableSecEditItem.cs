using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolReader
{
    public class SearchableSecEditItem
    {
        public Policy ParentPolicy { get; set; }
        public SecEditSection ParentSection { get; set; }                
        public String Name { get; set; }
        public String Value { get; set; }
        public int Linkage { get; set; }
    }
}
