using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices;

namespace PolReader.OUSelector
{
    class ADQuery
    {
        public static Domain[] GetDomains()
        {
            ArrayList ReturnValue = new ArrayList();
            
            DomainCollection domains = Forest.GetCurrentForest().Domains;


            foreach (System.DirectoryServices.ActiveDirectory.Domain domain in domains)
            {
                DirectoryEntry ldap = new DirectoryEntry("LDAP://" + domain.Name);
                ReturnValue.Add(new Domain(domain.Name, ldap));
            }

            return (Domain[])ReturnValue.ToArray(typeof(Domain));

        }

        public static OU[] GetOUs(Domain pdomain)
        {
            ArrayList ReturnValue = new ArrayList();

            pdomain.DirectoryEntry.Children.SchemaFilter.Add("organizationalUnit");

            foreach (DirectoryEntry de in pdomain.DirectoryEntry.Children)
            {
                ReturnValue.Add(new OU(de.Name.Replace("OU=",""), de));
            }

            return (OU[])ReturnValue.ToArray(typeof(OU));

        }

        public static DomainMachine[] FindMachine(string MachineName)
        {
            ArrayList ReturnValue = new ArrayList();

            DirectoryEntry entryRoot = new DirectoryEntry("LDAP://RootDSE");
            string domain = (string)entryRoot.Properties["defaultNamingContext"][0];
            DirectoryEntry entryDomain = new DirectoryEntry("LDAP://" + domain);

            using (DirectorySearcher searcher = new DirectorySearcher(entryDomain))
            {                    
                searcher.ReferralChasing = ReferralChasingOption.All;
                searcher.SearchScope = SearchScope.Subtree;
                searcher.Filter = "(&(objectCategory=computer)(objectClass=computer)(cn=" + MachineName + "))";
                SearchResultCollection resultCol = searcher.FindAll();

                if (resultCol != null)
                {
                    for (int counter = 0; counter < resultCol.Count; counter++)
                    {
                        SearchResult result = resultCol[counter];

                        if (result.Properties.Contains("cn"))
                        {
                            DomainMachine Dm = new DomainMachine((String)result.Properties["cn"][0],
                                                            result.Path);
                            ReturnValue.Add(Dm);
                             
                        }
                    }
                }



            }



            return (DomainMachine[])ReturnValue.ToArray(typeof(DomainMachine));

        }

        public static DomainUser[] FindUser(string UserName)
        {
            ArrayList ReturnValue = new ArrayList();

            DirectoryEntry entryRoot = new DirectoryEntry("LDAP://RootDSE");
            string domain = (string)entryRoot.Properties["defaultNamingContext"][0];
            DirectoryEntry entryDomain = new DirectoryEntry("LDAP://" + domain);

            using (DirectorySearcher searcher = new DirectorySearcher(entryDomain))
            {                    
                searcher.ReferralChasing = ReferralChasingOption.All;
                searcher.SearchScope = SearchScope.Subtree;
                searcher.Filter = "(&(objectCategory=user)(objectClass=person)(sAMAccountName=" + UserName + "))";
                SearchResultCollection resultCol = searcher.FindAll();

                if (resultCol != null)
                {
                    for (int counter = 0; counter < resultCol.Count; counter++)
                    {
                        SearchResult result = resultCol[counter];

                        if (result.Properties.Contains("samaccountname"))
                        {
                            DomainUser Du = new DomainUser((String)result.Properties["samaccountname"][0],
                                                            result.Path);
                            ReturnValue.Add(Du);
                             
                        }
                    }
                }



            }



            return (DomainUser[])ReturnValue.ToArray(typeof(DomainUser));

        }

        public static OU[] GetOUs(OU pou)
        {
            ArrayList ReturnValue = new ArrayList();

            pou.DirectoryEntry.Children.SchemaFilter.Add("organizationalUnit");

            foreach (DirectoryEntry de in pou.DirectoryEntry.Children)
            {
                ReturnValue.Add(new OU(de.Name.Replace("OU=", ""), de));
            }

            return (OU[])ReturnValue.ToArray(typeof(OU));

        }
    }
}
