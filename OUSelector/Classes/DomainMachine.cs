using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolReader.OUSelector
{
    class DomainMachine
    {
        public DomainMachine(string Name, string Path)
        {
            this.Name = Name;
            this.Path = Path;
        }

        public string Name { get; private set; }
        public string Path { get; private set; }
    }
}
