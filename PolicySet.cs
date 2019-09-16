using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using PolReader.DiffingClasses;

namespace PolReader
{
    [Serializable()]
    public class PolicySet : ISerializable
    {        
        public List<Policy> Policies { get; set; }        
        public GPOManagement GPOM = new GPOManagement();

        public bool CanSaveToDB { get; set; }        

        public delegate void LogUpdate(int Total,int Current,string Details);
        public event LogUpdate LogUpdateMessage;

        public int BackupSetID { get; set; }        

        private void LogProgress(int Count, int Current, string Details)
        {
            if (LogUpdateMessage != null)
            {
                LogUpdateMessage(Count, Current, Details);
            }
        }

        public PolicySet(SerializationInfo info, StreamingContext ctxt)
        {           
            this.Policies = (List<Policy>)info.GetValue("Policies", typeof(List<Policy>));
            CanSaveToDB = false;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Policies", this.Policies);
        }


        public PolicySet()
        {            
            Policies = new List<Policy>();                        
        }

        public void Clear()
        {
            Policies.Clear();

        }

        public void GetAllPolicies()
        {            
            GPOM.GetAllPolicies(Policies);
            CanSaveToDB = true;
        }

        //public void OpenDBBackupSet(int DBBackupID)
        //{
        //   Policies = DB.GetPolicies(DBBackupID);
        //   BackupSetID = DBBackupID;

        //   System.Data.DataSet rsDS = DB.GetAllRegSettings(DBBackupID,-1);
        //   System.Data.DataSet sedDS = DB.GetAllSecEditSectionData(DBBackupID,-1);
        //   System.Data.DataSet ieakDS = DB.GetAllIEAKData(DBBackupID,false,-1);
        //   System.Data.DataSet ieakMachineDS = DB.GetAllIEAKData(DBBackupID, true,-1);

        //   System.Data.DataSet wmiDS = DB.GetAllWMIFilters(DBBackupID, -1);
        //   System.Data.DataSet linkDS = DB.GetAllLinks(DBBackupID, -1);
        //   System.Data.DataSet assDS = DB.GetAllAssignments(DBBackupID, -1);

        //   System.Data.DataSet prefDS = DB.GetAllPreferenceNodes(DBBackupID, -1);
        //   System.Data.DataSet propDS = DB.GetAllPreferenceProperties(DBBackupID, -1);
           
        //   int Current = 0;

        //   foreach (Policy pol in Policies)
        //   {
        //       Current++;
        //       LogProgress(Policies.Count, Current, pol.Name);

        //       int PolcyBackupID = DB.GetPoliciesBackupID(DBBackupID, pol.GUID);

               
        //       sedDS.Tables[0].DefaultView.RowFilter = "policyBackupID = " + PolcyBackupID;
        //       ieakDS.Tables[0].DefaultView.RowFilter = "policyBackupID = " + PolcyBackupID;
        //       wmiDS.Tables[0].DefaultView.RowFilter = "policyBackupID = " + PolcyBackupID;
        //       linkDS.Tables[0].DefaultView.RowFilter = "policyBackupID = " + PolcyBackupID;
        //       assDS.Tables[0].DefaultView.RowFilter = "policyBackupID = " + PolcyBackupID;
               
        //       rsDS.Tables[0].DefaultView.RowFilter = "policyBackupID = " + PolcyBackupID + " AND  PolType = 'User'" ;
        //       pol.PolicyFiles.Add(DB.GetPoliciesPolicyFile(rsDS));
        //       pol.PolicyFiles[pol.PolicyFiles.Count - 1].Type = PolicyFile.POLICY_FILE_TYPE_USER;

        //       rsDS.Tables[0].DefaultView.RowFilter = "policyBackupID = " + PolcyBackupID + " AND  PolType = 'Machine'";                    
        //       pol.PolicyFiles.Add(DB.GetPoliciesPolicyFile(rsDS));
        //       pol.PolicyFiles[pol.PolicyFiles.Count - 1].Type = PolicyFile.POLICY_FILE_TYPE_MACHINE;

