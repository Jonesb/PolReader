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

namespace PolReader
{
    /// <summary>
    /// Interaction logic for PolicySelect.xaml
    /// </summary>
    public partial class PolicySelect : Window
    {
        ListSortDirection _lastDirection = ListSortDirection.Ascending;
        GridViewColumnHeader _lastHeaderClicked = null;

        public const int INPUT_OK = 1;
        public const int INPUT_CANCEL = 2;

        public int SelectionInput = INPUT_CANCEL;
        public List<Policy> SelectPolicies = new List<Policy>();

        public PolicySelect()
        {
            InitializeComponent();
            CommandBinding cb = new CommandBinding(ApplicationCommands.Copy, CopyCmdExecuted, CopyCmdCanExecute);
            PoliciesListView.CommandBindings.Add(cb);
        }

        public PolicySelect(bool Multiples)
        {
            InitializeComponent();
            CommandBinding cb = new CommandBinding(ApplicationCommands.Copy, CopyCmdExecuted, CopyCmdCanExecute);
            PoliciesListView.CommandBindings.Add(cb);

            PoliciesListView.SelectionMode = Multiples ? SelectionMode.Multiple : SelectionMode.Single;
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
                        Sort("LinkedTo.Count", direction);
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
              CollectionViewSource.GetDefaultView(PoliciesListView.ItemsSource);
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

                if (item is Policy)
                {
                    Policy policy = (Policy)item;
                    copyContent += policy.LinkedTo.Count + "\t" +
                        policy.Name + "\t" +
                        policy.GUID + "\t" +
                        policy.Version + "\t" +
                        policy.Date;
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

        private void PoliciesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PoliciesListView.SelectedItems.Count > 0)
            {
                OKButton.IsEnabled = true;
            }
            else
            {
                OKButton.IsEnabled = false;
            }

        }

        public void FilterData()
        {
            if (PoliciesListView != null)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(PoliciesListView.ItemsSource);
                view.Filter = null;

                view.Filter = new Predicate<object>(FilterListView);
            }
        }
        public bool FilterListView(Object item)
        {
            Policy policy = (Policy)item;

            return (FilterText.Text.Length < 1 || (policy.Name.ToUpper().Contains(FilterText.Text.ToUpper())));

        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Policy item in PoliciesListView.SelectedItems)
            {
                SelectPolicies.Add(item);
            }

            SelectionInput = INPUT_OK;
            this.Close();
        }

        private void CancelButtton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterData();
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
