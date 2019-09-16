using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PolReader.DiffingClasses
{
    [Serializable()]
    public class DiffPolicyInfo : ISerializable
    {
        public const int UNCHANGED_POLICY = 0;
        public const int NEW_POLICY = 1;
        public const int DELETED_POLICY = 2;
        public const int UPDATED_POLICY = 3;
        public const int PREVIOUS_REVISION_POLICY = 4;
        
        public string PolicyGuid { get; set; }
        public string Name { get; set; }

        public string LastRevisionPolicyGuid { get; set; }
        public string LastRevisionName { get; set; }
        public string LastRevisionBackUpSetID { get; set; } 
        //public List<DiffPolicyItem> Differences { get; set; }

        public bool Updated { get; set; }
        public int UpdateType { get; set; }

        public SecEditDiffInfo SecEditDifferenceInfo { get; set; }
        public IEAKDiffInfo IEAKDifferenceInfo { get; set; }
        public IEAKDiffInfo IEAKMachineDifferenceInfo { get; set; }

        public WMIDiffInfo WMIDifferenceInfo { get; set; }
        public LinkageDiffInfo LinkageDifferenceInfo { get; set; }
        public AssignmentsDiffInfo AssignmentsDifferenceInfo { get; set; }
        public RegDiffInfo MachineRegDifferenceInfo { get; set; }
        public RegDiffInfo UserRegDifferenceInfo { get; set; }

        public XmlNodeDiffInfo PreferenceDifferenceInfo { get; set; }
        public XmlNodeDiffInfo MachinePreferenceDifferenceInfo { get; set; }
        //public XmlNodeDiffInfo AvectoDifferenceInfo { get; set; }
        //public XmlNodeDiffInfo UserAvectoDifferenceInfo { get; set; }

        public List<object> Children
        {
            get
            {
                List<object> children = new List<object>();
                
                if (MachineRegDifferenceInfo.Type != RegDiffInfo.UNCHANGED_POLICY_ITEM) children.Add(MachineRegDifferenceInfo);
                if (UserRegDifferenceInfo.Type != RegDiffInfo.UNCHANGED_POLICY_ITEM) children.Add(UserRegDifferenceInfo);                
                if (LinkageDifferenceInfo.Type != LinkageDiffInfo.UNCHANGED_POLICY_ITEM) children.Add(LinkageDifferenceInfo);
                if (AssignmentsDifferenceInfo.Type != AssignmentsDiffInfo.UNCHANGED_POLICY_ITEM) children.Add(AssignmentsDifferenceInfo);
                if (WMIDifferenceInfo.Type != WMIDiffInfo.UNCHANGED_POLICY_ITEM) children.Add(WMIDifferenceInfo);
                if (PreferenceDifferenceInfo.Type != XmlNodeDiffInfo.UNCHANGED_POLICY) children.Add(PreferenceDifferenceInfo);
                if (MachinePreferenceDifferenceInfo.Type != XmlNodeDiffInfo.UNCHANGED_POLICY) children.Add(MachinePreferenceDifferenceInfo);
                if (SecEditDifferenceInfo.Type != SecEditDiffInfo.UNCHANGED_POLICY_ITEM) children.Add(SecEditDifferenceInfo);
                if (IEAKDifferenceInfo.Type != IEAKDiffInfo.UNCHANGED_POLICY_ITEM) children.Add(IEAKDifferenceInfo);
                if (IEAKMachineDifferenceInfo.Type != IEAKDiffInfo.UNCHANGED_POLICY_ITEM) children.Add(IEAKMachineDifferenceInfo);

                return children;
            }
        }
       

        public BaseDiffViewItem GetViewItems(BaseDiffViewItem parent)
        {
            BaseDiffViewItem Item = new BaseDiffViewItem(parent);

            Item.IconString = IconString;
            Item.Name = Name;

            if(SecEditDifferenceInfo.Type != SecEditDiffInfo.UNCHANGED_POLICY_ITEM)
                Item.Children.Add(SecEditDifferenceInfo.GetViewItems(Item));

            if (IEAKDifferenceInfo.Type != IEAKDiffInfo.UNCHANGED_POLICY_ITEM)
                Item.Children.Add(IEAKDifferenceInfo.GetViewItems(Item));

            if (IEAKMachineDifferenceInfo.Type != IEAKDiffInfo.UNCHANGED_POLICY_ITEM)
                Item.Children.Add(IEAKMachineDifferenceInfo.GetViewItems(Item));

            if (WMIDifferenceInfo.Type != WMIDiffInfo.UNCHANGED_POLICY_ITEM)
                Item.Children.Add(WMIDifferenceInfo.GetViewItems(Item));

            if (LinkageDifferenceInfo.Type != LinkageDiffInfo.UNCHANGED_POLICY_ITEM)
                Item.Children.Add(LinkageDifferenceInfo.GetViewItems(Item));

            if (AssignmentsDifferenceInfo.Type != AssignmentsDiffInfo.UNCHANGED_POLICY_ITEM)
                Item.Children.Add(AssignmentsDifferenceInfo.GetViewItems(Item));

            if (MachineRegDifferenceInfo.Type != RegDiffInfo.UNCHANGED_POLICY_ITEM)
                Item.Children.Add(MachineRegDifferenceInfo.GetViewItems(Item));

            if (UserRegDifferenceInfo.Type != RegDiffInfo.UNCHANGED_POLICY_ITEM)
                Item.Children.Add(UserRegDifferenceInfo.GetViewItems(Item));


            if (PreferenceDifferenceInfo.Type != XmlNodeDiffInfo.UNCHANGED_POLICY)
                Item.Children.Add(PreferenceDifferenceInfo.GetViewItems(Item));

            if (MachinePreferenceDifferenceInfo.Type != XmlNodeDiffInfo.UNCHANGED_POLICY)
                Item.Children.Add(MachinePreferenceDifferenceInfo.GetViewItems(Item));

           // if (AvectoDifferenceInfo.Type != XmlNodeDiffInfo.UNCHANGED_POLICY)
            //    Item.Children.Add(AvectoDifferenceInfo.GetViewItems(Item));

            //if (UserAvectoDifferenceInfo.Type != XmlNodeDiffInfo.UNCHANGED_POLICY)
            //    Item.Children.Add(UserAvectoDifferenceInfo.GetViewItems(Item));

            return Item;
        }

        public bool HasItems()
        {
            return SecEditDifferenceInfo.Type != SecEditDiffInfo.UNCHANGED_POLICY_ITEM ||
                IEAKDifferenceInfo.Type != IEAKDiffInfo.UNCHANGED_POLICY_ITEM ||
                IEAKMachineDifferenceInfo.Type != IEAKDiffInfo.UNCHANGED_POLICY_ITEM ||
                WMIDifferenceInfo.Type != WMIDiffInfo.UNCHANGED_POLICY_ITEM ||
                LinkageDifferenceInfo.Type != LinkageDiffInfo.UNCHANGED_POLICY_ITEM ||
                AssignmentsDifferenceInfo.Type != AssignmentsDiffInfo.UNCHANGED_POLICY_ITEM ||
                MachineRegDifferenceInfo.Type != RegDiffInfo.UNCHANGED_POLICY_ITEM ||
                UserRegDifferenceInfo.Type != RegDiffInfo.UNCHANGED_POLICY_ITEM ||
                PreferenceDifferenceInfo.Type != XmlNodeDiffInfo.UNCHANGED_POLICY ||
                MachinePreferenceDifferenceInfo.Type != XmlNodeDiffInfo.UNCHANGED_POLICY;
                //AvectoDifferenceInfo.Type != XmlNodeDiffInfo.UNCHANGED_POLICY ||
                //UserAvectoDifferenceInfo.Type != XmlNodeDiffInfo.UNCHANGED_POLICY;
        }

        public String IconString
        {
            get
            {
                string ReturnType = "";

                switch (UpdateType)
                {
                    case UNCHANGED_POLICY:
                        ReturnType = "nochange.png";
                        break;  
                    case NEW_POLICY:
                        ReturnType = "add.png";
                        break;
                    case DELETED_POLICY:
                        ReturnType = "delete.png";
                        break;
                    case UPDATED_POLICY:
                        ReturnType = "refresh.png";
                        break;                                   
                    default:
                        break;

                }

                return ReturnType;
            }

        }

        public String TypeString
        {
            get
            {
                string ReturnType="";

                switch (UpdateType)
                {
                    case NEW_POLICY:
                        ReturnType = "NEW_POLICY";
                        break;
                    case DELETED_POLICY:
                        ReturnType = "DELETED_POLICY";
                        break;
                    case UPDATED_POLICY:
                        ReturnType = "UPDATED_POLICY";
                        break;                    
                    default:
                        break;

                }
                return ReturnType;
            }

            set
            {
                int t=0;
                t++;
            }

        }

        public DiffPolicyInfo(SerializationInfo info, StreamingContext ctxt)
        {
            this.PolicyGuid = (string)info.GetValue("PolicyGuid", typeof(string));
            this.Name = (string)info.GetValue("Name", typeof(string));

            this.UpdateType = (int)info.GetValue("UpdateType", typeof(int));

            this.SecEditDifferenceInfo = (SecEditDiffInfo)info.GetValue("SecEditDifferenceInfo", typeof(SecEditDiffInfo));
            this.IEAKDifferenceInfo = (IEAKDiffInfo)info.GetValue("IEAKDifferenceInfo", typeof(IEAKDiffInfo));
            this.IEAKMachineDifferenceInfo = (IEAKDiffInfo)info.GetValue("IEAKMachineDifferenceInfo", typeof(IEAKDiffInfo));
       
            this.WMIDifferenceInfo = (WMIDiffInfo)info.GetValue("WMIDifferenceInfo", typeof(WMIDiffInfo));
            this.LinkageDifferenceInfo = (LinkageDiffInfo)info.GetValue("LinkageDifferenceInfo", typeof(LinkageDiffInfo));
            this.AssignmentsDifferenceInfo = (AssignmentsDiffInfo)info.GetValue("AssignmentsDifferenceInfo", typeof(AssignmentsDiffInfo));

            this.MachineRegDifferenceInfo = (RegDiffInfo)info.GetValue("MachineRegDifferenceInfo", typeof(RegDiffInfo));
            this.UserRegDifferenceInfo = (RegDiffInfo)info.GetValue("UserRegDifferenceInfo", typeof(RegDiffInfo));

            this.PreferenceDifferenceInfo = (XmlNodeDiffInfo)info.GetValue("PreferenceDifferenceInfo", typeof(XmlNodeDiffInfo));
            this.MachinePreferenceDifferenceInfo = (XmlNodeDiffInfo)info.GetValue("MachinePreferenceDifferenceInfo", typeof(XmlNodeDiffInfo));
            //this.AvectoDifferenceInfo = (XmlNodeDiffInfo)info.GetValue("AvectoDifferenceInfo", typeof(XmlNodeDiffInfo));
            //this.UserAvectoDifferenceInfo = (XmlNodeDiffInfo)info.GetValue("UserAvectoDifferenceInfo", typeof(XmlNodeDiffInfo));            

        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("PolicyGuid", this.PolicyGuid);
            info.AddValue("Name", this.Name);

            info.AddValue("UpdateType", this.UpdateType);

            info.AddValue("SecEditDifferenceInfo", this.SecEditDifferenceInfo);
            info.AddValue("IEAKDifferenceInfo", this.IEAKDifferenceInfo);
            info.AddValue("IEAKMachineDifferenceInfo", this.IEAKMachineDifferenceInfo);
            
            info.AddValue("WMIDifferenceInfo", this.WMIDifferenceInfo);
            info.AddValue("LinkageDifferenceInfo", this.LinkageDifferenceInfo);
            info.AddValue("AssignmentsDifferenceInfo", this.AssignmentsDifferenceInfo);

            info.AddValue("MachineRegDifferenceInfo", this.MachineRegDifferenceInfo);
            info.AddValue("UserRegDifferenceInfo", this.UserRegDifferenceInfo);   

            info.AddValue("PreferenceDifferenceInfo", this.PreferenceDifferenceInfo);  
            info.AddValue("MachinePreferenceDifferenceInfo", this.MachinePreferenceDifferenceInfo);  
            //info.AddValue("AvectoDifferenceInfo", this.AvectoDifferenceInfo);  
            //info.AddValue("UserAvectoDifferenceInfo", this.UserAvectoDifferenceInfo);  

        }

        public DiffPolicyInfo()
        {
            PolicyGuid = "";
            Name = "";
            
            SecEditDifferenceInfo = new SecEditDiffInfo();
            IEAKDifferenceInfo = new IEAKDiffInfo(false);
            IEAKMachineDifferenceInfo = new IEAKDiffInfo(true);

            WMIDifferenceInfo = new WMIDiffInfo();
            LinkageDifferenceInfo = new LinkageDiffInfo();
            AssignmentsDifferenceInfo = new AssignmentsDiffInfo();
            MachineRegDifferenceInfo = new RegDiffInfo();
            MachineRegDifferenceInfo.PolicyType = RegDiffInfo.MACHINE_POLICY_TYPE;
            UserRegDifferenceInfo = new RegDiffInfo();
            UserRegDifferenceInfo.PolicyType = RegDiffInfo.USER_POLICY_TYPE;


            PreferenceDifferenceInfo = new XmlNodeDiffInfo();
            MachinePreferenceDifferenceInfo = new XmlNodeDiffInfo();
            //AvectoDifferenceInfo = new XmlNodeDiffInfo();
            //UserAvectoDifferenceInfo = new XmlNodeDiffInfo();

        }

        //public void RecursivePreferenceXMLDeletion(int PolicyBackupID, XmlNodeDiffInfo Item)
        //{

        //    if (Item.Type == XmlNodeDiffInfo.DELETED_POLICY)
        //    {
        //        //Log to Database
        //        if (Item.DBID == 0)
        //            throw new Exception("DBID is 0 for deleted Item : " + Item.Name);

        //        DB.AddPolicyPreferenceNodeDeletion(PolicyBackupID, Item.DBID);
                 
        //    }
        //    else
        //    {
        //        foreach (XmlPropertyDiffInfo PropItem in Item.propItems)
        //        {
        //            if (PropItem.Type == XmlPropertyDiffInfo.DELETED_POLICY_ITEM)
        //            {
        //                if (PropItem.OldItem.DBID == 0)
        //                    throw new Exception("DBID is 0 for deleted Item : " + Item.Name);

        //                DB.AddPolicyPreferencePropertyDeletion(PolicyBackupID, PropItem.OldItem.DBID);
        //            }
        //        }

        //        foreach (XmlNodeDiffInfo NodeItem in Item.nodeItems)
        //        {
        //            RecursivePreferenceXMLDeletion(PolicyBackupID, NodeItem);
        //        }
        //    }
            
        //}
       
        //public void LogDeletions(int BackUpSetID)
        //{
        //    if (HasItems() || (UpdateType == PREVIOUS_REVISION_POLICY))
        //    {
        //        if (UpdateType == DELETED_POLICY)
        //        {
        //            int PolicyBackupID = DB.GetPolicyBackupID(BackUpSetID, Name, PolicyGuid);

        //            DB.AddPolicyDeletion(PolicyBackupID);

        //        }
        //        else if (UpdateType != NEW_POLICY)
        //        {
        //            int PolicyBackupID = DB.GetPolicyBackupID(BackUpSetID, Name, PolicyGuid);

        //            if (UpdateType == PREVIOUS_REVISION_POLICY)
        //            {
        //                PolicyBackupID = DB.CreateHistoricPolicyBackupID(BackUpSetID, Name, PolicyGuid);

        //                DB.AddHistoricDeletions(PolicyBackupID,this.LastRevisionPolicyGuid);

        //            }

        //            foreach (SecEditDiffSectionInfo secDiffSec in SecEditDifferenceInfo.Sections)
        //            {
        //                foreach (SecEditDiffValueInfo secDiffSecVal in secDiffSec.Values)
        //                {
        //                    if (secDiffSecVal.Type == SecEditDiffValueInfo.DELETED_POLICY_ITEM)
        //                    {
                                
        //                        DB.AddPolicySecEditSectionDataDeletion(PolicyBackupID,
        //                                                    secDiffSec.Name,
        //                                                    secDiffSecVal.OldItem.Name,
        //                                                    secDiffSecVal.OldItem.Value);
        //                    }
        //                }
        //            }

        //            foreach (SecEditDiffSectionInfo secDiffSec in IEAKDifferenceInfo.Sections)
        //            {
        //                foreach (SecEditDiffValueInfo secDiffSecVal in secDiffSec.Values)
        //                {
        //                    if (secDiffSecVal.Type == SecEditDiffValueInfo.DELETED_POLICY_ITEM)
        //                    {
                               
        //                        DB.AddPolicyIEAKDataDeletion(PolicyBackupID,
        //                                                    secDiffSec.Name,
        //                                                    secDiffSecVal.OldItem.Name,
        //                                                    secDiffSecVal.OldItem.Value, false);
        //                    }
        //                }
        //            }

        //            foreach (SecEditDiffSectionInfo secDiffSec in IEAKMachineDifferenceInfo.Sections)
        //            {
        //                foreach (SecEditDiffValueInfo secDiffSecVal in secDiffSec.Values)
        //                {
        //                    if (secDiffSecVal.Type == SecEditDiffValueInfo.DELETED_POLICY_ITEM)
        //                    {
        //                        DB.AddPolicyIEAKDataDeletion(PolicyBackupID,
        //                                                    secDiffSec.Name,
        //                                                    secDiffSecVal.OldItem.Name,
        //                                                    secDiffSecVal.OldItem.Value, true);
        //                    }
        //                }
        //            }


        //            foreach (WMIDiffInfoItem wmiDiff in WMIDifferenceInfo.WMIItems)
        //            {
        //                if (wmiDiff.Type == WMIDiffInfoItem.DELETED_POLICY_ITEM)
        //                {
        //                    DB.AddPolicyWMIFilterDeletion(PolicyBackupID, wmiDiff.WMIQuery);
        //                }

        //            }

        //            if (UpdateType != PREVIOUS_REVISION_POLICY)
        //            {

        //                foreach (LinkageDiffInfoItem linkDiff in LinkageDifferenceInfo.Linkages)
        //                {
        //                    if (linkDiff.Type == LinkageDiffInfoItem.DELETED_POLICY_ITEM)
        //                    {
        //                        DB.AddPolicyLinkageDeletion(PolicyBackupID, linkDiff.Name);
        //                    }

        //                }
        //            }

        //            foreach (AssignmentsDiffInfoItem AssignmentsDiff in AssignmentsDifferenceInfo.Assignmments)
        //            {
        //                if (AssignmentsDiff.Type == AssignmentsDiffInfoItem.DELETED_POLICY_ITEM)
        //                {
        //                    DB.AddPolicyAssignmentDeletion(PolicyBackupID, AssignmentsDiff.Name);
        //                }

        //            }


        //            foreach (RegDiffItemInfo regDiff in MachineRegDifferenceInfo.Items)
        //            {
        //                if (regDiff.Type == RegDiffItemInfo.DELETED_POLICY_ITEM)
        //                {
        //                    DB.AddPolicyRegSettingDeletion(PolicyBackupID, "Machine"
        //                        , regDiff.OldItem.Key
        //                        , regDiff.OldItem.Value
        //                        , regDiff.OldItem.Data
        //                        , regDiff.OldItem.StringType);


        //                }
        //            }

        //            foreach (RegDiffItemInfo regDiff in UserRegDifferenceInfo.Items)
        //            {
        //                if (regDiff.Type == RegDiffItemInfo.DELETED_POLICY_ITEM)
        //                {
        //                    DB.AddPolicyRegSettingDeletion(PolicyBackupID, "User"
        //                        , regDiff.OldItem.Key
        //                        , regDiff.OldItem.Value
        //                        , regDiff.OldItem.Data
        //                        , regDiff.OldItem.StringType);
        //                }
        //            }

        //            if (UpdateType != PREVIOUS_REVISION_POLICY)
        //            {
        //                RecursivePreferenceXMLDeletion(PolicyBackupID, PreferenceDifferenceInfo);
        //                RecursivePreferenceXMLDeletion(PolicyBackupID, MachinePreferenceDifferenceInfo);
        //            }                   

        //        }

        //    }
        //}

    }
}
