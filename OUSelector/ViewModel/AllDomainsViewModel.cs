using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace PolReader.OUSelector
{
    public class AllDomainsViewModel
    {
        readonly ReadOnlyCollection<DomainViewModel> _domains;

        public AllDomainsViewModel(Domain[] domains)
        {
            _domains = new ReadOnlyCollection<DomainViewModel>(
                (from domain in domains
                 select new DomainViewModel(domain))
                .ToList());
        }

        public ReadOnlyCollection<DomainViewModel> Domains
        {
            get { return _domains; }
        }
    }
}
