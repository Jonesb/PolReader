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
using System.IO;

namespace ADMX
{
    /// <summary>
    /// Interaction logic for ADMXSelect.xaml
    /// </summary>
    public partial class ADMXSelect : Window
    {
        public const int INPUT_OK = 1;
        public const int INPUT_CANCEL = 2;

        public string ADMX_Location = "";
        public string ADMX_Language = "";

        public int SelectionInput = INPUT_CANCEL;

        public ADMXSelect()
        {
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void ADMXLocation_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbDialog = new System.Windows.Forms.FolderBrowserDialog();
            
            if (fbDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.ADMXLocation.Text = fbDialog.SelectedPath;
                UpdateLanguages();
            }
        }

        private void UpdateLanguages()
        {
            LanguageSelection.Items.Clear();

            if (Directory.Exists(this.ADMXLocation.Text))
            {
                foreach (String LanguageFolder in Directory.GetDirectories(this.ADMXLocation.Text, "*-*"))
                {
                    ComboBoxItem cbi = new ComboBoxItem();

                    if (this.ADMXLocation.Text.EndsWith("\\"))
                    {
                        cbi.Content = LanguageFolder.Replace(this.ADMXLocation.Text , "");
                    }
                    else
                    {
                        cbi.Content = LanguageFolder.Replace(this.ADMXLocation.Text + "\\", "");
                    }
                    LanguageSelection.Items.Add(cbi);
                }

            }

            if (LanguageSelection.Items.Count > 0)
            {
                LanguageSelection.SelectedIndex = 0;
            }

            LanguageSelection.IsEnabled = LanguageSelection.Items.Count > 0;            
            OKButton.IsEnabled = LanguageSelection.Items.Count > 0;
            
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            ADMX_Location = ADMXLocation.Text;
            ComboBoxItem cbi = (ComboBoxItem)LanguageSelection.SelectedValue;
            ADMX_Language = (string)cbi.Content;
            SelectionInput = INPUT_OK;
            this.Close();
        }

        private void CancellButoon_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

       
    }
}
