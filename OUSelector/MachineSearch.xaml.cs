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

namespace PolReader.OUSelector
{
    /// <summary>
    /// Interaction logic for MachineSearch.xaml
    /// </summary>
    public partial class MachineSearch : Window
    {
        public const int RETURN_OK = 1;
        public const int RETURN_CANCEL = 0;

        public int ReturnValue = RETURN_CANCEL;

        public string SelectedOU = "";

        public MachineSearch()
        {
            InitializeComponent();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (MachineNameText.Text.Length > 0)
            {
                DomainMachine[] DomainMachines = ADQuery.FindMachine(MachineNameText.Text);
                MachineItems.ItemsSource = DomainMachines;

            }
        }

        private void UserItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (MachineItems.SelectedItem is DomainMachine)
            {

                DomainMachine SelectedItem = (DomainMachine)MachineItems.SelectedItem;
                SelectedOU = SelectedItem.Path;

            }

            ReturnValue = RETURN_OK;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MachineItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
