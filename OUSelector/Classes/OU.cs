using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices;

namespace PolReader.OUSelector
{
    public class OU
    {
        public OU(string ouName, DirectoryEntry directoryEntry)
        {
            this.OUName = ouName;
            this.DirectoryEntry = directoryEntry;
        }

        public string OUName { get; private set; }
        public DirectoryEntry DirectoryEntry { get; private set; }

        readonly List<OU> _ous = new List<OU>();
        public List<OU> ous
        {
            get { return _ous; }
        }
    }
}
