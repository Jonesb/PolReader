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

namespace PolReader.BackupSet
{
    /// <summary>
    /// Interaction logic for BackupSetSelector.xaml
    /// </summary>
    public partial class BackupSetSelector : Window
    {
        public const int RETURN_OK = 1;
        public const int RETURN_CANCEL = 0;

        public int ReturnValue = RETURN_CANCEL;

        public int SelectedBackupSetID = -1;


        public BackupSetSelector()
        {            
            InitializeComponent();

            BackupSetTree.ItemsSource = DB.GetAllBackedUpDomains();
        }
       
        private void BackupSetTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (BackupSetTree.SelectedItem is BackupSet)
            {
                OKButton.IsEnabled = true;
            }
            else
            {
                OKButton.IsEnabled = false;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (BackupSetTree.SelectedItem is BackupSet)
            {

                BackupSet SelectedItem = (BackupSet)BackupSetTree.SelectedItem;
                SelectedBackupSetID = SelectedItem.BackupSetId;

            }

            ReturnValue = RETURN_OK;
            this.Close();
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
    }
}
