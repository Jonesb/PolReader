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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace PolReader
{
    /// <summary>
    /// Interaction logic for SelectFolder.xaml
    /// </summary>
    public partial class SelectFolder : Window
    {
        public SelectFolder()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbDialog = new System.Windows.Forms.FolderBrowserDialog();

            if (fbDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderText.Text = fbDialog.SelectedPath;
            }

        }


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.Directory.Exists(FolderText.Text))
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Folder doesn't exists");
            }
        }
    }
}
