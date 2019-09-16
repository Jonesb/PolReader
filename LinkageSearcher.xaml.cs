using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace PolReader
{
    /// <summary>
    /// Interaction logic for LinkageSearcher.xaml
    /// </summary>
    public partial class LinkageSearcher : Window
    {
        ListSortDirection _lastDirection = ListSortDirection.Ascending;
        GridViewColumnHeader _lastHeaderClicked = null;

        List<LinkageItem> SourcePolicyItems;
        ObservableCollection<LinkageItem> CurrentPolicyItems = new ObservableCollection<LinkageItem>();


        public LinkageSearcher(List<LinkageItem> sourcePolicyItems)
        {
            InitializeComponent();

            SourcePolicyItems = sourcePolicyItems;

            foreach (LinkageItem spi in SourcePolicyItems)
            {
                CurrentPolicyItems.Add(spi);
            }

            PolicyItems.ItemsSource = CurrentPolicyItems;

            CommandBinding cb = new CommandBinding(ApplicationCommands.Copy, CopyCmdExecuted, CopyCmdCanExecute);
            PolicyItems.CommandBindings.Add(cb);
        }

        void GridViewColumnHeaderClickedHandler(object sender,
                                         RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked =
                  e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {

                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {

                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    if (headerClicked.Column.Header.ToString() == "Linked")
                    {
                        Sort("ParentPolicy.LinkedTo.Count", direction);
                    }
                    else
                    {
                        Binding binding = (Binding)(headerClicked.Column.DisplayMemberBinding);
                        Sort(binding.Path.Path, direction);
                    }
                    

                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.HeaderTemplate = null;
                    }


                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }


        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView =
              CollectionViewSource.GetDefaultView(PolicyItems.ItemsSource);
            if (dataView != null)
            {
                dataView.SortDescriptions.Clear();
                SortDescription sd = new SortDescription(sortBy, direction);
                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();
            }
        }

        void CopyCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            ListView lv = e.OriginalSource as ListView;

            string copyContent = String.Empty;

            int count = 0;
            foreach (object item in lv.SelectedItems)
            {
                if (count > 0)
                    copyContent += Environment.NewLine;

                if (item is LinkageItem)
                {
                    LinkageItem policyItem = (LinkageItem)item;
                    copyContent += policyItem.ParentPolicy.Name + "\t" + policyItem.Linkage;
                }
                count++;
            }

            try
            {
                Clipboard.SetData(DataFormats.Text, copyContent);
            }
            catch
            {

            }
        }

        void CopyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ListView lv = e.OriginalSource as ListView;
            // CanExecute only if there is one or more selected Item.   

            if (lv.SelectedItem != null)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        public void FilterData()
        {
            if (PolicyItems != null)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(PolicyItems.ItemsSource);
                view.Filter = null;                
                view.Filter = new Predicate<object>(FilterListView);
            }
        }
        public bool FilterListView(Object item)
        {
            LinkageItem SAi = (LinkageItem)item;

            // return (OUFilter.Text.Trim().Length == 0 || Spi.ParentPolicy.IsLinked(OUFilter.Text)) && ((FilterText.Text.Length < 1 || (Spi.Key.ToUpper().Contains(FilterText.Text.ToUpper()) ||
            return ((FilterText.Text.Length < 1 || (SAi.ParentPolicy.Name.ToUpper().Contains(FilterText.Text.ToUpper())) || (SAi.Linkage.ToUpper().Contains(FilterText.Text.ToUpper()))));
        }

        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterData();
        }
        private void PolicyItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DependencyObject src = (DependencyObject)(e.OriginalSource);

            while (!(src is Control))
                src = VisualTreeHelper.GetParent(src);

            if (src.GetType().Equals(typeof(ListViewItem)))
            {
                PolicyView PolView = new PolicyView(((LinkageItem)PolicyItems.SelectedItem).ParentPolicy);
                
                PolView.Owner = this;
                PolView.ShowDialog();
            }
        }
    }
}
