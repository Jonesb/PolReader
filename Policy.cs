using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using PolReader.DiffingClasses;

namespace PolReader
{
    [Serializable()]
    public class Policy : ISerializable
    {
        public string GUID { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public int Version { get; set; }
        public DateTime Date { get; set; }

        public List<PolicyFile> PolicyFiles { get; set; }
        public List<AssignmentItem> AppliedTo { get; set; }
        public List<LinkageItem> LinkedTo { get; set; }
        public List<WMIItem> WMIFilters { get; set; }

        public SecEditFile SecEditFileData { get; set; }
        public SecEditFile IEAKFileData { get; set; }
        public SecEditFile IEAKMachineFileData { get; set; }

        public List<PolReaderXMLNode> PreferencesFiles { get; set; }       
        public List<PolReaderXMLNode> MachinePreferencesFiles { get; set; }

        //public List<PolReaderXMLNode> MachineAvecto { get; set; }
        //public List<PolReaderXMLNode> Avecto { get; set; }



        public Policy(SerializationInfo info, StreamingContext ctxt)
        {
            this.GUID = (string)info.GetValue("GUID", typeof(string));
            this.Name = (string)info.GetValue("Name", typeof(string));
            this.Version = (int)info.GetValue("Version", typeof(int));
            this.Date = (DateTime)info.GetValue("Date", typeof(DateTime));
            
           this.PolicyFiles = (List<PolicyFile>)info.GetValue("PolicyFiles", typeof(List<PolicyFile>));            
           this.AppliedTo = (List<AssignmentItem>)info.GetValue("AppliedTo", typeof(List<AssignmentItem>));
           this.LinkedTo = (List<LinkageItem>)info.GetValue("LinkedTo", typeof(List<LinkageItem>));
           this.WMIFilters = (List<WMIItem>)info.GetValue("WMIFilters", typeof(List<WMIItem>));

           try
           {
               this.MachinePreferencesFiles = (List<PolReaderXMLNode>)info.GetValue("MachinePreferencesFiles", typeof(List<PolReaderXMLNode>));

           }
           catch (Exception e)
           {
               this.MachinePreferencesFiles = new List<PolReaderXMLNode>();
           }

           //try
           //{
           //    this.MachineAvecto = (List<PolReaderXMLNode>)info.GetValue("MachineAvecto", typeof(List<PolReaderXMLNode>));

           //}
           //catch (Exception e)
           //{
           //    this.MachineAvecto = new List<PolReaderXMLNode>();
           //}

           try
           {
               this.PreferencesFiles = (List<PolReaderXMLNode>)info.GetValue("PreferencesFiles", typeof(List<PolReaderXMLNode>));

           }
           catch (Exception e)
           {
               this.PreferencesFiles = new List<PolReaderXMLNode>();
           }

           //try
           //{
           //    this.Avecto = (List<PolReaderXMLNode>)info.GetValue("Avecto", typeof(List<PolReaderXMLNode>));

           //}
           //catch (Exception e)
           //{
           //    this.Avecto = new List<PolReaderXMLNode>();
           //}  

           try
           {
               this.SecEditFileData = (SecEditFile)info.GetValue("SecEditFileData", typeof(SecEditFile));
           }
           catch (Exception e)
           {
               this.SecEditFileData = new SecEditFile();
           }

           try
           {
               this.IEAKFileData = (SecEditFile)info.GetValue("IEAKFileData", typeof(SecEditFile));
           }
           catch (Exception e)
           {
               this.IEAKFileData = new SecEditFile();
           }    

           try
           {
               this.IEAKMachineFileData = (SecEditFile)info.GetValue("IEAKMachineFileData", typeof(SecEditFile));
           }
           catch (Exception e)
           {
               this.IEAKMachineFileData = new SecEditFile();
           }  

        }

        public Boolean IsLinked(string OU)
        {
            Boolean ReturnValue = false;

            foreach (LinkageItem Link in LinkedTo)
            {
                if(!Link.Linkage.EndsWith("(Site)"))
                {
                    bool Blocked = Link.Linkage.EndsWith("(True)");

                    String RawPath = Link.Linkage.Replace("(Domain) - Inheritence Blocked (True)", "");
                    RawPath = RawPath.Replace("(Domain) - Inheritence Blocked (False)", "");
                    RawPath = RawPath.Replace("(OU) - Inheritence Blocked (True)", "");
                    RawPath = RawPath.Replace("(OU) - Inheritence Blocked (False)", "");                                 

                    if ((Blocked && OU.Equals(RawPath))
                       || (!Blocked && OU.Contains(RawPath)))
                    {
                        ReturnValue = true;
                    }
                }
                                
            }

            return ReturnValue;
        }

        public String IconString
        {
            get
            {
                string ReturnType = "";

                bool ContainsAuthenticatedUsers=false;

                foreach(AssignmentItem assItem in AppliedTo)
                {
                    if(assItem.Assignment.Equals("NT AUTHORITY\\Authenticated Users"))
                        ContainsAuthenticatedUsers = true;
                }

                if (LinkedTo.Count > 0)
                {
                    if (WMIFilters.Count > 0)
                    {
                        if (!ContainsAuthenticatedUsers)
                        {
                            ReturnType = "SecFilteredLinkedPolicy.png";
                        }
                        else
                        {
                            ReturnType = "FilteredLinkedPolicy.png";
                        }
                    }
                    else
                    {
                        if (!ContainsAuthenticatedUsers)
                        {
                            ReturnType = "SecLinkedPolicy.png";
                        }
                        else
                        {
                            ReturnType = "LinkedPolicy.png";
                        }
                        
                    }
                }
                else
                {
                    if (WMIFilters.Count > 0)
                    {
                        if (!ContainsAuthenticatedUsers)
                        {
                            ReturnType = "SecFilteredUnLinkedPolicy.png";
                        }
                        else
                        {
                            ReturnType = "FilteredUnLinkedPolicy.png";
                        }
                        
                    }
                    else
                    {
                        if (!ContainsAuthenticatedUsers)
                        {
                            ReturnType = "SecUnLinkedPolicy.png";
                        }
                        else
                        {
                            ReturnType = "UnLinkedPolicy.png";
                        }
                        
                    }
                    
                }                       

                return ReturnType;
            }

        }

        public bool SecurityGroupCompare(string SecurityFunction)
        {
            bool returnValue = false;

            foreach (AssignmentItem item in AppliedTo)
	        {
                if (item.Assignment.Equals(SecurityFunction))
                {
                    returnValue = true;
                    break;
                }
	        }

            return returnValue;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("GUID", this.GUID);
            info.AddValue("Name", this.Name);
            info.AddValue("Version", this.Version);
            info.AddValue("Date", this.Date);

            info.AddValue("PolicyFiles", this.PolicyFiles);
            info.AddValue("AppliedTo", this.AppliedTo);
            info.AddValue("LinkedTo", this.LinkedTo);

            info.AddValue("WMIFilters", this.WMIFilters);

            info.AddValue("SecEditFileData", this.SecEditFileData);

            info.AddValue("IEAKFileData", this.IEAKFileData);
            info.AddValue("IEAKMachineFileData", this.IEAKMachineFileData);

            info.AddValue("PreferencesFiles", this.PreferencesFiles);            
            info.AddValue("MachinePreferencesFiles", this.MachinePreferencesFiles);
            //info.AddValue("MachineAvecto", this.MachineAvecto);
            //info.AddValue("Avecto", this.Avecto);

        }

        public Policy()
        {
            PolicyFiles = new List<PolicyFile>();
            AppliedTo = new List<AssignmentItem>();
            LinkedTo = new List<LinkageItem>();
            WMIFilters = new List<WMIItem>();

            SecEditFileData = new SecEditFile();

            IEAKFileData = new SecEditFile();
            IEAKMachineFileData = new SecEditFile();

            
            PreferencesFiles = new List<PolReaderXMLNode>();            
            MachinePreferencesFiles = new List<PolReaderXMLNode>();

            //Avecto = new List<PolReaderXMLNode>();
            //MachineAvecto =new List<PolReaderXMLNode>();

            GUID = "";
            Name = "";            
            Date = new DateTime();
        }

        private void AddPrefItem(List<PolReaderXMLNode> polArray, string Filename)
        {
            if (File.Exists(Filename))
            {               
                polArray.Add(new PolReaderXMLNode(Filename));               
            }
        }

        public void Run()
        {
            if (Location == null || Location.Length == 0 )
            {
                throw new Exception("Run can only be called when the Location is Set");
            }

            PolicyFiles = new List<PolicyFile>();
            AppliedTo = new List<AssignmentItem>();
            LinkedTo = new List<LinkageItem>();
            WMIFilters = new List<WMIItem>();

            SecEditFileData = new SecEditFile(Location + "\\Machine\\microsoft\\windows nt\\secedit\\GptTmpl.inf");

            IEAKFileData = new SecEditFile(Location + "\\User\\Microsoft\\IEAK\\install.ins");
            IEAKFileData.AddExtraFile(Location + "\\User\\Microsoft\\IEAK\\branding\\Zones\\seczones.inf");
            IEAKFileData.AddExtraFile(Location + "\\User\\Microsoft\\IEAK\\branding\\Zones\\seczrsop.inf");
            IEAKFileData.AddExtraFile(Location + "\\User\\Microsoft\\IEAK\\branding\\Ratings\\ratings.inf");
            IEAKFileData.AddExtraFile(Location + "\\User\\Microsoft\\IEAK\\branding\\Ratings\\ratrsop.inf");
            IEAKFileData.AddExtraFile(Location + "\\User\\Microsoft\\IEAK\\branding\\Authcode\\authcode.inf");
            IEAKFileData.AddExtraFile(Location + "\\User\\Microsoft\\IEAK\\branding\\Programs\\programs.inf");
            IEAKFileData.AddExtraFile(Location + "\\User\\Microsoft\\IEAK\\branding\\Adm\\inetcorp.inf");
            IEAKFileData.AddExtraFile(Location + "\\User\\Microsoft\\IEAK\\branding\\Adm\\inetset.inf");

            IEAKMachineFileData = new SecEditFile(Location + "\\Machine\\Microsoft\\IEAK\\install.ins");
            IEAKMachineFileData.AddExtraFile(Location + "\\User\\Microsoft\\IEAK\\branding\\Zones\\seczones.inf");
            IEAKMachineFileData.AddExtraFile(Location + "\\User\\Microsoft\\IEAK\\branding\\Zones\\seczrsop.inf");
            IEAKMachineFileData.AddExtraFile(Location + "\\User\\Microsoft\\IEAK\\branding\\Ratings\\ratings.inf");
            IEAKMachineFileData.AddExtraFile(Location + "\\User\\Microsoft\\IEAK\\branding\\Ratings\\ratrsop.inf");
            IEAKMachineFileData.AddExtraFile(Location + "\\User\\Microsoft\\IEAK\\branding\\Authcode\\authcode.inf");
            IEAKMachineFileData.AddExtraFile(Location + "\\User\\Microsoft\\IEAK\\branding\\Programs\\programs.inf");
            IEAKMachineFileData.AddExtraFile(Location + "\\User\\Microsoft\\IEAK\\branding\\Adm\\inetcorp.inf");
            IEAKMachineFileData.AddExtraFile(Location + "\\User\\Microsoft\\IEAK\\branding\\Adm\\inetset.inf");


            PreferencesFiles = new List<PolReaderXMLNode>();
            MachinePreferencesFiles = new List<PolReaderXMLNode>();

            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\Applications\\Applications.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\Data Sources\\DataSources.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\Devices\\Devices.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\Drives\\Drives.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\EnvironmentVariables\\EnvironmentVariables.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\Files\\Files.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\FolderOptions\\FolderOptions.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\Folders\\Folders.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\IniFiles\\IniFiles.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\InternetSettings\\InternetSettings.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\Groups\\Groups.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\NetworkOptions\\NetworkOptions.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\NetworkShares\\NetworkShares.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\PowerOptions\\PowerOptions.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\Printers\\Printers.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\RegionalOptions\\RegionalOptions.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\Registry\\Registry.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\ScheduledTasks\\ScheduledTasks.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\Services\\Services.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\Shortcuts\\Shortcuts.xml");
            AddPrefItem(MachinePreferencesFiles, Location + "\\Machine\\Preferences\\StartMenuTaskbar\\StartMenuTaskbar.xml");


            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\Applications\\Applications.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\Data Sources\\DataSources.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\Devices\\Devices.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\Drives\\Drives.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\EnvironmentVariables\\EnvironmentVariables.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\Files\\Files.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\FolderOptions\\FolderOptions.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\Folders\\Folders.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\IniFiles\\IniFiles.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\InternetSettings\\InternetSettings.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\Groups\\Groups.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\NetworkOptions\\NetworkOptions.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\NetworkShares\\NetworkShares.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\PowerOptions\\PowerOptions.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\Printers\\Printers.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\RegionalOptions\\RegionalOptions.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\Registry\\Registry.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\ScheduledTasks\\ScheduledTasks.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\Services\\Services.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\Shortcuts\\Shortcuts.xml");
            AddPrefItem(PreferencesFiles, Location + "\\User\\Preferences\\StartMenuTaskbar\\StartMenuTaskbar.xml");

            //MachineAvecto = new List<PolReaderXMLNode>();
            //Avecto = new List<PolReaderXMLNode>();

            //AddPrefItem(MachineAvecto, Location + "\\Machine\\Avecto\\Privilege Guard\\PrivilegeGuardConfig.xml");
            //AddPrefItem(Avecto, Location + "\\User\\Avecto\\Privilege Guard\\PrivilegeGuardConfig.xml");

            
            GetPolicySettings(Location);
        }

        public Policy(string polLocation,string polGUID,string polName,DateTime ModDate,int Ver,bool DeferRun = false)
        {
            Location = polLocation;
            GUID = polGUID;
            Name = polName;
            Version = Ver;
            Date = ModDate;

            if (!DeferRun)
            {
                Run();
            }


        }

        private void GetPolicySettings(string Location)
        {
            string MachinePolicy = Location + "\\Machine\\registry.pol";
            string UserPolicy = Location + "\\User\\registry.pol";
            

            if (File.Exists(MachinePolicy))
            {
                PolicyFiles.Add(new PolicyFile(MachinePolicy,PolicyFile.POLICY_FILE_TYPE_MACHINE));
            }
            if (File.Exists(UserPolicy))
            {
                PolicyFiles.Add(new PolicyFile(UserPolicy, PolicyFile.POLICY_FILE_TYPE_USER));
            }
           
        }

        public DiffPolicyInfo Compare(Policy OldPolicy)
        {
            DiffPolicyInfo returnValue = new DiffPolicyInfo();

            returnValue.PolicyGuid = GUID;
            returnValue.Name = Name;


            if (MachinePreferencesFiles != null && OldPolicy.MachinePreferencesFiles != null)
            {
                if (OldPolicy.MachinePreferencesFiles.Count > 0)
                {
                    returnValue.Name = Name;
                }

                foreach (PolReaderXMLNode PrefNode in MachinePreferencesFiles)
                {
                    bool found = false;

                    foreach (PolReaderXMLNode oldPrefNode in OldPolicy.MachinePreferencesFiles)
                    {                       
                        if(PrefNode.Name.Equals(oldPrefNode.Name))
                        {
                            XmlNodeDiffInfo DiffItems = PrefNode.Compare(oldPrefNode);

                            if (DiffItems.nodeItems.Count > 0 || DiffItems.propItems.Count > 0)
                            {
                                returnValue.MachinePreferenceDifferenceInfo.Name = "Machine Preferences";
                                returnValue.MachinePreferenceDifferenceInfo.Type = XmlNodeDiffInfo.UPDATED_POLICY;
                                DiffItems.Type = XmlNodeDiffInfo.UPDATED_POLICY;
                                returnValue.MachinePreferenceDifferenceInfo.nodeItems.Add(DiffItems);
                            }

                            found = true;
                            break;
                        }
                        
                    }

                    if (!found)
                    {
                        returnValue.MachinePreferenceDifferenceInfo.Name = "Machine Preferences";
                        returnValue.MachinePreferenceDifferenceInfo.Type = XmlNodeDiffInfo.NEW_POLICY;
                        
                        XmlNodeDiffInfo DiffItem = PrefNode.Compare(new PolReaderXMLNode());
                        DiffItem.Type = XmlNodeDiffInfo.NEW_POLICY;
                        returnValue.MachinePreferenceDifferenceInfo.nodeItems.Add(DiffItem);

                    }
                }

                foreach (PolReaderXMLNode oldPrefNode in OldPolicy.MachinePreferencesFiles)
                {
                    bool found = false;

                    foreach (PolReaderXMLNode PrefNode in MachinePreferencesFiles)
                    {
                        if (PrefNode.Name.Equals(oldPrefNode.Name))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found )
                    {
                        returnValue.MachinePreferenceDifferenceInfo.Name = "Machine Preferences";
                        returnValue.MachinePreferenceDifferenceInfo.Type = XmlNodeDiffInfo.UPDATED_POLICY;
                        XmlNodeDiffInfo DiffItem = new PolReaderXMLNode().Compare(oldPrefNode);

                        DiffItem.DBID = oldPrefNode.DBID;
                        DiffItem.Type = XmlNodeDiffInfo.DELETED_POLICY;
                        DiffItem.Name = oldPrefNode.Name;
                        returnValue.MachinePreferenceDifferenceInfo.nodeItems.Add(DiffItem);

                    }
                }
            }

            if (PreferencesFiles != null && OldPolicy.PreferencesFiles != null)
            {
                if (OldPolicy.MachinePreferencesFiles.Count > 0)
                {
                    returnValue.Name = Name;
                }

                foreach (PolReaderXMLNode PrefNode in PreferencesFiles)
                {
                    bool found = false;

                    foreach (PolReaderXMLNode oldPrefNode in OldPolicy.PreferencesFiles)
                    {
                        if (PrefNode.Name.Equals(oldPrefNode.Name))
                        {
                            XmlNodeDiffInfo DiffItems = PrefNode.Compare(oldPrefNode);

                            if (DiffItems.nodeItems.Count > 0 || DiffItems.propItems.Count > 0)
                            {
                                returnValue.PreferenceDifferenceInfo.Name = "User Preferences";
                                returnValue.PreferenceDifferenceInfo.Type = XmlNodeDiffInfo.UPDATED_POLICY;
                                DiffItems.Type = XmlNodeDiffInfo.UPDATED_POLICY;
                                returnValue.PreferenceDifferenceInfo.nodeItems.Add(DiffItems);
                            }

                            found = true;
                            break;
                        }

                    }

                    if (!found)
                    {
                        returnValue.PreferenceDifferenceInfo.Name = "User Preferences";
                        returnValue.PreferenceDifferenceInfo.Type = XmlNodeDiffInfo.NEW_POLICY;

                        XmlNodeDiffInfo DiffItem = PrefNode.Compare(new PolReaderXMLNode());
                        DiffItem.Type = XmlNodeDiffInfo.NEW_POLICY;
                        returnValue.PreferenceDifferenceInfo.nodeItems.Add(DiffItem);

                    }
                }

                foreach (PolReaderXMLNode oldPrefNode in OldPolicy.PreferencesFiles)
                {
                    bool found = false;

                    foreach (PolReaderXMLNode PrefNode in PreferencesFiles)
                    {
                        if (PrefNode.Name.Equals(oldPrefNode.Name))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        returnValue.PreferenceDifferenceInfo.Name = "User Preferences";
                        returnValue.PreferenceDifferenceInfo.Type = XmlNodeDiffInfo.UPDATED_POLICY;
                        XmlNodeDiffInfo DiffItem = new PolReaderXMLNode().Compare(oldPrefNode);
                        DiffItem.Type = XmlNodeDiffInfo.DELETED_POLICY;
                        DiffItem.DBID = oldPrefNode.DBID;
                        DiffItem.Name = oldPrefNode.Name;
                        returnValue.PreferenceDifferenceInfo.nodeItems.Add(DiffItem);

                    }
                }
            }

            if (SecEditFileData != null && SecEditFileData.Sections!=null &&
                OldPolicy.SecEditFileData != null && OldPolicy.SecEditFileData.Sections != null)
            {
                
                foreach (SecEditSection Section in SecEditFileData.Sections)
                {
                    bool found = false;

                    foreach (SecEditSection oldSection in OldPolicy.SecEditFileData.Sections)
                    {
                        if (oldSection.Name.Equals(Section.Name))
                        {
                            List<SecEditDiffValueInfo> Items = Section.Compare(oldSection);

                            if (Items.Count > 0)
                            {
                                returnValue.SecEditDifferenceInfo.Type = SecEditDiffInfo.UPDATED_POLICY_ITEM;
                                SecEditDiffSectionInfo secEditDiffInfo = new SecEditDiffSectionInfo();

                                secEditDiffInfo.Name = Section.Name;
                                secEditDiffInfo.Type = SecEditDiffSectionInfo.UPDATED_POLICY_ITEM;
                                secEditDiffInfo.Values = Items;

                                returnValue.SecEditDifferenceInfo.Sections.Add(secEditDiffInfo);
                            }

                            found = true;
                            break;
                        }
                    }

                    if (!found && Section.Entries.Count > 0)
                    {
                        returnValue.SecEditDifferenceInfo.Type = SecEditDiffInfo.UPDATED_POLICY_ITEM;
                        SecEditDiffSectionInfo secEditDiffInfo = new SecEditDiffSectionInfo();

                        secEditDiffInfo.Name = Section.Name;
                        secEditDiffInfo.Type = SecEditDiffSectionInfo.NEW_POLICY_ITEM;
                        secEditDiffInfo.Values = Section.Compare(new SecEditSection());

                        returnValue.SecEditDifferenceInfo.Sections.Add(secEditDiffInfo);

                    }
                }

                foreach (SecEditSection oldSection in OldPolicy.SecEditFileData.Sections)
                {
                    bool found = false;

                    foreach (SecEditSection Section in SecEditFileData.Sections)
                    {
                        if (oldSection.Name.Equals(Section.Name))
                        {                         
                            found = true;
                            break;
                        }
                    }

                    if (!found && oldSection.Entries.Count>0)
                    {
                        returnValue.SecEditDifferenceInfo.Type = SecEditDiffInfo.UPDATED_POLICY_ITEM;
                        SecEditDiffSectionInfo secEditDiffInfo = new SecEditDiffSectionInfo();

                        secEditDiffInfo.Name = oldSection.Name;
                        secEditDiffInfo.Type = SecEditDiffSectionInfo.DELETED_POLICY_ITEM;
                        secEditDiffInfo.Values = new SecEditSection().Compare(oldSection);

                        returnValue.SecEditDifferenceInfo.Sections.Add(secEditDiffInfo);
                    }
                }
            }

            if (IEAKFileData != null && IEAKFileData.Sections != null &&
                OldPolicy.IEAKFileData != null && OldPolicy.IEAKFileData.Sections != null)
            {

                foreach (SecEditSection Section in IEAKFileData.Sections)
                {
                    bool found = false;

                    foreach (SecEditSection oldSection in OldPolicy.IEAKFileData.Sections)
                    {
                        if (oldSection.Name.Equals(Section.Name))
                        {
                            List<SecEditDiffValueInfo> Items = Section.Compare(oldSection);

                            if (Items.Count > 0)
                            {
                                returnValue.IEAKDifferenceInfo.Type = SecEditDiffInfo.UPDATED_POLICY_ITEM;
                                SecEditDiffSectionInfo secEditDiffInfo = new SecEditDiffSectionInfo();

                                secEditDiffInfo.Name = Section.Name;
                                secEditDiffInfo.Type = SecEditDiffSectionInfo.UPDATED_POLICY_ITEM;
                                secEditDiffInfo.Values = Items;

                                returnValue.IEAKDifferenceInfo.Sections.Add(secEditDiffInfo);
                            }

                            found = true;
                            break;
                        }
                    }

                    if (!found && Section.Entries.Count > 0)
                    {
                        returnValue.IEAKDifferenceInfo.Type = SecEditDiffInfo.UPDATED_POLICY_ITEM;
                        SecEditDiffSectionInfo secEditDiffInfo = new SecEditDiffSectionInfo();

                        secEditDiffInfo.Name = Section.Name;
                        secEditDiffInfo.Type = SecEditDiffSectionInfo.NEW_POLICY_ITEM;
                        secEditDiffInfo.Values = Section.Compare(new SecEditSection());

                        returnValue.IEAKDifferenceInfo.Sections.Add(secEditDiffInfo);

                    }
                }

                foreach (SecEditSection oldSection in OldPolicy.IEAKFileData.Sections)
                {
                    bool found = false;

                    foreach (SecEditSection Section in IEAKFileData.Sections)
                    {
                        if (oldSection.Name.Equals(Section.Name))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found && oldSection.Entries.Count > 0)
                    {
                        returnValue.IEAKDifferenceInfo.Type = SecEditDiffInfo.UPDATED_POLICY_ITEM;
                        SecEditDiffSectionInfo secEditDiffInfo = new SecEditDiffSectionInfo();

                        secEditDiffInfo.Name = oldSection.Name;
                        secEditDiffInfo.Type = SecEditDiffSectionInfo.DELETED_POLICY_ITEM;
                        secEditDiffInfo.Values = new SecEditSection().Compare(oldSection);

                        returnValue.IEAKDifferenceInfo.Sections.Add(secEditDiffInfo);
                    }
                }
            }

            if (IEAKMachineFileData != null && IEAKMachineFileData.Sections != null &&
                OldPolicy.IEAKMachineFileData != null && OldPolicy.IEAKMachineFileData.Sections != null)
            {

                foreach (SecEditSection Section in IEAKMachineFileData.Sections)
                {
                    bool found = false;

                    foreach (SecEditSection oldSection in OldPolicy.IEAKMachineFileData.Sections)
                    {
                        if (oldSection.Name.Equals(Section.Name))
                        {
                            List<SecEditDiffValueInfo> Items = Section.Compare(oldSection);

                            if (Items.Count > 0)
                            {
                                returnValue.IEAKDifferenceInfo.Type = SecEditDiffInfo.UPDATED_POLICY_ITEM;
                                SecEditDiffSectionInfo secEditDiffInfo = new SecEditDiffSectionInfo();

                                secEditDiffInfo.Name = Section.Name;
                                secEditDiffInfo.Type = SecEditDiffSectionInfo.UPDATED_POLICY_ITEM;
                                secEditDiffInfo.Values = Items;

                                returnValue.IEAKDifferenceInfo.Sections.Add(secEditDiffInfo);
                            }

                            found = true;
                            break;
                        }
                    }

                    if (!found && Section.Entries.Count > 0)
                    {
                        returnValue.IEAKDifferenceInfo.Type = SecEditDiffInfo.UPDATED_POLICY_ITEM;
                        SecEditDiffSectionInfo secEditDiffInfo = new SecEditDiffSectionInfo();

                        secEditDiffInfo.Name = Section.Name;
                        secEditDiffInfo.Type = SecEditDiffSectionInfo.NEW_POLICY_ITEM;
                        secEditDiffInfo.Values = Section.Compare(new SecEditSection());

                        returnValue.IEAKDifferenceInfo.Sections.Add(secEditDiffInfo);

                    }
                }

                foreach (SecEditSection oldSection in OldPolicy.IEAKMachineFileData.Sections)
                {
                    bool found = false;

                    foreach (SecEditSection Section in IEAKMachineFileData.Sections)
                    {
                        if (oldSection.Name.Equals(Section.Name))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found && oldSection.Entries.Count > 0)
                    {
                        returnValue.IEAKDifferenceInfo.Type = SecEditDiffInfo.UPDATED_POLICY_ITEM;
                        SecEditDiffSectionInfo secEditDiffInfo = new SecEditDiffSectionInfo();

                        secEditDiffInfo.Name = oldSection.Name;
                        secEditDiffInfo.Type = SecEditDiffSectionInfo.DELETED_POLICY_ITEM;
                        secEditDiffInfo.Values = new SecEditSection().Compare(oldSection);

                        returnValue.IEAKDifferenceInfo.Sections.Add(secEditDiffInfo);
                    }
                }
            }

            foreach (WMIItem WmiFilter in WMIFilters)
            {
                bool found = false;

                foreach (WMIItem oldWmiFilter in OldPolicy.WMIFilters)
                {
                    if (oldWmiFilter.WMIFilter.Equals(WmiFilter.WMIFilter))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {                    
                    returnValue.WMIDifferenceInfo.Type = WMIDiffInfo.UPDATED_POLICY_ITEM;

                    WMIDiffInfoItem wmiItem = new WMIDiffInfoItem();
                    wmiItem.WMIQuery = WmiFilter.WMIFilter ;
                    wmiItem.Type = SecEditDiffSectionInfo.NEW_POLICY_ITEM;

                    returnValue.WMIDifferenceInfo.WMIItems.Add(wmiItem);
                }
            }

            foreach (WMIItem oldWmiFilter in OldPolicy.WMIFilters)
            {
                bool found = false;

                foreach (WMIItem WmiFilter in WMIFilters)
                {
                    if (oldWmiFilter.WMIFilter.Equals(WmiFilter.WMIFilter))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    returnValue.WMIDifferenceInfo.Type = WMIDiffInfo.UPDATED_POLICY_ITEM;

                    WMIDiffInfoItem wmiItem = new WMIDiffInfoItem();
                    wmiItem.WMIQuery = oldWmiFilter.WMIFilter;
                    wmiItem.Type = SecEditDiffSectionInfo.DELETED_POLICY_ITEM;

                    returnValue.WMIDifferenceInfo.WMIItems.Add(wmiItem);
                }
            }



            foreach (AssignmentItem Assignment in AppliedTo)
            {
                bool found = false;

                foreach (AssignmentItem oldAssignment in OldPolicy.AppliedTo)
                {
                    if (oldAssignment.Assignment.Equals(Assignment.Assignment))
                    {
                        found=true;
                        break;
                    }
                }

                if (!found)
                {
                    returnValue.AssignmentsDifferenceInfo.Type = AssignmentsDiffInfo.UPDATED_POLICY_ITEM;

                    AssignmentsDiffInfoItem assignItem = new AssignmentsDiffInfoItem();
                    assignItem.Name = Assignment.Assignment;
                    assignItem.Type = AssignmentsDiffInfoItem.NEW_POLICY_ITEM;

                    returnValue.AssignmentsDifferenceInfo.Assignmments.Add(assignItem);
                }
            }

            foreach (AssignmentItem oldAssignment in OldPolicy.AppliedTo)
            {
                bool found = false;

                foreach (AssignmentItem Assignment in AppliedTo)
                {
                    if (oldAssignment.Assignment.Equals(Assignment.Assignment))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    
                    returnValue.AssignmentsDifferenceInfo.Type = AssignmentsDiffInfo.UPDATED_POLICY_ITEM;

                    AssignmentsDiffInfoItem assignItem = new AssignmentsDiffInfoItem();
                    assignItem.Name = oldAssignment.Assignment;
                    assignItem.Type = AssignmentsDiffInfoItem.DELETED_POLICY_ITEM;

                    returnValue.AssignmentsDifferenceInfo.Assignmments.Add(assignItem);

                }
            }

            foreach (LinkageItem Linkage in LinkedTo)
            {
                bool found = false;

                foreach (LinkageItem oldLinkage in OldPolicy.LinkedTo)
                {
                    if (oldLinkage.Linkage.Equals(Linkage.Linkage))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    returnValue.LinkageDifferenceInfo.Type = LinkageDiffInfo.UPDATED_POLICY_ITEM;

                    LinkageDiffInfoItem linkItem = new LinkageDiffInfoItem();
                    linkItem.Name = Linkage.Linkage;
                    linkItem.Type = LinkageDiffInfoItem.NEW_POLICY_ITEM;

                    returnValue.LinkageDifferenceInfo.Linkages.Add(linkItem);
                }
            }

            foreach (LinkageItem oldLinkage in OldPolicy.LinkedTo)
            {
                bool found = false;

                foreach (LinkageItem Linkage in LinkedTo)
                {
                    if (oldLinkage.Linkage.Equals(Linkage.Linkage))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    returnValue.LinkageDifferenceInfo.Type = LinkageDiffInfo.UPDATED_POLICY_ITEM;

                    LinkageDiffInfoItem linkItem = new LinkageDiffInfoItem();
                    linkItem.Name = oldLinkage.Linkage;
                    linkItem.Type = LinkageDiffInfoItem.DELETED_POLICY_ITEM;

                    returnValue.LinkageDifferenceInfo.Linkages.Add(linkItem);
                }
            }

            foreach (PolicyFile polFile in PolicyFiles)
            {
                bool found = false;

                foreach (PolicyFile oldPolFile in OldPolicy.PolicyFiles)
                {
                    if (polFile.Type == oldPolFile.Type)
                    {

                        List<RegDiffItemInfo> DiffItems = polFile.Compare(oldPolFile);

                        if (DiffItems.Count > 0)
                        {
                            if (polFile.Type == PolicyFile.POLICY_FILE_TYPE_MACHINE )
                            {
                                returnValue.MachineRegDifferenceInfo.Type = RegDiffInfo.UPDATED_POLICY_ITEM;
                                returnValue.MachineRegDifferenceInfo.Items = polFile.Compare(oldPolFile);
                            }
                            else if (polFile.Type == PolicyFile.POLICY_FILE_TYPE_USER)
                            {
                                returnValue.UserRegDifferenceInfo.Type = RegDiffInfo.UPDATED_POLICY_ITEM;
                                returnValue.UserRegDifferenceInfo.Items = polFile.Compare(oldPolFile);
                            }
                        }                       

                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    if (polFile.Type == PolicyFile.POLICY_FILE_TYPE_MACHINE && polFile.PolicyItems.Count >0)
                    {
                        returnValue.MachineRegDifferenceInfo.Type = RegDiffInfo.NEW_POLICY_ITEM;
                        returnValue.MachineRegDifferenceInfo.Items = polFile.Compare(new PolicyFile());
                    }
                    else if (polFile.Type == PolicyFile.POLICY_FILE_TYPE_USER && polFile.PolicyItems.Count > 0)
                    {
                        returnValue.UserRegDifferenceInfo.Type = RegDiffInfo.NEW_POLICY_ITEM;
                        returnValue.UserRegDifferenceInfo.Items = polFile.Compare(new PolicyFile());
                    }
                }
            }

            foreach (PolicyFile oldPolFile in OldPolicy.PolicyFiles)
            {
                bool found = false;

                foreach (PolicyFile polFile in PolicyFiles)
                {
                    if (polFile.Type == oldPolFile.Type)
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    if (oldPolFile.Type == PolicyFile.POLICY_FILE_TYPE_MACHINE && oldPolFile.PolicyItems.Count > 0)
                    {
                        returnValue.MachineRegDifferenceInfo.Type = RegDiffInfo.DELETED_POLICY_ITEM;
                        returnValue.MachineRegDifferenceInfo.Items = new PolicyFile().Compare(oldPolFile);
                    }
                    else if (oldPolFile.Type == PolicyFile.POLICY_FILE_TYPE_USER && oldPolFile.PolicyItems.Count > 0)
                    {
                        returnValue.UserRegDifferenceInfo.Type = RegDiffInfo.DELETED_POLICY_ITEM;
                        returnValue.UserRegDifferenceInfo.Items = new PolicyFile().Compare(oldPolFile);
                    }
                }
            }

            return returnValue;
        }

    }
}
