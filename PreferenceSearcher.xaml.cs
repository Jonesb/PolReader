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
using System.IO;

namespace PolReader
{
    /// <summary>
    /// Interaction logic for PreferenceSearcher.xaml
    /// </summary>
    public partial class PreferenceSearcher : Window
    {       
        ListSortDirection _lastDirection = ListSortDirection.Ascending;
        GridViewColumnHeader _lastHeaderClicked = null;

        List<SearchableXMLItem> SourcePolicyItems;
        ObservableCollection<SearchableXMLItem> CurrentPolicyItems = new ObservableCollection<SearchableXMLItem>();

        public PreferenceSearcher(List<SearchableXMLItem> sourcePolicyItems)
        {
            InitializeComponent();

            SourcePolicyItems = sourcePolicyItems;

            foreach(SearchableXMLItem spi in SourcePolicyItems)
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

        public PreferenceSearcher()
        {
            InitializeComponent();

            CommandBinding cb = new CommandBinding(ApplicationCommands.Copy, CopyCmdExecuted, CopyCmdCanExecute);
            PolicyItems.CommandBindings.Add(cb);
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

                if (item is SearchableXMLItem)
                {
                    SearchableXMLItem policyItem = (SearchableXMLItem)item;
                    copyContent += policyItem.ParentPolicy.Name + "\t" + policyItem.Type + "\t" + policyItem.Root + "\t" + policyItem.Path + "\t" + policyItem.ResolvedPath +  "\t" + policyItem.Name + "\t" + policyItem.Value;
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

        public void FilterData()
        {
            if (PolicyItems != null)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(PolicyItems.ItemsSource);
                view.Filter = null;

                if (OUFilter.Text.Trim().Length > 0)
                {
                    GPOManagement gman = new GPOManagement();

                    string OULocation = OUFilter.Text;

                    while (!OULocation.StartsWith("OU=") && OULocation.Contains("OU="))
                    {
                        OULocation=OULocation.Substring(OULocation.IndexOf(",")+1);
                    }

                    string[] LinkageOrder = gman.GetLinkageOrder(OULocation);

                    CurrentPolicyItems.Clear();

                    int LinkCount = 1;
                    foreach (string Link in LinkageOrder)
                    {
                        foreach (SearchableXMLItem spi in SourcePolicyItems)
                        {
                            if (spi.ParentPolicy.GUID.Equals(Link))
                            {
                                spi.Linkage = LinkCount;
                                CurrentPolicyItems.Add(spi);                                
                            }
                        }

                        LinkCount++;
                    }

                }
                else
                {
                    CurrentPolicyItems.Clear();

                    foreach (SearchableXMLItem spi in SourcePolicyItems)
                    {
                        spi.Linkage = -1;
                        CurrentPolicyItems.Add(spi);                        
                    }
                }

                view.Filter = new Predicate<object>(FilterListView);                
            }
        }

        public bool FilterListView(Object item)
        {
            SearchableXMLItem Spi = (SearchableXMLItem)item;

           // return (OUFilter.Text.Trim().Length == 0 || Spi.ParentPolicy.IsLinked(OUFilter.Text)) && ((FilterText.Text.Length < 1 || (Spi.Key.ToUpper().Contains(FilterText.Text.ToUpper()) ||
            return ((FilterText.Text.Length < 1 || (Spi.Name.ToUpper().Contains(FilterText.Text.ToUpper()) ||
                        Spi.ParentPolicy.Name.ToUpper().Contains(FilterText.Text.ToUpper()) ||
                        Spi.Value.ToUpper().Contains(FilterText.Text.ToUpper()) ||
                        Spi.Path.ToUpper().Contains(FilterText.Text.ToUpper()) ||
                        Spi.Root.ToUpper().Contains(FilterText.Text.ToUpper()))) &&
                        ((IncludeMachine.IsChecked.Value && Spi.Type.Equals("Machine")) ||
                    (IncludeUser.IsChecked.Value && Spi.Type.Equals("Users"))) &&
                    (IncludeUnlink.IsChecked.Value ||
                    (!IncludeUnlink.IsChecked.Value && Spi.ParentPolicy.LinkedTo.Count>0)));

            
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
                PolicyView PolView = new PolicyView(((SearchableXMLItem)PolicyItems.SelectedItem).ParentPolicy);
                
                PolView.Owner = this;
                PolView.ShowDialog();
            }
        }

        private void IncludeCheckEvent(object sender, RoutedEventArgs e)
        {
            FilterData();
        }



        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            
            System.Windows.Forms.SaveFileDialog fbDialog = new System.Windows.Forms.SaveFileDialog();
            fbDialog.Filter = "Comma Seperated File|*.csv";

            if (fbDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextWriter OutPut = new StreamWriter(fbDialog.FileName);

                OutPut.WriteLine("\"Policy\",\"Linkages\",\"Type\",\"Root\",\"Path\",\"Name\",\"Value\"");

                foreach (SearchableXMLItem pol in (ObservableCollection<SearchableXMLItem>)PolicyItems.ItemsSource)
                {

                    if ((FilterText.Text.Length < 1 || (pol.Name.ToUpper().Contains(FilterText.Text.ToUpper()) ||
                        pol.ParentPolicy.Name.ToUpper().Contains(FilterText.Text.ToUpper()) ||
                        pol.Value.ToUpper().Contains(FilterText.Text.ToUpper()) ||
                        pol.Path.ToUpper().Contains(FilterText.Text.ToUpper()) ||
                        pol.ResolvedPath.ToUpper().Contains(FilterText.Text.ToUpper()) ||
                        pol.Root.ToUpper().Contains(FilterText.Text.ToUpper()))) &&
                        ((IncludeMachine.IsChecked.Value && pol.Type.Equals("Machine")) ||
                    (IncludeUser.IsChecked.Value && pol.Type.Equals("Users"))) &&
                    (IncludeUnlink.IsChecked.Value ||
                    (!IncludeUnlink.IsChecked.Value && pol.ParentPolicy.LinkedTo.Count > 0)))
                    {
                        OutPut.WriteLine("\"" + pol.ParentPolicy.Name.Replace("\"", "\"\"") + "\",\"" +
                            pol.ParentPolicy.LinkedTo.Count + "\",\"" +
                            pol.Type + "\",\"" +
                            pol.Root.Replace("\"", "\"\"") + "\",\"" +
                            pol.Path.Replace("\"", "\"\"") + "\",\"" +
                            pol.ResolvedPath.Replace("\"", "\"\"") + "\",\"" +
                            pol.Name.Replace("\"", "\"\"") + "\",\"" +
                            pol.Value.Replace("\"", "\"\"") + "\"");

                    }
                    
                }

                OutPut.Close();

            }     
           
        }
    
