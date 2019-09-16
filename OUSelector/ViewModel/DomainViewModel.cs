using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolReader.OUSelector
{
    public class DomainViewModel : TreeViewMVItem
    {
        readonly Domain _domain;

        public DomainViewModel(Domain domain) 
            : base(null, true)
        {
            _domain = domain;
        }


        public Domain domain
        {
            get { return _domain; }
        }

        public string DomainName
        {
            get { return _domain.DomainName; }
        }

        protected override void LoadChildren()
        {
            foreach (OU ou in ADQuery.GetOUs(_domain))
                base.Children.Add(new OUViewModel(ou, this));
        }
    }
}
