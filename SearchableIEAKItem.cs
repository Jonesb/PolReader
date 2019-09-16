using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolReader
{
    public class SearchableIEAKItem : SearchableSecEditItem
    {
        public bool MachineSetting { get; set; }

        public string PolicyType
        {
            get
            {
                if (MachineSetting)
                    return "Machine";
                else
                    return "User";
            }
        }
    }
}
