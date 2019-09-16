using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolReader.OUSelector
{
    public class OUViewModel : TreeViewMVItem
    {
        readonly OU _ou;

        public OUViewModel(OU ou, TreeViewMVItem parentItem)
            : base(parentItem, true)
        {
            _ou = ou;
        }

        public OU ou
        {
            get { return _ou; }
        }

        public string OUName
        {
            get { return _ou.OUName; }
        }

        protected override void LoadChildren()
        {
            foreach (OU ou in ADQuery.GetOUs(_ou))
                base.Children.Add(new OUViewModel(ou, this));
        }
    }
}