        //       pol.AppliedTo = DB.GetPoliciesAssignments(assDS, pol);
        //       pol.LinkedTo = DB.GetPoliciesLinks(linkDS, pol);
        //       pol.WMIFilters = DB.GetPoliciesWMIFilters(wmiDS, pol);

        //       pol.SecEditFileData = DB.GetPoliciesSecEditSectionData(sedDS);
        //       pol.IEAKFileData = DB.GetPoliciesIEAKData(ieakDS);
        //       pol.IEAKMachineFileData = DB.GetPoliciesIEAKData(ieakMachineDS);
               
        //       pol.PreferencesFiles = DB.GetPolicyPreferences(prefDS, propDS,0,"policyBackupID = " + PolcyBackupID + " AND Type = 0");               
        //       pol.MachinePreferencesFiles = DB.GetPolicyPreferences(prefDS, propDS, 0, "policyBackupID = " + PolcyBackupID + " AND Type = 1");
               
        //   }

        //   CanSaveToDB = false;
        //}

        //public bool CreateDBBackupSet()
        //{
        //    //int DomainID = DB.AddDomainID("Delete.Me.Domain");
        //    int DomainID = DB.AddDomainID(GPOM.GetDomainName());
        //    BackupSetID = DB.AddBackupSet(DomainID);

        //    int Current = 0;

        //    foreach (Policy pol in Policies)
        //    {
        //        Current++;
        //        LogProgress(Policies.Count, Current, pol.Name);

        //        int PolicyBackupID = DB.AddPolicyBackupID(BackupSetID, pol.Name, pol.GUID,pol.Version,pol.Date);

        //        foreach (PolicyFile pf in pol.PolicyFiles)
        //        {
        //            foreach(PolicyItem pi in pf.PolicyItems)
        //            {
        //                DB.AddPolicyRegSetting(PolicyBackupID, pf.StringType,
        //                    pi.Key,pi.Value,pi.Data,pi.StringType);
        //            }
        //        }

        //        foreach (AssignmentItem Assignment in pol.AppliedTo)
        //        {
        //            DB.AddPolicyAssignment(PolicyBackupID, Assignment.Assignment);
        //        }

        //        foreach (LinkageItem Link in pol.LinkedTo)
        //        {
        //            DB.AddPolicyLinkage(PolicyBackupID, Link.Linkage);
        //        }

        //        foreach (WMIItem WMIFilter in pol.WMIFilters)
        //        {
        //            DB.AddPolicyWMIFilter(PolicyBackupID, WMIFilter.WMIFilter);
        //        }

        //        foreach (SecEditSection Ses in pol.SecEditFileData.Sections)
        //        {
        //            foreach (SecEditValuePair Sevp in Ses.Entries)
        //            {
        //                DB.AddPolicySecEditSectionData(PolicyBackupID, Ses.Name, Sevp.Name, Sevp.Value);
        //            }
        //        }

        //        foreach (SecEditSection Ses in pol.IEAKFileData.Sections)
        //        {
        //            foreach (SecEditValuePair Sevp in Ses.Entries)
        //            {
        //                DB.AddPolicyIEAKData(PolicyBackupID, Ses.Name, Sevp.Name, Sevp.Value,false);
        //            }
        //        }

        //        foreach (SecEditSection Ses in pol.IEAKMachineFileData.Sections)
        //        {
        //            foreach (SecEditValuePair Sevp in Ses.Entries)
        //            {
        //                DB.AddPolicyIEAKData(PolicyBackupID, Ses.Name, Sevp.Name, Sevp.Value,true);
        //            }
        //        }

        //        DB.AddPolicyPreferenceData(PolicyBackupID,pol.PreferencesFiles, 0, 0);
        //        DB.AddPolicyPreferenceData(PolicyBackupID,pol.MachinePreferencesFiles, 1, 0);



        //    }

        //    return DB.SetBackupFinished(BackupSetID);
        //}

