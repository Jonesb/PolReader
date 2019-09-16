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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using PolReader.DiffingClasses;
//using PolReader.BackupSet;
using ExcelXML;
//using Excel = Microsoft.Office.Interop.Excel; 

namespace PolReader
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        ListSortDirection _lastDirection = ListSortDirection.Ascending;
        GridViewColumnHeader _lastHeaderClicked = null;

        PolicySet polSet;
        Thread thread;

        string ADMXLoc;
        string ADMXLang;

        public List<Policy> SelectReportPolicies = new List<Policy>();
        
        string ReportFolderName = "";

        public delegate void ADMXLoadFinishedDelegate();
        public ADMXLoadFinishedDelegate ADMXLoadFinishedMethodInstance;

        public delegate void QueryFinishedDelegate();
        public QueryFinishedDelegate QueryFinishedMethodInstance;

        public delegate void SaveFinishedDelegate();
        public SaveFinishedDelegate SaveFinishedMethodInstance;

        public delegate void OpenFinishedDelegate();
        public OpenFinishedDelegate OpenFinishedMethodInstance;

        public delegate void CompareFinishedDelegate(List<DiffPolicyInfo> PolInfo);
        public CompareFinishedDelegate CompareFinishedMethodInstance;

        public delegate void UpdateProgViewDelegate(int Total,int Count,string Message);
        public UpdateProgViewDelegate UpdateProgViewMethodInstance;
        
        private ProgressView progView =new ProgressView();

        private AutoRunInfo arInfo;
        ADMX.GPOTemplates gptemp = null;

        public Window1()
        {
            InitializeComponent();

            CommandBinding cb = new CommandBinding(ApplicationCommands.Copy, CopyCmdExecuted, CopyCmdCanExecute);
            PoliciesListView.CommandBindings.Add(cb);


           // Menu_File_Save_DB.IsEnabled = DB.HasDBSaveRights();

            ADMXLoadFinishedMethodInstance = new ADMXLoadFinishedDelegate(AdmxLoadFinish);
            QueryFinishedMethodInstance = new QueryFinishedDelegate(QueryFinished);
            CompareFinishedMethodInstance = new CompareFinishedDelegate(CompareFinished);
            SaveFinishedMethodInstance = new SaveFinishedDelegate(SaveFinished);
            OpenFinishedMethodInstance = new OpenFinishedDelegate(OpenFinished);
            UpdateProgViewMethodInstance = new UpdateProgViewDelegate(UpdateProgView);

            arInfo = new AutoRunInfo(Environment.GetCommandLineArgs());

            if (arInfo.AutoRun() || arInfo.LogToDB)
            {               
                polSet = new PolicySet();
                polSet.GPOM.LogUpdateMessage += new GPOManagement.LogUpdate(GPOM_LogUpdateMessage);
                polSet.GetAllPolicies();
                
                if (arInfo.LogToAutoPath())
                {
                    Stream stream = File.Open(arInfo.GetSaveFilename(), FileMode.Create);
                    BinaryFormatter bformatter = new BinaryFormatter();
                    bformatter.Serialize(stream, polSet);
                    stream.Close();
                }
                
                if (arInfo.LogToAutoPath() && arInfo.CompareToYesterday())
                {
                    Stream YesterdaysStream = File.Open(arInfo.GetYesterdaysFile(), FileMode.Open);
                    BinaryFormatter bformatter2 = new BinaryFormatter();
                    PolicySet YesterdaysPolicySet = (PolicySet)bformatter2.Deserialize(YesterdaysStream);
                    YesterdaysStream.Close();

                    List<DiffPolicyInfo> PolInfo = polSet.Compare(YesterdaysPolicySet,false);

                    Stream compareStream = File.Open(arInfo.GetSaveCompareFilename(), FileMode.Create);
                    BinaryFormatter bformatter3 = new BinaryFormatter();
                    bformatter3.Serialize(compareStream, PolInfo);
                    compareStream.Close();
                }

                //if (arInfo.LogToDB)
                //{
                //    polSet.CreateDBBackupSet();

                //    int GetPreviousBackupID = DB.GetPreviousBackupID(polSet.BackupSetID);

                //    if (GetPreviousBackupID > 0)
                //    {

                //        List<DiffPolicyInfo> PolInfo = null;
                //        PolicySet LastDBBackup = new PolicySet();

                //        LastDBBackup.OpenDBBackupSet(GetPreviousBackupID);
                //        PolInfo = polSet.Compare(LastDBBackup,arInfo.CheckPreviousRelease);

                //        foreach (DiffPolicyInfo polDiff in PolInfo)
                //        {
                //            polDiff.LogDeletions(GetPreviousBackupID);
                //        }

                //    }
                    
                //}
                
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

        public void FilterData()
        {
            if (PoliciesListView != null && PoliciesListView.ItemsSource != null )
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(PoliciesListView.ItemsSource);
                view.Filter = null;

                view.Filter = new Predicate<object>(FilterListView);
            }
        }
        public bool FilterListView(Object item)
        {
            Policy policy = (Policy)item;

            return (FilterText.Text.Length < 1 || (policy.Name.ToUpper().Contains(FilterText.Text.ToUpper())) || (IncludeSecurityGroups.IsChecked == true && policy.SecurityGroupCompare(FilterText.Text)));

        }

        void GPOM_LogUpdateMessage(int Total, int Current,string Detail)
        {
            progView.Update(Total, Current);
            progView.UpdateDetail(Detail);
        }

        void Save_LogUpdateMessage(int Total, int Current, string Detail)
        {
            progView.Update(Total, Current);
            progView.UpdateDetail(Detail);
        }

        void Open_LogUpdateMessage(int Total, int Current, string Detail)
        {
            progView.Update(Total, Current);
            progView.UpdateDetail(Detail);
        }

        public void UpdateProgView(int Total, int Current, string Detail)
        {
            progView.Update(Total, Current);
            progView.UpdateDetail(Detail);
        }

        public void CompareFinished(List<DiffPolicyInfo> PolInfo)
        {            
            StopProgressDialog();

            CompareViewEx cv = new CompareViewEx();            

            cv.CompareTreeView.ItemsSource = PolInfo;
         

            cv.Owner = this;
            cv.ShowDialog();
        }
        
        public void QueryFinished()
        {
                                    
                PoliciesListView.ItemsSource = polSet.Policies;
                StopProgressDialog();
                EnableButtons();
                        
        }

        public void SaveFinished()
        {

            StopProgressDialog();
            EnableButtons();

        }

        public void OpenFinished()
        {
            PoliciesListView.ItemsSource = polSet.Policies;            
            StopProgressDialog();
            EnableButtons();

        }

        public void EnableButtons()
        {
            CompareButton.IsEnabled = true;
            //CompareDBButton.IsEnabled = true;
            //SearchButton.IsEnabled = true;
            //AssignmentsItemButton.IsEnabled = true;
            //LinksItemButton.IsEnabled = true;
            //WMIItemButton.IsEnabled = true;
            //SecEditItemButton.IsEnabled = true;

            Menu_Search.IsEnabled = true;
            //Menu_Report.IsEnabled = true;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e) 
        {
                        
            thread = new Thread(new ThreadStart(this.RunQueryFunction));
            thread.Start();

            progView.Reset();

            StartProgressDialog();
            
        }

        private void StartProgressDialog()
        {
            FormContainer.IsEnabled = false;
            //progView.Left = this.Left + (this.Width / 2.0) - (progView.Width / 2.0);
            //progView.Top = this.Top + (this.Height / 2.0) - (progView.Height / 2.0);
            
            progView.Owner = this;
            progView.ShowDialog();
        }

        private void StopProgressDialog()
        {
            FormContainer.IsEnabled = true;

            progView.Hide();
            
        }

        private void RunQueryFunction()
        {
            polSet = new PolicySet();            
            polSet.GPOM.LogUpdateMessage += new GPOManagement.LogUpdate(GPOM_LogUpdateMessage);
            polSet.GetAllPolicies();

            this.Dispatcher.Invoke(QueryFinishedMethodInstance, null);
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

        private void Menu_File_Open_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fbDialog = new System.Windows.Forms.OpenFileDialog();
            fbDialog.Filter = "Extracted Policy Files|*.Epl";

            if (fbDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Stream stream = File.Open(fbDialog.FileName, FileMode.Open);
                BinaryFormatter bformatter = new BinaryFormatter();
                polSet = (PolicySet)bformatter.Deserialize(stream);
                stream.Close();
                
                PoliciesListView.ItemsSource = polSet.Policies;
                EnableButtons();
            }
           
        }

        private void Menu_File_Save_As_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog fbDialog = new System.Windows.Forms.SaveFileDialog();
            fbDialog.Filter = "Extracted Policy Files|*.Epl";

            if (fbDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Stream stream = File.Open(fbDialog.FileName, FileMode.Create);
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, polSet);
                stream.Close();
                
            }            
        }

        private void Menu_File_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CompareButton_Click(object sender, RoutedEventArgs e)
        {
            if (polSet != null)
            {
                System.Windows.Forms.OpenFileDialog fbDialog = new System.Windows.Forms.OpenFileDialog();
                fbDialog.Filter = "Extracted Policy Files|*.Epl";

                if (fbDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    progView.Reset();
                    progView.UpdateDetail("Comparing....");
                    
                    thread = new Thread(new ParameterizedThreadStart(this.RunCompareFunction));
                    thread.Start(new string[]{fbDialog.FileName});

                    StartProgressDialog();
                    
                }
            }
        }

        //private void RunDBCompareFunction(object PolicyBackupID)
        //{
        //    List<DiffPolicyInfo> PolInfo = null;
        //    PolicySet PassedPolicy = new PolicySet();

        //    PassedPolicy.LogUpdateMessage += new PolicySet.LogUpdate(Open_LogUpdateMessage);
        //    PassedPolicy.OpenDBBackupSet(Int32.Parse(PolicyBackupID.ToString()));

        //    PolInfo = polSet.Compare(PassedPolicy,false);

        //    this.Dispatcher.Invoke(CompareFinishedMethodInstance, new object[] { PolInfo });

        //}

        private void RunCompareFunction(object FileArray)
        {
            string[] Files = (string[])FileArray;
            List<DiffPolicyInfo> PolInfo = null;

            PolicySet PassedPolicy;

            Stream stream = File.Open((string)Files[0], FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();
            PassedPolicy = (PolicySet)bformatter.Deserialize(stream);
            stream.Close();

            if (Files.Length > 1)
            {
                PolicySet PassedPolicyTwo;

                Stream streamTwo = File.Open((string)Files[1], FileMode.Open);
                BinaryFormatter bformatterTwo = new BinaryFormatter();
                PassedPolicyTwo = (PolicySet)bformatterTwo.Deserialize(streamTwo);
                streamTwo.Close();

                PolInfo = PassedPolicy.Compare(PassedPolicyTwo,false);
            }
            else
            {
                PolInfo = polSet.Compare(PassedPolicy,false);
            }
         

            this.Dispatcher.Invoke(CompareFinishedMethodInstance, new object[] { PolInfo });
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            progView.Close();
        }

        private void Menu_File_Open_Compare_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Forms.OpenFileDialog fbDialog = new System.Windows.Forms.OpenFileDialog();
            fbDialog.Filter = "Compare Policy Files|*.Cpf";

            if (fbDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CompareViewEx cv = new CompareViewEx();
                                
                Stream stream = File.Open(fbDialog.FileName, FileMode.Open);
                BinaryFormatter bformatter = new BinaryFormatter();

                List<DiffPolicyInfo> PolInfo = (List<DiffPolicyInfo>)bformatter.Deserialize(stream);

                cv.CompareTreeView.ItemsSource = PolInfo;
                stream.Close();

                cv.Owner = this;
                cv.ShowDialog();
            }
            
        }
        
        private void Menu_Tools_Query_Domain_Click(object sender, RoutedEventArgs e)
        {
            thread = new Thread(new ThreadStart(this.RunQueryFunction));
            thread.Start();

            progView.Reset();

            StartProgressDialog();
        }

        private void Menu_Tools_Compare_Files_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fbDialog = new System.Windows.Forms.OpenFileDialog();
            fbDialog.Filter = "Extracted Policy Files|*.Epl";
            fbDialog.Title = "Newest Extract File";

            if (fbDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Windows.Forms.OpenFileDialog fbDialogTwo = new System.Windows.Forms.OpenFileDialog();
                fbDialogTwo.Filter = "Extracted Policy Files|*.Epl";
                fbDialogTwo.Title = "Oldest Extract File";

                if (fbDialogTwo.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    thread = new Thread(new ParameterizedThreadStart(this.RunCompareFunction));
                    thread.Start(new string[] { fbDialog.FileName, fbDialogTwo.FileName });
                    progView.Reset();
                    StartProgressDialog();
                }
            }
        }

        private void PoliciesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DependencyObject src = (DependencyObject)(e.OriginalSource);   

            while (!(src is Control))     
                src = VisualTreeHelper.GetParent(src);

            if (src.GetType().Equals(typeof(ListViewItem)))
            {
                PolicyView PolView = new PolicyView((Policy)PoliciesListView.SelectedItem);                

                PolView.Owner = this;
                PolView.ShowDialog();
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (arInfo.AutoRun())
            {
                this.Close();
            }
        }

        private void CompareItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (PoliciesListView.SelectedItems.Count == 1)
            {
                PolicySelect PolSelect = new PolicySelect();
                PolicySet CopyPolset = new PolicySet();

                foreach (Policy item in polSet.Policies)
                {
                    CopyPolset.Policies.Add(item);
                }

                PolSelect.PoliciesListView.ItemsSource = CopyPolset.Policies;
                PolSelect.Owner = this;
                PolSelect.ShowDialog();

                if (PolSelect.SelectionInput == PolicySelect.INPUT_OK)
                {
                    Policy SourcePolicy = (Policy)PoliciesListView.SelectedItem;
                    Policy ComparePolicy = (Policy)PolSelect.PoliciesListView.SelectedItem;

                    CompareViewEx cv = new CompareViewEx();

                    List<DiffPolicyInfo> Items1 = new List<DiffPolicyInfo>();

                    Items1.Add(SourcePolicy.Compare(ComparePolicy));

                    cv.CompareTreeView.ItemsSource = Items1;

                    cv.Owner = this;
                    cv.ShowDialog();
                    
                }
            }
        }


        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterData();
        }

        private void IncludeSecurityGroups_Checked(object sender, RoutedEventArgs e)
        {
            FilterData();
        }

        private void IncludeSecurityGroups_Unchecked(object sender, RoutedEventArgs e)
        {
            FilterData();
        }
        
        //private void Menu_File_Save_DB_Click(object sender, RoutedEventArgs e)
        //{
        //    if (polSet != null && polSet.CanSaveToDB)            
        //    {

        //        thread = new Thread(new ThreadStart(this.SaveToDB));
        //        thread.Start();

        //        progView.Reset();

        //        StartProgressDialog();

        //    }

        //    else
        //    {
        //        MessageBox.Show("You can only save freshly queried data to the server","Save to DB Error");
        //    }

        //}

        /*private void SaveToDB()
        {
            polSet.LogUpdateMessage += new PolicySet.LogUpdate(Save_LogUpdateMessage);        
            polSet.CreateDBBackupSet();

            PolicySet ThisDBBackup = new PolicySet();
            ThisDBBackup.LogUpdateMessage += new PolicySet.LogUpdate(Open_LogUpdateMessage);
            ThisDBBackup.OpenDBBackupSet(polSet.BackupSetID);

            int GetPreviousBackupID = DB.GetPreviousBackupID(polSet.BackupSetID);

            if (true)
            {
                
                List<DiffPolicyInfo> PolInfo = null;
                PolicySet LastDBBackup = new PolicySet();

                if (GetPreviousBackupID > 0)
                {
                    LastDBBackup.LogUpdateMessage += new PolicySet.LogUpdate(Open_LogUpdateMessage);
                    LastDBBackup.OpenDBBackupSet(GetPreviousBackupID);
                }

                PolInfo = ThisDBBackup.Compare(LastDBBackup, true);

                int count = 0;

                PolInfo.Sort(delegate(DiffPolicyInfo p1, DiffPolicyInfo p2) { return p1.Name.CompareTo(p2.Name); });

                foreach (DiffPolicyInfo polDiff in PolInfo)
                {
                    count++;

                    progView.Update(PolInfo.Count, count);
                    progView.UpdateDetail(polDiff.Name);

                    if (polDiff.UpdateType == DiffPolicyInfo.PREVIOUS_REVISION_POLICY)
                    {
                        polDiff.LogDeletions(polSet.BackupSetID);

                        DB.ClonePolicyComments(DB.GetPolicyID(polSet.BackupSetID,polDiff.LastRevisionPolicyGuid),
                                                DB.GetPolicyID(polSet.BackupSetID,polDiff.PolicyGuid));

                    }
                    else
                    {
                        polDiff.LogDeletions(GetPreviousBackupID);
                    }
                                        
                }

            }

            this.Dispatcher.Invoke(SaveFinishedMethodInstance, null);
        }*/

        private void Menu_File_Open_DB_Click(object sender, RoutedEventArgs e)
        {
            /*try
            {

                BackupSetSelector setSelect = new BackupSetSelector();

                setSelect.Owner = this;
                setSelect.ShowDialog();

                if (setSelect.ReturnValue == BackupSetSelector.RETURN_OK)
                {
                    if (polSet == null)
                        polSet = new PolicySet();

                    thread = new Thread(this.OpenFromDB);
                    thread.Start(setSelect.SelectedBackupSetID);

                    progView.Reset();

                    StartProgressDialog();
                }
            }
            catch 
            {
                MessageBox.Show("Error Opening from DB ");
            }*/
                        
        }

        //private void OpenFromDB(object data)
        //{
        //    polSet.LogUpdateMessage += new PolicySet.LogUpdate(Open_LogUpdateMessage);
        //    polSet.OpenDBBackupSet(Int32.Parse(data.ToString()));


        //    this.Dispatcher.Invoke(OpenFinishedMethodInstance, null);
        //}

        //private void CompareDBButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (polSet != null)
        //    {
        //        BackupSetSelector setSelect = new BackupSetSelector();

        //        setSelect.Owner = this;
        //        setSelect.ShowDialog();

        //        if (setSelect.ReturnValue == BackupSetSelector.RETURN_OK)
        //        {
        //            if (polSet == null)
        //                polSet = new PolicySet();

        //            thread = new Thread(this.RunDBCompareFunction);
        //            thread.Start(setSelect.SelectedBackupSetID);

        //            StartProgressDialog();
        //        }

                
        //    }
        //}

          

        private void Menu_Search_PolicyItems_Click(object sender, RoutedEventArgs e)
        {
            List<SearchablePolicyItem> SpItems = new List<SearchablePolicyItem>();
                       

            foreach (Policy pol in polSet.Policies)
            {
                foreach (PolicyFile pf in pol.PolicyFiles)
                {
                    foreach (PolicyItem pi in pf.PolicyItems)
                    {
                        SearchablePolicyItem Spi = new SearchablePolicyItem();

                        Spi.ParentPolicy = pol;
                        Spi.Context = pf.Type == PolicyFile.POLICY_FILE_TYPE_MACHINE ? "Machine" : "Users";
                        Spi.Linkage = -1;
                        
                        Spi.Value = pi.Value;                                                
                        Spi.Key = pi.Key;
                        Spi.Data = pi.Data;
                        Spi.Type = pi.StringType;
                        
                        SpItems.Add(Spi);


                    }
                }

            }

            //PolSearcher.PolicyItems.ItemsSource = SpItems;

            PolicySearcher PolSearcher = new PolicySearcher(SpItems);

            PolSearcher.Owner = this;
            PolSearcher.ShowDialog();
        }

        private void Menu_Search_Secedit_Click(object sender, RoutedEventArgs e)
        {

            List<SearchableSecEditItem> SpItems = new List<SearchableSecEditItem>();

            foreach (Policy pol in polSet.Policies)
            {

                foreach (SecEditSection ses in pol.SecEditFileData.Sections)
                {

                    foreach (SecEditValuePair svp in ses.Entries)
                    {
                        SearchableSecEditItem ssei = new SearchableSecEditItem();

                        ssei.ParentPolicy = pol;
                        ssei.Linkage = -1;
                        ssei.Name = svp.Name;
                        ssei.ParentSection = ses;
                        ssei.Value = svp.Value;

                        SpItems.Add(ssei);

                    }

                }


            }

            SecEditSearcher PolSearcher = new SecEditSearcher(SpItems);

            PolSearcher.Owner = this;
            PolSearcher.ShowDialog();

        }

        private void Menu_Search_Assignments_Click(object sender, RoutedEventArgs e)
        {
            List<AssignmentItem> SpItems = new List<AssignmentItem>();

            foreach (Policy pol in polSet.Policies)
            {
                SpItems.AddRange(pol.AppliedTo);

            }

            AssignmentSearcher PolSearcher = new AssignmentSearcher(SpItems);
            PolSearcher.Owner = this;
            PolSearcher.ShowDialog();
        }

        private void Menu_Search_Links_Click(object sender, RoutedEventArgs e)
        {
            List<LinkageItem> SpItems = new List<LinkageItem>();

            foreach (Policy pol in polSet.Policies)
            {
                SpItems.AddRange(pol.LinkedTo);
            }

            LinkageSearcher PolSearcher = new LinkageSearcher(SpItems);
            PolSearcher.Owner = this;
            PolSearcher.ShowDialog();
        }

        private void Menu_Search_WMI_Click(object sender, RoutedEventArgs e)
        {
            List<WMIItem> SpItems = new List<WMIItem>();

            foreach (Policy pol in polSet.Policies)
            {
                SpItems.AddRange(pol.WMIFilters);
            }

            WMISearcher PolSearcher = new WMISearcher(SpItems);
            PolSearcher.Owner = this;
            PolSearcher.ShowDialog();
        }

        private void Menu_Search_IEAK_Click(object sender, RoutedEventArgs e)
        {
            List<SearchableIEAKItem> SpItems = new List<SearchableIEAKItem>();

            foreach (Policy pol in polSet.Policies)
            {

                foreach (SecEditSection ses in pol.IEAKFileData.Sections)
                {

                    foreach (SecEditValuePair svp in ses.Entries)
                    {
                        SearchableIEAKItem ssei = new SearchableIEAKItem();

                        ssei.ParentPolicy = pol;
                        ssei.Linkage = -1;
                        ssei.Name = svp.Name;
                        ssei.ParentSection = ses;
                        ssei.Value = svp.Value;
                        ssei.MachineSetting = false;

                        SpItems.Add(ssei);

                    }

                }

                foreach (SecEditSection ses in pol.IEAKMachineFileData.Sections)
                {

                    foreach (SecEditValuePair svp in ses.Entries)
                    {
                        SearchableIEAKItem ssei = new SearchableIEAKItem();

                        ssei.ParentPolicy = pol;
                        ssei.Linkage = -1;
                        ssei.Name = svp.Name;
                        ssei.ParentSection = ses;
                        ssei.Value = svp.Value;
                        ssei.MachineSetting = true;

                        SpItems.Add(ssei);

                    }

                }


            }

            IEAKSearcher PolSearcher = new IEAKSearcher(SpItems);

            PolSearcher.Owner = this;
            PolSearcher.ShowDialog();

        }

        private List<SearchableXMLItem> GetRecursiveXMLSearchItems(Policy ParentPol,
                                                                    String Type,
                                                                    String Root,
                                                                    String Path,
                                                                    String ResolvedPath,
                                                                    PolReaderXMLNode PolNode)
        {
            List<SearchableXMLItem> SpItems = new List<SearchableXMLItem>();

            string Name = "";
            bool CLSIDfound = false;

            foreach (PolReaderXMLProperty prop in PolNode.Properties)
            {
                if (prop.Name.Equals("name", StringComparison.CurrentCultureIgnoreCase))
                {
                    Name = "\\" + prop.Value;
                }
                else if (prop.Name.Equals("clsid", StringComparison.CurrentCultureIgnoreCase))
                {
                    CLSIDfound = true;
                    
                }
            }

            if (!CLSIDfound)
            {
                Name = "";
            }

            foreach (PolReaderXMLProperty prop in PolNode.Properties)
            {

                if (!(prop.Name.Equals("name", StringComparison.CurrentCultureIgnoreCase) ||
                    (prop.Name.Equals("clsid", StringComparison.CurrentCultureIgnoreCase))))
                {
                   
                    SearchableXMLItem xmlNode = new SearchableXMLItem();

                    xmlNode.ParentPolicy = ParentPol;
                    xmlNode.Linkage = -1;
                    xmlNode.Name = prop.Name;
                    xmlNode.Value = prop.Value;
                    xmlNode.Root = Root;
                    xmlNode.ResolvedPath = ResolvedPath + Name;
                    xmlNode.Path = Path;
                    xmlNode.Type = Type;

                    SpItems.Add(xmlNode);
                }

            }

            foreach (PolReaderXMLNode node in PolNode.Children)
            {
                SpItems.AddRange(GetRecursiveXMLSearchItems(ParentPol, Type, Root, Path + "\\" + node.Name, ResolvedPath + Name, node));
            }

            return SpItems;
        }

        private void Menu_Search_Preferences_Click(object sender, RoutedEventArgs e)
        {
            List<SearchableXMLItem> SpItems = new List<SearchableXMLItem>();

            foreach (Policy pol in polSet.Policies)
            {

                foreach (PolReaderXMLNode Pref in pol.PreferencesFiles)
                {
                    SpItems.AddRange(GetRecursiveXMLSearchItems(pol, "Users", Pref.Name, "", Pref.Name, Pref));
                }

                foreach (PolReaderXMLNode Pref in pol.MachinePreferencesFiles)
                {
                    SpItems.AddRange(GetRecursiveXMLSearchItems(pol, "Machine", Pref.Name, "", Pref.Name, Pref));
                } 

            }

            PreferenceSearcher PolSearcher = new PreferenceSearcher(SpItems);

            PolSearcher.Owner = this;
            PolSearcher.ShowDialog();
        }

        //private void Menu_Report_PolicyItem_Click(object sender, RoutedEventArgs e)
        //{
            
        //    if(polSet.BackupSetID > 0)
        //    {

        //        PolicySelect PolSelect = new PolicySelect(true);
        //        PolicySet CopyPolset = new PolicySet();

        //        foreach (Policy item in polSet.Policies)
        //        {
        //            CopyPolset.Policies.Add(item);
        //        }

        //        PolSelect.PoliciesListView.ItemsSource = CopyPolset.Policies;
        //        PolSelect.Owner = this;
        //        PolSelect.ShowDialog();

        //        if (PolSelect.SelectionInput == PolicySelect.INPUT_OK)
        //        {
        //            ADMXLoc = "";
        //            ADMXLang = "";

        //            SelectReportPolicies = PolSelect.SelectPolicies;

        //            System.Windows.Forms.FolderBrowserDialog fbDialog = new System.Windows.Forms.FolderBrowserDialog();
        //            //fbDialog.FileName = ReportPolicy.Name + ".xml";
        //            //fbDialog.Filter = "Excel XML Files|*.xml";

        //            if (fbDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //            {
        //                ReportFolderName = fbDialog.SelectedPath;

        //                thread = new Thread(new ThreadStart(this.StartADMXLOad));
        //                thread.Start();

        //                progView.Reset();
        //                StartProgressDialog();
        //            }                       

        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("You can only report on Items opened or saved to the DB","Policy Report");
        //    }
                                   
        //}

        //private void Menu_Report_LoadADMX_Click(object sender, RoutedEventArgs e)
        //{

        //    if (polSet.BackupSetID > 0)
        //    {
                
        //        ADMX.ADMXSelect admxSelector = null;

        //        if (gptemp == null)
        //        {
        //            admxSelector = new ADMX.ADMXSelect();
        //            admxSelector.Owner = this;
        //            admxSelector.ShowDialog();
        //        }

        //        if (gptemp != null || admxSelector.SelectionInput == ADMX.ADMXSelect.INPUT_OK)
        //        {
                                       
        //            PolicySelect PolSelect = new PolicySelect(true);
        //            PolicySet CopyPolset = new PolicySet();

        //            foreach (Policy item in polSet.Policies)
        //            {
        //                CopyPolset.Policies.Add(item);
        //            }

        //            PolSelect.PoliciesListView.ItemsSource = CopyPolset.Policies;
        //            PolSelect.Owner = this;
        //            PolSelect.ShowDialog();

        //            if (PolSelect.SelectionInput == PolicySelect.INPUT_OK)
        //            {                        
        //                System.Windows.Forms.FolderBrowserDialog fbDialog = new System.Windows.Forms.FolderBrowserDialog();
                    
        //                if (fbDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //                {
        //                    ReportFolderName = fbDialog.SelectedPath;

        //                    if (gptemp == null)
        //                    {
        //                        ADMXLoc = admxSelector.ADMX_Location;
        //                        ADMXLang = admxSelector.ADMX_Language;
        //                    }

        //                    SelectReportPolicies = PolSelect.SelectPolicies;

        //                    thread = new Thread(new ThreadStart(this.StartADMXLOad));
        //                    thread.Start();

        //                    progView.Reset();
        //                    StartProgressDialog();

        //                }                               
        //            }
                   
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("You can only report on Items opened or saved to the DB", "Policy Report");
        //    }
        //}

        /*void StartADMXLOad()
        {

            if (gptemp == null && ADMXLoc.Length > 0 && ADMXLang.Length > 0)
            {
                gptemp = new ADMX.GPOTemplates();
                gptemp.LogUpdateMessage += new ADMX.GPOTemplates.LogUpdate(ADMX_LogUpdateMessage);
                gptemp.LoadTemplates(ADMXLoc, ADMXLang);
            }
          
            try
            {

                foreach (Policy reportPol in SelectReportPolicies)
                {
                    Point matrix;
                    ExcelXML.WorkBook xlWorkBook = new WorkBook();

                    ExcelXML.WorkSheet xlWorkSheet = new WorkSheet("Registry Policies");

                    this.Dispatcher.Invoke(UpdateProgViewMethodInstance, new object[] { 4, 1, "Gathering Registry Settings" });

                    matrix = DB.ReportRegSettings("ReportRegPolicySettings", DB.GetPolicyBackupID(polSet.BackupSetID, reportPol.Name,
                                                                            reportPol.GUID), xlWorkSheet, gptemp);

                    if ((matrix.X) > 0 && (matrix.Y > 0))
                    {
                        xlWorkBook.WorkSheets.Add(xlWorkSheet);
                    }

                    this.Dispatcher.Invoke(UpdateProgViewMethodInstance, new object[] { 4, 2, "Gathering Secedit Settings" });

                    xlWorkSheet = new WorkSheet("GPTTmpl Policies");
                    matrix = DB.ReportSettings("ReportSecEditPolicySettings", DB.GetPolicyBackupID(polSet.BackupSetID, reportPol.Name,
                                                                            reportPol.GUID), xlWorkSheet);

                    if ((matrix.X) > 0 && (matrix.Y > 0))
                    {
                        xlWorkBook.WorkSheets.Add(xlWorkSheet);
                    }

                    this.Dispatcher.Invoke(UpdateProgViewMethodInstance, new object[] { 4, 3, "Gathering IEAK Settings" });

                    xlWorkSheet = new WorkSheet("IEAK Settings");
                    matrix = DB.ReportSettings("ReportIEAKPolicySettings", DB.GetPolicyBackupID(polSet.BackupSetID, reportPol.Name,
                                                                            reportPol.GUID), xlWorkSheet);
                    if ((matrix.X) > 0 && (matrix.Y > 0))
                    {
                        xlWorkBook.WorkSheets.Add(xlWorkSheet);
                    }

                    this.Dispatcher.Invoke(UpdateProgViewMethodInstance, new object[] { 4, 4, "Gathering Preferences" });

                    PreferenceReportItem[] pritems = DB.ReportPreferencePolicySettings(DB.GetPolicyBackupID(polSet.BackupSetID, reportPol.Name,
                                                                            reportPol.GUID));

                    PreferenceReport pr = new PreferenceReport();

                    foreach (PreferenceReportItem pri in pritems)
                    {

                        if (pri.ClsidProperty.Length > 0)
                        {
                            pr.AddRootNode(pri);
                        }

                        pr.AddToParent(pri);

                    }

                    xlWorkSheet = new WorkSheet("Preferences");
                    matrix = pr.OutPutToWorkSheet(xlWorkSheet);

                    if ((matrix.X) > 0 && (matrix.Y > 0))
                    {
                        xlWorkBook.WorkSheets.Add(xlWorkSheet);

                    }

                    xlWorkBook.LogUpdateMessage += new WorkBook.LogUpdate(ADMX_LogUpdateMessage);
                    xlWorkBook.Save(this.ReportFolderName + "\\" + reportPol.Name + ".XML");
                }


            }
            catch (Exception err)
            {
                MessageBox.Show("Error creating excel file \n" + err.Message, "Creating Report");
            }
            this.Dispatcher.Invoke(ADMXLoadFinishedMethodInstance, null);
        }*/

        public void AdmxLoadFinish()
        {
            StopProgressDialog();
            EnableButtons();
        }

        void ADMX_LogUpdateMessage(int Total, int Current, string Detail)
        {
            progView.Update(Total, Current);
            progView.UpdateDetail(Detail);
        }

        private void CompareDBItemButton_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (PoliciesListView.SelectedItems.Count == 1)
            {
                BackupSetSelector setSelect = new BackupSetSelector();

                setSelect.Owner = this;
                setSelect.ShowDialog();

                if (setSelect.ReturnValue == BackupSetSelector.RETURN_OK)
                {

                    BackupSetPolicySelector setSelectPol = new BackupSetPolicySelector(setSelect.SelectedBackupSetID);

                    setSelectPol.Owner = this;
                    setSelectPol.ShowDialog();

                    if (setSelectPol.ReturnValue == BackupSetPolicySelector.RETURN_OK)
                    {
                        
                        Policy SourcePolicy = (Policy)PoliciesListView.SelectedItem;
                        Policy ComparePolicy = new Policy();


                        System.Data.DataSet rsDS = DB.GetAllRegSettings(setSelect.SelectedBackupSetID,setSelectPol.SelectedBackupSetPolicyID);
                        System.Data.DataSet sedDS = DB.GetAllSecEditSectionData(setSelect.SelectedBackupSetID,setSelectPol.SelectedBackupSetPolicyID);
                        System.Data.DataSet ieakDS = DB.GetAllIEAKData(setSelect.SelectedBackupSetID,false,setSelectPol.SelectedBackupSetPolicyID);
                        System.Data.DataSet ieakMachineDS = DB.GetAllIEAKData(setSelect.SelectedBackupSetID, true,setSelectPol.SelectedBackupSetPolicyID);

                        System.Data.DataSet wmiDS = DB.GetAllWMIFilters(setSelect.SelectedBackupSetID, setSelectPol.SelectedBackupSetPolicyID);
                        System.Data.DataSet linkDS = DB.GetAllLinks(setSelect.SelectedBackupSetID, setSelectPol.SelectedBackupSetPolicyID);
                        System.Data.DataSet assDS = DB.GetAllAssignments(setSelect.SelectedBackupSetID, setSelectPol.SelectedBackupSetPolicyID);

                        System.Data.DataSet prefDS = DB.GetAllPreferenceNodes(setSelect.SelectedBackupSetID, setSelectPol.SelectedBackupSetPolicyID);
                        System.Data.DataSet propDS = DB.GetAllPreferenceProperties(setSelect.SelectedBackupSetID, setSelectPol.SelectedBackupSetPolicyID);
           
                        sedDS.Tables[0].DefaultView.RowFilter = "policyBackupID = " + setSelectPol.SelectedBackupSetPolicyID;
                        ieakDS.Tables[0].DefaultView.RowFilter = "policyBackupID = " + setSelectPol.SelectedBackupSetPolicyID;
                        wmiDS.Tables[0].DefaultView.RowFilter = "policyBackupID = " + setSelectPol.SelectedBackupSetPolicyID;
                        linkDS.Tables[0].DefaultView.RowFilter = "policyBackupID = " + setSelectPol.SelectedBackupSetPolicyID;
                        assDS.Tables[0].DefaultView.RowFilter = "policyBackupID = " + setSelectPol.SelectedBackupSetPolicyID;
               
                        rsDS.Tables[0].DefaultView.RowFilter = "policyBackupID = " + setSelectPol.SelectedBackupSetPolicyID + " AND  PolType = 'User'" ;
                        ComparePolicy.PolicyFiles.Add(DB.GetPoliciesPolicyFile(rsDS));
                        ComparePolicy.PolicyFiles[ComparePolicy.PolicyFiles.Count - 1].Type = PolicyFile.POLICY_FILE_TYPE_USER;

                        rsDS.Tables[0].DefaultView.RowFilter = "policyBackupID = " + setSelectPol.SelectedBackupSetPolicyID + " AND  PolType = 'Machine'";                    
                        ComparePolicy.PolicyFiles.Add(DB.GetPoliciesPolicyFile(rsDS));
                        ComparePolicy.PolicyFiles[ComparePolicy.PolicyFiles.Count - 1].Type = PolicyFile.POLICY_FILE_TYPE_MACHINE;

                        ComparePolicy.AppliedTo = DB.GetPoliciesAssignments(assDS, ComparePolicy);
                        ComparePolicy.LinkedTo = DB.GetPoliciesLinks(linkDS, ComparePolicy);
                        ComparePolicy.WMIFilters = DB.GetPoliciesWMIFilters(wmiDS, ComparePolicy);

                        ComparePolicy.SecEditFileData = DB.GetPoliciesSecEditSectionData(sedDS);
                        ComparePolicy.IEAKFileData = DB.GetPoliciesIEAKData(ieakDS);
                        ComparePolicy.IEAKMachineFileData = DB.GetPoliciesIEAKData(ieakMachineDS);
               
                        ComparePolicy.PreferencesFiles = DB.GetPolicyPreferences(prefDS, propDS,0,"policyBackupID = " + setSelectPol.SelectedBackupSetPolicyID + " AND Type = 0");               
                        ComparePolicy.MachinePreferencesFiles = DB.GetPolicyPreferences(prefDS, propDS, 0, "policyBackupID = " + setSelectPol.SelectedBackupSetPolicyID + " AND Type = 1");


                        CompareViewEx cv = new CompareViewEx();

                        List<DiffPolicyInfo> Items1 = new List<DiffPolicyInfo>();

                        Items1.Add(SourcePolicy.Compare(ComparePolicy));

                        cv.CompareTreeView.ItemsSource = Items1;


                        cv.Owner = this;
                        cv.ShowDialog();
                    }
                    
                }


            }
           */
        }

        private void Menu_File_Folder_Policy_Click(object sender, RoutedEventArgs e)
        {
            SelectFolder setfolder = new SelectFolder();

            setfolder.Owner = this;
            setfolder.ShowDialog();

            if (setfolder.DialogResult.HasValue && setfolder.DialogResult.Value)
            {

                polSet = new PolicySet();
                polSet.CanSaveToDB = false;

                polSet.Policies.Add(new Policy(setfolder.FolderText.Text,
                        "Opened For Folder", setfolder.FolderText.Text, new DateTime(), 1));
                
                PoliciesListView.ItemsSource = polSet.Policies;
                EnableButtons();
            }
        }

        private void CompareFolderButton_Click(object sender, RoutedEventArgs e)
        {
            SelectFolder  setfolder = new SelectFolder();

            setfolder.Owner = this;
            setfolder.ShowDialog();

            if (setfolder.DialogResult.HasValue && setfolder.DialogResult.Value)
            {
                Policy ComparePolicy = new Policy(setfolder.FolderText.Text ,
                        "Opened For Folder", setfolder.FolderText.Text, new DateTime(), 1);

                CompareViewEx cv = new CompareViewEx();

                //List<BaseDiffViewItem> Items = new List<BaseDiffViewItem>();

                Policy SourcePolicy = (Policy)PoliciesListView.SelectedItem;
                //Items.Add(SourcePolicy.Compare(ComparePolicy).GetViewItems(null));

                //cv.CompareTreeView.ItemsSource = Items;

                 List<DiffPolicyInfo> Items1 = new List<DiffPolicyInfo>();

                 Items1.Add(SourcePolicy.Compare(ComparePolicy));
                 cv.DataContext = Items1;
                cv.Owner = this;
                cv.ShowDialog();
            }

            
        }
             
        
    }
}
