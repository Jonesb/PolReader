using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices;

namespace PolReader.OUSelector
{
    class DomainUser
    {

        public DomainUser(string Name, string Path)
        {
            this.Name = Name;
            this.Path = Path;
        }

        public string Name { get; private set; }
        public string Path { get; private set; }

    }
}
