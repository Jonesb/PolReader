using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.Collections.ObjectModel;
using System.Threading;
using System.IO;


namespace PolReader
{
    //This class requires the GPO console installed
    public class GPOManagement
    {

        public delegate void LogUpdate(int Total,int Current,string Details);
        public event LogUpdate LogUpdateMessage;

        public GPOManagement()
        {

        }

        private void LogProgress(int Count, int Current, string Details)
        {
            if (LogUpdateMessage != null)
            {
                LogUpdateMessage(Count, Current, Details);
            }
        }

        private static string GetForest()
        {
            DirectoryEntry defaultServer = new DirectoryEntry("LDAP://rootDSE");
            string strLdapServer = (string)defaultServer.Properties["rootDomainNamingContext"].Value;

            strLdapServer = strLdapServer.Replace(",", ".").Replace("DC=", "");
            return strLdapServer;
        }


        public string[] GetLinkageOrder(string OUName)
        {
            List<string> ReturnValue = new List<string>();

            try
            {
                
                GPMGMTLib.GPM gpm = new GPMGMTLib.GPM();
                GPMGMTLib.IGPMConstants gpc = gpm.GetConstants();

                GPMGMTLib.IGPMDomain gpd = gpm.GetDomain(GetDomainName(), "", gpc.UseAnyDC);
                GPMGMTLib.GPMSOM gsom = gpd.GetSOM(OUName);

                GPMGMTLib.GPMGPOLinksCollection gsolinks = gsom.GetInheritedGPOLinks();

                foreach (GPMGMTLib.GPMGPOLink link in gsolinks)
                {
                    ReturnValue.Add(link.GPOID);
                }
            }
            catch (Exception e)
            {

            }
            finally
            {

            }

            return ReturnValue.ToArray();
        }

        public string GetDomainName()
        {
            return Environment.GetEnvironmentVariable("USERDNSDOMAIN");
        }

        class ThreadParameterObj
        {
            public Policy Policy{ get; set; }
            public GPMGMTLib.GPMGPO GPOPolItem { get; set; }
            public string forest { get; set; }
            public string domain { get; set; }
            public int DC{ get; set; }
            public int perm { get; set; }
            public int SearchOp { get; set; }
            public int somDomain { get; set; }
            public int somLink { get; set; }
            
                
        }

        private void GetPolicy(object passObj)
        {
            try
            {
                ThreadParameterObj polobj = (ThreadParameterObj)passObj;
                Policy pol = polobj.Policy;
                
                pol.Run();

                GPMGMTLib.GPMGPO name = polobj.GPOPolItem;

                GPMGMTLib.GPM gpm = new GPMGMTLib.GPM();
                GPMGMTLib.IGPMConstants gpc = gpm.GetConstants();

                GPMGMTLib.IGPMDomain gpd = gpm.GetDomain(GetDomainName(), "", polobj.DC);
                GPMGMTLib.GPMSearchCriteria gps = gpm.CreateSearchCriteria();
                GPMGMTLib.IGPMGPOCollection gpoc = gpd.SearchGPOs(gps);

                GPMGMTLib.GPMSitesContainer gpsc = gpm.GetSitesContainer(polobj.forest, polobj.domain, "", polobj.DC);

                GPMGMTLib.GPMSecurityInfo SecurityInfo = name.GetSecurityInfo();

                foreach (GPMGMTLib.GPMPermission Perm in SecurityInfo)
                {
                    if (Perm.Permission == gpc.permGPOApply)
                    {
                        AssignmentItem assItem = new AssignmentItem();
                        assItem.ParentPolicy = pol;
                        try
                        {
                            assItem.Assignment = Perm.Trustee.TrusteeDomain + "\\" + Perm.Trustee.TrusteeName;

                            pol.AppliedTo.Add(assItem);
                        }
                        catch
                        {
                            assItem.Assignment = Perm.Trustee.TrusteeSid;

                            pol.AppliedTo.Add(assItem);
                        }
                    }
                }

                GPMGMTLib.GPMSearchCriteria SearchLinkage = gpm.CreateSearchCriteria();

                SearchLinkage.Add(gpc.SearchPropertySOMLinks, gpc.SearchOpContains, name);

                foreach (GPMGMTLib.GPMSOM SOM in gpd.SearchSOMs(SearchLinkage))
                {
                    LinkageItem lnkItem = new LinkageItem();
                    lnkItem.ParentPolicy = pol;

                    if (SOM.Type == gpc.somDomain)
                    {
                        lnkItem.Linkage = SOM.Path + "(Domain) - Inheritence Blocked (" + SOM.GPOInheritanceBlocked.ToString() + ")";
                        pol.LinkedTo.Add(lnkItem);
                    }
                    else
                    {
                        lnkItem.Linkage = SOM.Path + "(OU) - Inheritence Blocked (" + SOM.GPOInheritanceBlocked.ToString() + ")";
                        pol.LinkedTo.Add(lnkItem);
                    }
                }


                foreach (GPMGMTLib.GPMSOM SOM in gpsc.SearchSites(SearchLinkage))
                {
                    LinkageItem lnkItem = new LinkageItem();
                    lnkItem.ParentPolicy = pol;
                    lnkItem.Linkage = SOM.Name + "(Site)";

                    pol.LinkedTo.Add(lnkItem);
                }

                GPMGMTLib.GPMWMIFilter wmifilter = name.GetWMIFilter();

                if (wmifilter != null)
                {

                    Array rawWMIFilters = (Array)wmifilter.GetQueryList();

                    String[] WMIFilters = new String[rawWMIFilters.Length];
                    rawWMIFilters.CopyTo(WMIFilters, 0);


                    foreach (string filter in WMIFilters)
                    {
                        WMIItem wmiItem = new WMIItem();
                        wmiItem.ParentPolicy = pol;
                        wmiItem.WMIFilter = filter;
                        pol.WMIFilters.Add(wmiItem);
                    }
                }
            }
            catch
            {
            }
            finally
            {
                AddThreadCount(-1);
            }
        }

