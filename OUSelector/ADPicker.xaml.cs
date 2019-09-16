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
    /// Interaction logic for ADPicker.xaml
    /// </summary>
    public partial class ADPicker : Window
    {
        public const int RETURN_OK =1;
        public const int RETURN_CANCEL = 0;

        public int ReturnValue = RETURN_CANCEL;

        public string SelectedOU="";

        public ADPicker()
        {
            InitializeComponent();

            Domain[] domains = ADQuery.GetDomains();
            AllDomainsViewModel viewModel = new AllDomainsViewModel(domains);
            base.DataContext = viewModel;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if(OUTreeView.SelectedItem is OUViewModel)
            {
                
                OUViewModel selectedOU = (OUViewModel)OUTreeView.SelectedItem;
                SelectedOU = selectedOU.ou.DirectoryEntry.Path;

                

                while(!(selectedOU.Parent is DomainViewModel))
                {
                    selectedOU = (OUViewModel)selectedOU.Parent;
                }

                DomainViewModel BaseDomain = (DomainViewModel)selectedOU.Parent;

               SelectedOU =  SelectedOU.Replace(BaseDomain.domain.DirectoryEntry.Path + "/", "");

                
            }
            else if (OUTreeView.SelectedItem is DomainViewModel)
            {
                DomainViewModel selectedDomain = (DomainViewModel)OUTreeView.SelectedItem;
                SelectedOU = "DC=" + selectedDomain.DomainName.Replace(".",",DC=");
            }
            
            ReturnValue = RETURN_OK;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OUTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            OKButton.IsEnabled = true;
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
