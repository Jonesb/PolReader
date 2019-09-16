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
using Microsoft.Win32;
using System.ComponentModel;
using System.Threading;

namespace PolReader
{
    /// <summary>
    /// Interaction logic for PolicyView.xaml
    /// </summary>
    public partial class PolicyView : Window
    {
        ListSortDirection _lastDirection = ListSortDirection.Ascending;
        GridViewColumnHeader _lastHeaderClicked = null;

        private ProgressView progView = new ProgressView();

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

                    if (headerClicked.Column.Header.ToString() == "")
                    {
                        Sort("Exists", direction, (ListView)e.Source);
                    }
                    else
                    {
                        Binding binding = (Binding)(headerClicked.Column.DisplayMemberBinding);
                        Sort(binding.Path.Path, direction, (ListView)e.Source);
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

        public delegate void QueryFinishedDelegate();
        public QueryFinishedDelegate QueryFinishedMethodInstance;
        public delegate void UpdateProgViewDelegate(int Total, int Count, string Message);
        public UpdateProgViewDelegate UpdateProgViewMethodInstance;

        private void Sort(string sortBy, ListSortDirection direction,ListView vl)
        {            
            ICollectionView dataView =
              CollectionViewSource.GetDefaultView(vl.ItemsSource);

            if (dataView != null)
            {
                dataView.SortDescriptions.Clear();
                SortDescription sd = new SortDescription(sortBy, direction);
                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();
            }
        }
        private Policy thisPolicy;

        int MachineCountItem = 0;
        int UserCountItem = 0;

        public PolicyView(Policy policy)
        {

            InitializeComponent();

            CommandBinding cb = new CommandBinding(ApplicationCommands.Copy, CopyCmdExecuted, CopyCmdCanExecute);

            MachinePolicyItems.CommandBindings.Add(cb);
            UserPolicyItems.CommandBindings.Add(cb);
            LinkedView.CommandBindings.Add(cb);
            AssignedView.CommandBindings.Add(cb);
            WMIView.CommandBindings.Add(cb);
            PrefPropertyList.CommandBindings.Add(cb);

            QueryFinishedMethodInstance = new QueryFinishedDelegate(QueryFinished);
            UpdateProgViewMethodInstance = new UpdateProgViewDelegate(UpdateProgView);

            this.Title = "Policy - " + policy.Name;
            thisPolicy = policy;                                    

        }

        public void UpdateProgView(int Total, int Current, string Detail)
        {
            progView.Update(Total, Current);
            progView.UpdateDetail(Detail);
        }

        private string ConvertRegKindToString(RegistryValueKind rvk)
        {
            string returnValue = "";
            switch (rvk)
            {
                case RegistryValueKind.Binary:
                    returnValue = "REG_BINARY";
                    break;
                case RegistryValueKind.DWord:
                    returnValue = "REG_DWORD";
                    break;
                case RegistryValueKind.ExpandString:
                    returnValue = "REG_EXPAND_SZ";
                    break;
                case RegistryValueKind.MultiString:
                    returnValue = "REG_MULTI_SZ";
                    break;
                case RegistryValueKind.None:
                    returnValue = "REG_NONE";
                    break;
                case RegistryValueKind.QWord:
                    returnValue = "REG_QWORD";
                    break;
                case RegistryValueKind.String:
                    returnValue = "REG_SZ";
                    break;
            }
            return returnValue;
        }

        void CopyCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            ListView lv = e.OriginalSource as ListView;

            string copyContent = String.Empty;

            int count =0;
            foreach (object item in lv.SelectedItems)
            {
                if (count > 0)
                    copyContent += Environment.NewLine;

                if (item is String)
                {                    
                    copyContent += (String)item ;
                }
                else if (item is PolicyItem)
                {
                    
                    PolicyItem policyItem = (PolicyItem)item;
                    copyContent += policyItem.LocalCompare.ActualPolicy.ExistsText() + "\t" +
                                    policyItem.LocalCompare.ActualPolicy.LocalValue + "\t" +
                                    policyItem.LocalCompare.DiffContextPolicy.ExistsText() + "\t" +
                                    policyItem.LocalCompare.DiffContextPolicy.LocalValue + "\t" +
                                    policyItem.LocalCompare.NonPolicy.ExistsText() + "\t" +
                                    policyItem.LocalCompare.NonPolicy.LocalValue + "\t" +
                                    policyItem.LocalCompare.DiffContextNonPolicy.ExistsText() + "\t" +
                                    policyItem.LocalCompare.DiffContextNonPolicy.LocalValue + "\t" +
                                    policyItem.Key + "\t" + policyItem.Value + "\t" + policyItem.StringType + "\t" + policyItem.Data;
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }        

        private void CheckExists()
        {
            
            foreach (PolicyFile pf in thisPolicy.PolicyFiles)
            {
                RegistryKey Root = Registry.LocalMachine;
                RegistryKey DiffContextRoot = Registry.CurrentUser;

                if (pf.Type == PolicyFile.POLICY_FILE_TYPE_USER)
                {
                    Root = Registry.CurrentUser;
                    DiffContextRoot = Registry.LocalMachine;
                }

                int c = 0;
                foreach (PolicyItem item in pf.PolicyItems)
                {
                    c++;

                    this.Dispatcher.Invoke(UpdateProgViewMethodInstance, new object[] { pf.PolicyItems.Count, c, item.Key + "\\" + item.Value });

                    item.LocalCompare.ActualPolicy = TestItem(Root, item.Key, item.Value, item);
                    item.LocalCompare.DiffContextPolicy = TestItem(DiffContextRoot, item.Key, item.Value, item);

                    if (item.Key.ToUpper().StartsWith("SOFTWARE\\POLICIES"))
                    {
                        item.LocalCompare.NonPolicy = TestItem(Root, item.Key.ToUpper().Replace("SOFTWARE\\POLICIES\\", "SOFTWARE\\"), item.Value, item);
                        item.LocalCompare.DiffContextNonPolicy = TestItem(DiffContextRoot, item.Key.ToUpper().Replace("SOFTWARE\\POLICIES\\", "SOFTWARE\\"), item.Value, item);
                    }                    
                   
                }

            }
            
            this.Dispatcher.Invoke(QueryFinishedMethodInstance, null);

        }

        private PolicyItemCheckItem TestItem(RegistryKey passedRoot, string passedKey, string passedValue, PolicyItem item)
        {

            PolicyItemCheckItem returnValue = new PolicyItemCheckItem();
            try
            {
                RegistryKey rk = passedRoot.OpenSubKey(passedKey);

                object Value = rk.GetValue(passedValue);

                if (Value != null)
                {
                    RegistryValueKind rvk = rk.GetValueKind(item.Value);

                    if (ConvertRegKindToString(rvk).ToString().Equals(item.StringType))
                    {
                        if (rvk.Equals(RegistryValueKind.Binary))
                        {
                            byte[] bytes = (byte[])Value;
                            string vs = "";

                            for (int i = 0; i < bytes.Length; i++)
                            {
                                vs += String.Format("{0:X2}", bytes[i]);
                            }

                            returnValue.LocalValue = vs;

                            if (vs.Equals(item.Data))
                            {
                                returnValue.Exists = PolicyItem.ITEM_EXISTS_SAME;
                                returnValue.ExistsInfo = "Values Match";
                                
                            }
                            else
                            {
                                returnValue.Exists = PolicyItem.ITEM_EXISTS_VALUE_DIFFERS;
                                returnValue.ExistsInfo = "Local value " + vs;
                            }
                        }
                        else if (rvk.Equals(RegistryValueKind.ExpandString))
                        {
                            returnValue.LocalValue = Value.ToString();

                            if (Value.ToString().Equals(item.Data))
                            {
                                returnValue.Exists = PolicyItem.ITEM_EXISTS_SAME;
                                returnValue.ExistsInfo = "Values Match";
                            }
                            else
                            {
                                if (Environment.ExpandEnvironmentVariables(item.Data).Equals(Value.ToString()))
                                {
                                    returnValue.Exists = PolicyItem.ITEM_EXISTS_SAME;
                                    returnValue.ExistsInfo = "Values Match";
                                }
                                else
                                {
                                    returnValue.Exists = PolicyItem.ITEM_EXISTS_VALUE_DIFFERS;
                                    returnValue.ExistsInfo = "Local value " + Value;
                                }
                            }
                        }
                        else if (rvk.Equals(RegistryValueKind.MultiString))
                        {
                            string[] regItems = item.Data.Split('\0').Where(t => t.Length > 0).ToArray();

                            if (Value is string[] && regItems.SequenceEqual((string[])(Value)))
                            {

                                returnValue.LocalValue = Value.ToString();
                                returnValue.Exists = PolicyItem.ITEM_EXISTS_SAME;
                                returnValue.ExistsInfo = "Values Match";

                            }
                            else
                            {
                                returnValue.Exists = PolicyItem.ITEM_EXISTS_VALUE_DIFFERS;
                                returnValue.ExistsInfo = "Local value " + Value;
                                returnValue.LocalValue = Value.ToString();
                            }
                        }
                        else if (Value.ToString().Equals(item.Data))
                        {
                            returnValue.LocalValue = Value.ToString();
                            returnValue.Exists = PolicyItem.ITEM_EXISTS_SAME;
                            returnValue.ExistsInfo = "Values Match";

                        }
                        else
                        {
                            returnValue.Exists = PolicyItem.ITEM_EXISTS_VALUE_DIFFERS;
                            returnValue.ExistsInfo = "Local value " + Value;
                            returnValue.LocalValue = Value.ToString();
                        }

                    }
                    else
                    {
                        returnValue.Exists = PolicyItem.ITEM_EXISTS_TYPE_DIFFERS;
                        returnValue.ExistsInfo = "Local type " + ConvertRegKindToString(rvk).ToString();
                        returnValue.LocalValue = item.Data.ToString();
                    }


                }
                else if (item.Value.StartsWith("**del.") || item.StringType.Equals("REG_NONE"))
                {

                    returnValue.Exists = PolicyItem.ITEM_EXISTS_SAME;
                    returnValue.ExistsInfo = "Deleted locally too";
                    returnValue.LocalValue = "";

                }
                else
                {
                    returnValue.Exists = PolicyItem.ITEM_MISSING;
                    returnValue.ExistsInfo = "Not set locally";
                    returnValue.LocalValue = "";
                }
            }
            catch
            {
                returnValue.Exists = PolicyItem.ITEM_MISSING;
                returnValue.ExistsInfo = "Not set locally";
                returnValue.LocalValue = "";
            }

            return returnValue;
        }

        private void StartProgressDialog()
        {            
            //FormContainer.IsEnabled = false;
            //progView.Left = this.Left + (this.Width / 2.0) - (progView.Width / 2.0);
            //progView.Top = this.Top + (this.Height / 2.0) - (progView.Height / 2.0);

            this.IsEnabled = false;
            progView.Owner = this;
            progView.ShowDialog();
            
        }

        private void QueryFinished()
        {
            foreach (PolicyFile pf in thisPolicy.PolicyFiles)
            {

                if (pf.Type == PolicyFile.POLICY_FILE_TYPE_USER)
                {
                    UserPolicyItems.ItemsSource = pf.PolicyItems;
                    UserCountItem += pf.PolicyItems.Count;                    
                }
                else if (pf.Type == PolicyFile.POLICY_FILE_TYPE_MACHINE)
                {
                    MachinePolicyItems.ItemsSource = pf.PolicyItems;
                    MachineCountItem += pf.PolicyItems.Count;

                }
            }
            MachineRegItemsTab.Header = "Machine Registry Items (" + MachineCountItem + ")";
            MachineRegItemsTab.Visibility = MachineCountItem > 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            UserRegItemsTab.Header = "User Registry Items (" + UserCountItem + ")";
            UserRegItemsTab.Visibility = UserCountItem > 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            MachinePrefItemsTab.Header = "Machine Preferences (" + (thisPolicy.MachinePreferencesFiles.Count  ) + ")";
            MachinePrefItemsTab.Visibility = thisPolicy.MachinePreferencesFiles.Count > 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            UserPrefItemsTab.Header = "User Preferences (" + (thisPolicy.PreferencesFiles.Count) + ")";
            UserPrefItemsTab.Visibility = thisPolicy.PreferencesFiles.Count > 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            
            LinkedView.ItemsSource = thisPolicy.LinkedTo;
            AssignedView.ItemsSource = thisPolicy.AppliedTo;
            WMIView.ItemsSource = thisPolicy.WMIFilters;
            PrefTree.ItemsSource = thisPolicy.MachinePreferencesFiles;
            UserPrefTree.ItemsSource = thisPolicy.PreferencesFiles;

            GptTmplTree.ItemsSource = thisPolicy.SecEditFileData.Sections;
            GptTmplItemsTab.Header = "GptTmpl";
            GptTmplItemsTab.Visibility = thisPolicy.SecEditFileData.Sections.Count > 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            IEAKMachineTree.ItemsSource = thisPolicy.IEAKMachineFileData.Sections;
            IEAKMachineItemsTab.Header = "IEAK Machine";
            IEAKMachineItemsTab.Visibility = thisPolicy.IEAKMachineFileData.Sections.Count > 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            IEAKUserTree.ItemsSource = thisPolicy.IEAKFileData.Sections;
            IEAKUserItemsTab.Header = "IEAK User";
            IEAKUserItemsTab.Visibility = thisPolicy.IEAKFileData.Sections.Count > 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            tabControl.SelectedIndex = MachineCountItem > 0 ? 0 : UserCountItem > 0 ? 1 : thisPolicy.MachinePreferencesFiles.Count > 0 ? 2 : thisPolicy.PreferencesFiles.Count>0?3:7;

            progView.Hide();
            this.IsEnabled = true;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(this.CheckExists));
            thread.Start();

            progView.Reset();

            StartProgressDialog();
        }

        
        private void Machine_CreateReg(object sender, RoutedEventArgs e)
        {

            Dictionary<string, List<PolicyItem>> registryKeyItems = new Dictionary<string, List<PolicyItem>>();

            try{
                
                
                foreach (object item in MachinePolicyItems.SelectedItems)
                {
                   if(item is PolicyItem)
                   {
                       
                       PolicyItem policyItem = (PolicyItem)item;

                       if (!registryKeyItems.ContainsKey(policyItem.Key))
                       {
                           registryKeyItems.Add(policyItem.Key, new List<PolicyItem>());
                       }

                       registryKeyItems[policyItem.Key].Add(policyItem);
                   
                    }
                
                }

                string tester = "Windows Registry Editor Version 5.00\r\n";

                foreach (string key in registryKeyItems.Keys)
                {

                    if (registryKeyItems[key].Count == 1 && registryKeyItems[key][0].IsKeyDeletion)
                    {
                        tester += "\r\n[-HKEY_LOCAL_MACHINE\\" + key + "]\r\n";
                    }
                    else
                    {
                        tester += "\r\n[HKEY_LOCAL_MACHINE\\" + key + "]\r\n";

                        foreach (PolicyItem values in registryKeyItems[key])
                        {
                            tester += values.RegFileString() + "\r\n";
                        }
                    }
                }

                Clipboard.SetData(DataFormats.Text, tester);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void User_CreateReg(object sender, RoutedEventArgs e)
        {

            Dictionary<string, List<PolicyItem>> registryKeyItems = new Dictionary<string, List<PolicyItem>>();

            try
            {


                foreach (object item in UserPolicyItems.SelectedItems)
                {
                    if (item is PolicyItem)
                    {

                        PolicyItem policyItem = (PolicyItem)item;

                        if (!registryKeyItems.ContainsKey(policyItem.Key))
                        {
                            registryKeyItems.Add(policyItem.Key, new List<PolicyItem>());
                        }

                        registryKeyItems[policyItem.Key].Add(policyItem);

                    }

                }

                string tester = "Windows Registry Editor Version 5.00\r\n";

                foreach (string key in registryKeyItems.Keys)
                {

                    if (registryKeyItems[key].Count == 1 && registryKeyItems[key][0].IsKeyDeletion)
                    {
                        tester += "\r\n[-HKEY_CURRENT_USER\\" + key + "]\r\n";
                    }
                    else
                    {
                        tester += "\r\n[HKEY_CURRENT_USER\\" + key + "]\r\n";

                        foreach (PolicyItem values in registryKeyItems[key])
                        {
                            tester += values.RegFileString() + "\r\n";
                        }
                    }
                }

                Clipboard.SetData(DataFormats.Text, tester);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Machine_Write(object sender, RoutedEventArgs e)
        {

            try{
                foreach (object item in MachinePolicyItems.SelectedItems)
                {
                   if(item is PolicyItem)
                   {
                       PolicyItem policyItem = (PolicyItem)item;

                       policyItem.Write(true);
                   
                    }
                
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void User_Write(object sender, RoutedEventArgs e)
        {

            try
            {
                foreach (object item in UserPolicyItems.SelectedItems)
                {
                    if (item is PolicyItem)
                    {
                        PolicyItem policyItem = (PolicyItem)item;

                        policyItem.Write(false);

                    }

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        
    }
}
