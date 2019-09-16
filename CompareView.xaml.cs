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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using PolReader.DiffingClasses;

namespace PolReader
{
    /// <summary>
    /// Interaction logic for CompareView.xaml
    /// </summary>
    public partial class CompareView : Window
    {
        public CompareView()
        {
            InitializeComponent();
            
            CommandBinding cb = new CommandBinding(ApplicationCommands.Copy, CopyCmdExecuted, CopyCmdCanExecute);
            
            CompareTreeView.CommandBindings.Add(cb);  

        }

        void CopyCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            TreeView tv = e.OriginalSource as TreeView;

            string copyContent = String.Empty;

            if (tv.SelectedItem is DiffPolicyInfo)
            {   
                DiffPolicyInfo dpInf= (DiffPolicyInfo)tv.SelectedItem;
                copyContent = dpInf.Name;
            }
            else if (tv.SelectedItem is BaseDiffViewItem)
            {
                BaseDiffViewItem dpI = (BaseDiffViewItem)tv.SelectedItem;
                copyContent = dpI.Name;
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
            TreeView tv = e.OriginalSource as TreeView;
            // CanExecute only if there is one or more selected Item.   

            if (tv.SelectedItem !=null)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }   


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog fbDialog = new System.Windows.Forms.SaveFileDialog();
            fbDialog.Filter = "Compare Policy Files|*.Cpf";

            if (fbDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Stream stream = File.Open(fbDialog.FileName, FileMode.Create);
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, CompareTreeView.ItemsSource);
                stream.Close();

            }         
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fbDialog = new System.Windows.Forms.OpenFileDialog();
            fbDialog.Filter = "Compare Policy Files|*.Cpf";

            if (fbDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Stream stream = File.Open(fbDialog.FileName, FileMode.Open);
                BinaryFormatter bformatter = new BinaryFormatter();
                CompareTreeView.ItemsSource = (List<DiffPolicyInfo>)bformatter.Deserialize(stream);
                stream.Close();             
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
