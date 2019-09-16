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
    /// Interaction logic for UserSearch.xaml
    /// </summary>
    public partial class DomainUserSearch : Window
    {
        public const int RETURN_OK = 1;
        public const int RETURN_CANCEL = 0;

        public int ReturnValue = RETURN_CANCEL;

        public string SelectedOU = "";

        public DomainUserSearch()
        {
            InitializeComponent();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserNameText.Text.Length > 0)
            {
                DomainUser [] DomainUsers = ADQuery.FindUser(UserNameText.Text);
                UserItems.ItemsSource = DomainUsers;

            }
        }

        private void UserItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserItems.SelectedItem is DomainUser)
            {

                DomainUser SelectedItem = (DomainUser)UserItems.SelectedItem;
                SelectedOU = SelectedItem.Path;

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