        private int CurrentThreads = 0;
        private const int MAX_THREADS = 5;
        private object threadLock = new object();

        private void AddThreadCount(int count)
        {
            lock(threadLock)
            {
                CurrentThreads += count;
            }
        
        }

        public void GetAllPolicies(List<Policy> ReturnValue)
        {
            //List<Policy> ReturnValue = new List<Policy>();

           
            int Current = 0;
            GPMGMTLib.GPM gpm = new GPMGMTLib.GPM();
            GPMGMTLib.IGPMConstants gpc = gpm.GetConstants();

            GPMGMTLib.IGPMDomain gpd = gpm.GetDomain(GetDomainName(), "", gpc.UseAnyDC);
            GPMGMTLib.GPMSearchCriteria gps = gpm.CreateSearchCriteria();
            GPMGMTLib.IGPMGPOCollection gpoc = gpd.SearchGPOs(gps);

            //GPMGMTLib.GPMSitesContainer gpsc = gpm.GetSitesContainer(GetForest(), GetDomainName(), "", gpc.UseAnyDC);            

            string forest = GetForest();
            string domain = GetDomainName();

            foreach (GPMGMTLib.GPMGPO gpItem in gpoc)
            {

                while (CurrentThreads >= MAX_THREADS)
                {
                    Thread.Sleep(100);
                }

                Current++;
                LogProgress(gpoc.Count, Current, gpItem.DisplayName);

                Policy pol;
                
                int UserSysVolVersion = 0;

                try
                {
                    UserSysVolVersion = gpItem.UserSysvolVersionNumber;
                }
                catch
                {

                }

                pol = new Policy("\\\\" + gpd.DomainController + "\\sysvol\\" + gpd.Domain + "\\Policies\\" + gpItem.ID,
                                        gpItem.ID, gpItem.DisplayName, gpItem.ModificationTime, UserSysVolVersion, true);

                ReturnValue.Add(pol);

                ThreadParameterObj tpo = new ThreadParameterObj();
                tpo.Policy = pol;
                tpo.GPOPolItem = gpItem;
                tpo.forest = forest;
                tpo.domain = GetDomainName();
                tpo.DC = gpc.UseAnyDC;

                Thread thread = new Thread(this.GetPolicy);
                thread.Start(tpo);
                AddThreadCount(1);
                                
            }

            while (CurrentThreads >0)
            {
                Thread.Sleep(500);
            }

            return ;

        }


    }
}