        private void OULoaded_Click(object sender, RoutedEventArgs e)
        {
            PolReader.OUSelector.ADPicker adpicker = new PolReader.OUSelector.ADPicker();
            adpicker.Owner = this;
            adpicker.ShowDialog();

            if (adpicker.ReturnValue == PolReader.OUSelector.ADPicker.RETURN_OK)
            {
                OUFilter.Text = adpicker.SelectedOU;                
            }

            FilterData();
        }

        private void OUFilter_SelectionChanged(object sender, RoutedEventArgs e)
        {
            FilterData();
        }

        private void MachineSearch_Click(object sender, RoutedEventArgs e)
        {
            PolReader.OUSelector.MachineSearch adpicker = new PolReader.OUSelector.MachineSearch();
            adpicker.Owner = this;
            adpicker.ShowDialog();

            if (adpicker.ReturnValue == PolReader.OUSelector.MachineSearch.RETURN_OK)
            {
                OUFilter.Text = adpicker.SelectedOU;
            }

            FilterData();
        }

        private void UserSearch_Click(object sender, RoutedEventArgs e)
        {
            PolReader.OUSelector.DomainUserSearch adpicker = new PolReader.OUSelector.DomainUserSearch();
            adpicker.Owner = this;
            adpicker.ShowDialog();

            if (adpicker.ReturnValue == PolReader.OUSelector.DomainUserSearch.RETURN_OK)
            {
                OUFilter.Text = adpicker.SelectedOU;
            }

            FilterData();
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }

        private void Image_ImageFailed_1(object sender, ExceptionRoutedEventArgs e)
        {

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        } 
    }
}
