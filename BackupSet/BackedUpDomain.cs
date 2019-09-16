using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolReader.BackupSet
{
    public class BackedUpDomain
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public List<BackupSet> BackupSets { get; set; }

        public BackedUpDomain(int DomainID,String DomainName)
        {
            ID = DomainID;
            Name = DomainName;

            BackupSets = DB.GetAllDomainBackupSets(ID);
        }

    }
}