        public List<DiffPolicyInfo> Compare(PolicySet OldPolictSet,Boolean CheckPreviousReleaseVersion)
        {
            List<DiffPolicyInfo> returnValue = new List<DiffPolicyInfo>();

            int Current = 0;

            foreach(Policy pol in Policies)
            {
                Current++;
                LogProgress(Policies.Count, Current, pol.Name);


                bool PolicyFound = false;

                foreach (Policy oldPol in OldPolictSet.Policies)
                {
                    if(pol.GUID.Equals(oldPol.GUID))
                    {
                        DiffPolicyInfo DiffItems = pol.Compare(oldPol);

                        if (DiffItems.HasItems())
                        {
                            DiffItems.UpdateType = DiffPolicyInfo.UPDATED_POLICY;
                            returnValue.Add(DiffItems);
                        }

                        PolicyFound = true;
                        break;
                    }
                }

                if(!PolicyFound)
                {
                    DiffPolicyInfo DiffItems = pol.Compare(new Policy());
                    DiffItems.UpdateType = DiffPolicyInfo.NEW_POLICY;    
                    returnValue.Add(DiffItems);

                    //this is required to find the previous Rel of a policy
                    //The current policies are first queried then if there isn't one found
                    //the last backup policy set is queried
                    if (CheckPreviousReleaseVersion && pol.Name.Length > 7 && pol.Name.Substring(pol.Name.Length - 7, 4).Equals("rel-", StringComparison.CurrentCultureIgnoreCase))
                    {
                        int HighestOldPolicy = 0;
                        Policy LastPolicyRevision = null;

                        try
                        {
                            if (pol.Name.Length > 3)
                            {
                                int CurrentPolicy = Int32.Parse(pol.Name.Substring(pol.Name.Length - 3, 3));

                                foreach (Policy curPol in Policies)
                                {
                                    if (curPol.Name.Length > 3 && curPol.Name.Substring(0, curPol.Name.Length - 3).Equals(pol.Name.Substring(0, pol.Name.Length - 3)))
                                    {

                                        int CurrentNumber = Int32.Parse(curPol.Name.Substring(curPol.Name.Length - 3, 3));

                                        if (CurrentNumber > HighestOldPolicy &&
                                            CurrentPolicy > CurrentNumber)
                                        {
                                            HighestOldPolicy = CurrentNumber;
                                            LastPolicyRevision = curPol;
                                        }

                                    }
                                }

                                if (LastPolicyRevision == null)
                                {
                                    foreach (Policy oldPol in OldPolictSet.Policies)
                                    {
                                        if (oldPol.Name.Length > 3 && oldPol.Name.Substring(0, oldPol.Name.Length - 3).Equals(pol.Name.Substring(0, pol.Name.Length - 3)))
                                        {

                                            int CurrentNumber = Int32.Parse(oldPol.Name.Substring(oldPol.Name.Length - 3, 3));

                                            if (CurrentNumber > HighestOldPolicy &&
                                            CurrentPolicy > CurrentNumber)
                                            {
                                                HighestOldPolicy = CurrentNumber;
                                                LastPolicyRevision = oldPol;
                                            }

                                        }
                                    }
                                }
                            }
                        }
                        finally
                        {

                        }

                        // The previous policy revision is used to duplicate deletion information
                        if (LastPolicyRevision != null)
                        {
                            DiffPolicyInfo lpDiffItems = pol.Compare(LastPolicyRevision);
                            
                            lpDiffItems.LastRevisionName = LastPolicyRevision.Name;
                            lpDiffItems.LastRevisionPolicyGuid = LastPolicyRevision.GUID;
                            lpDiffItems.UpdateType = DiffPolicyInfo.PREVIOUS_REVISION_POLICY;
                            returnValue.Add(lpDiffItems);
                            
                        }
                    }
                }
            }

            foreach (Policy oldPol in OldPolictSet.Policies)
            {
                bool PolicyFound = false;

                foreach (Policy pol in Policies)
                {
                    if (pol.GUID.Equals(oldPol.GUID))
                    {
                        PolicyFound = true;

                        break;
                    }
                }

                if (!PolicyFound)
                {
                    Policy BlankPolicy = new Policy();
                    BlankPolicy.GUID = oldPol.GUID;
                    BlankPolicy.Name = oldPol.Name;

                    DiffPolicyInfo DiffItems = BlankPolicy.Compare(oldPol);
                    DiffItems.UpdateType = DiffPolicyInfo.DELETED_POLICY;
                    returnValue.Add(DiffItems);
                }
            }

            return returnValue;

        }
    }
}
