using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PolReader.DiffingClasses
{
    public class BaseDiffViewItem : INotifyPropertyChanged
    {
        static readonly BaseDiffViewItem DummyChild = new BaseDiffViewItem();

        readonly ObservableCollection<BaseDiffViewItem> _children;
        readonly BaseDiffViewItem _parent;

        bool _isExpanded;
        bool _isSelected;

        public BaseDiffViewItem(BaseDiffViewItem parent)
        {
            _parent = parent;

            _children = new ObservableCollection<BaseDiffViewItem>();

        }

        public string Name { get; set; }
        public string IconString { get; set; }

        // This is used to create the DummyChild instance.
        private BaseDiffViewItem()
        {
        }

        public ObservableCollection<BaseDiffViewItem> Children
        {
            get { return _children; }
        }

        public bool HasDummyChild
        {
            get { return this.Children.Count == 1 && this.Children[0] == DummyChild; }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                    _parent.IsExpanded = true;

                // Lazy load the child items, if necessary.
                if (this.HasDummyChild)
                {
                    this.Children.Remove(DummyChild);
                    this.LoadChildren();
                }
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        protected virtual void LoadChildren()
        {
        }


        public BaseDiffViewItem Parent
        {
            get { return _parent; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
