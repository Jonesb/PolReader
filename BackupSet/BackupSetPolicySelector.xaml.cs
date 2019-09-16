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

namespace PolReader.BackupSet
{
    /// <summary>
    /// Interaction logic for BackupSetItemSelector.xaml
    /// </summary>
    public partial class BackupSetPolicySelector : Window
    {
        public const int RETURN_OK = 1;
        public const int RETURN_CANCEL = 0;

        public int ReturnValue = RETURN_CANCEL;

        public int SelectedBackupSetPolicyID = -1;

        public BackupSetPolicySelector(int backupSetID)
        {            
            InitializeComponent();

            BackupSetTree.ItemsSource = DB.GetAllBackupSetPolicies(backupSetID);
        }
       

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (BackupSetTree.SelectedItem is BackupSetPolicy)
            {

                BackupSetPolicy SelectedItem = (BackupSetPolicy)BackupSetTree.SelectedItem;
                SelectedBackupSetPolicyID = SelectedItem.PolicyBackupID;

            }

            ReturnValue = RETURN_OK;
            this.Close();
        }

        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterData();
        }

        public void FilterData()
        {
            if (BackupSetTree != null)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(BackupSetTree.ItemsSource);
                view.Filter = null;

                view.Filter = new Predicate<object>(FilterListView);
            }
        }
        public bool FilterListView(Object item)
        {
            BackupSetPolicy policy = (BackupSetPolicy)item;

            return (FilterText.Text.Length < 1 || (policy.PolicyName.ToUpper().Contains(FilterText.Text.ToUpper())));

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void BackupSetTree_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BackupSetTree.SelectedItem is BackupSetPolicy)
            {
                OKButton.IsEnabled = true;
            }
            else
            {
                OKButton.IsEnabled = false;
            }
        } 
    }
}
