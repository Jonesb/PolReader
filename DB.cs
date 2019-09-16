/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Collections;
using System.Text.RegularExpressions;
using PolReader.BackupSet;
using System.Security.Principal;
using System.Windows;

namespace PolReader
{
 
    class DB
    {
        static string ConnStr = "Server=IBLONPSD33X384.gb.ad.drkw.net;Database=GPOAuditor;uid=Gpoauditoruser;pwd=P@ssW0rd;";

        public static bool HasDBSaveRights()
        {

            bool returnValue = false;

            try
            {
                String Query;

                Query = "\n SELECT Username,ISNULL(CanDBSave,0) as 'CanDBSave' FROM UserPermissions " +
                        "\n WHERE UserName like '" + WindowsIdentity.GetCurrent().Name + "%'" +
                        "\n AND CanDBSave = 1";

                SqlDataAdapter data = new SqlDataAdapter(Query, ConnStr);
                data.SelectCommand.CommandTimeout = 180;			//original value 90  

                DataSet ResultSet = new DataSet();
                data.Fill(ResultSet);
                DataView dview = new DataView(ResultSet.Tables[0]);

                for (int i = 0; i < dview.Count; i++)
                {
                    returnValue = true;
                }

            }
            catch (Exception e)
            {

            }

            return returnValue;
        }

        public static int AddDomainID(string DomainName)
        {
            int returnValue = -1;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {    
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddDomainID", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@Domain", SqlDbType.NVarChar, 50)).Value = DomainName;
                
                sqlConn.Open();
                returnValue = Int32.Parse(sqlCmd.ExecuteScalar().ToString());
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static List<BackedUpDomain> GetAllBackedUpDomains()
        {
            List<BackedUpDomain> returnValue = new List<BackedUpDomain>();

            try
            {
                String Query;

                Query = "SELECT ID,NAME From Domains";

                SqlDataAdapter data = new SqlDataAdapter(Query, ConnStr);
                data.SelectCommand.CommandTimeout = 180;			//original value 90  

                DataSet ResultSet = new DataSet();
                data.Fill(ResultSet);
                DataView dview = new DataView(ResultSet.Tables[0]);

                for (int i = 0; i < dview.Count; i++)
                {
                    BackedUpDomain bkdom = new BackedUpDomain(Int32.Parse(dview[i]["ID"].ToString()), dview[i]["Name"].ToString());
                    returnValue.Add(bkdom);
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }

            return returnValue;
        }

        public static int GetPolicyID(int BackupSetID, String GUIDString)
        {
            int returnValue = -1;

            try
            {
                String Query;

                Query = "SELECT  pol.ID as ID FROM Policies as pol" +
                        " WHERE pol.GUID = '" + GUIDString + "'" +
                        " AND pol.DomainID IN (SELECT DomainID from BackupSet WHERE ID = " + BackupSetID + ")";

                SqlDataAdapter data = new SqlDataAdapter(Query, ConnStr);
                data.SelectCommand.CommandTimeout = 180;			//original value 90  

                DataSet ResultSet = new DataSet();
                data.Fill(ResultSet);
                DataView dview = new DataView(ResultSet.Tables[0]);

                for (int i = 0; i < dview.Count; i++)
                {
                    returnValue = Int32.Parse(dview[i]["ID"].ToString());

                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }

            return returnValue;
        }

        public static List<BackupSet.BackupSet> GetAllDomainBackupSets(int ID)
        {
            List<BackupSet.BackupSet> returnValue = new List<BackupSet.BackupSet>();
            
            try
            {
                String Query;

                Query = "SELECT BK.ID as ID ,BK.Date as Date FROM BackupSet as BK, Domains as Dom " +
                        " WHERE BK.DomainID = Dom.ID AND Dom.ID = '" + ID + "' AND BK.Finished = 1" +
                        " ORDER BY Date DESC";

                SqlDataAdapter data = new SqlDataAdapter(Query, ConnStr);
                data.SelectCommand.CommandTimeout = 180;			//original value 90  

                DataSet ResultSet = new DataSet();
                data.Fill(ResultSet);
                DataView dview = new DataView(ResultSet.Tables[0]);

                for (int i = 0; i < dview.Count; i++)
                {
                    BackupSet.BackupSet dbk = new BackupSet.BackupSet();
                    dbk.BackupSetId = Int32.Parse(dview[i]["ID"].ToString());
                    dbk.BackupDate = DateTime.Parse(dview[i]["Date"].ToString());

                    returnValue.Add(dbk);
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }

            return returnValue;
        }

        public static List<BackupSet.BackupSetPolicy> GetAllBackupSetPolicies(int backupSetID)
        {
            List<BackupSet.BackupSetPolicy> returnValue = new List<BackupSet.BackupSetPolicy>();

            try
            {
                String Query;

                Query = "SELECT pbk.ID, pol.Name " +
                        " FROM [GPOAuditor].[dbo].[PolicyBackup] as pbk,[GPOAuditor].[dbo].[Policies] as pol " +
                        " WHERE pol.ID = pbk.PolicyID " +
                        " AND pbk.BackupSetID  = " + backupSetID +
                        " ORDER BY pol.Name";

                SqlDataAdapter data = new SqlDataAdapter(Query, ConnStr);
                data.SelectCommand.CommandTimeout = 180;			//original value 90  

                DataSet ResultSet = new DataSet();
                data.Fill(ResultSet);
                DataView dview = new DataView(ResultSet.Tables[0]);

                for (int i = 0; i < dview.Count; i++)
                {
                    BackupSet.BackupSetPolicy dbk = new BackupSet.BackupSetPolicy();
                    dbk.PolicyBackupID = Int32.Parse(dview[i]["ID"].ToString());
                    dbk.PolicyName = dview[i]["Name"].ToString();

                    returnValue.Add(dbk);
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }

            return returnValue;
        }

        public static bool AddHistoricDeletions(int PolicyBackupID, string LastRevisionPolicyGuid)
        {
            bool returnValue = false;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;

            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddHistoricDeletions", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.CommandTimeout = 600;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;
                sqlCmd.Parameters.Add(new SqlParameter("@LastRevisionPolicyGuid", SqlDbType.NVarChar,50)).Value = LastRevisionPolicyGuid;

                sqlConn.Open();
                sqlCmd.ExecuteNonQuery();
                returnValue = true;
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static int AddPolicyBackupID(int BackupSetID, string Name, string GUID, int Version,DateTime Date)
        {
            int returnValue = -1;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddPolicyBackup", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                
                sqlCmd.Parameters.Add(new SqlParameter("@BackupSetID", SqlDbType.Int)).Value = BackupSetID;
                sqlCmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100)).Value = Name;
                sqlCmd.Parameters.Add(new SqlParameter("@GUID", SqlDbType.NVarChar, 50)).Value = GUID;
                sqlCmd.Parameters.Add(new SqlParameter("@Version", SqlDbType.Int)).Value = Version;
                sqlCmd.Parameters.Add(new SqlParameter("@Date", SqlDbType.DateTime)).Value = Date;

                sqlConn.Open();
                returnValue = Int32.Parse(sqlCmd.ExecuteScalar().ToString());
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

    
        public static int GetPolicyBackupID(int BackupSetID, string Name, string GUID)
        {
            int returnValue = -1;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("GetPolicyBackupID", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@BackupSetID", SqlDbType.Int)).Value = BackupSetID;
                sqlCmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100)).Value = Name;
                sqlCmd.Parameters.Add(new SqlParameter("@GUID", SqlDbType.NVarChar, 50)).Value = GUID;
                
                sqlConn.Open();
                returnValue = Int32.Parse(sqlCmd.ExecuteScalar().ToString());
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }


        public static int GetPreviousBackupID(int BackupSetID)
        {
            int returnValue = -1;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("GetPreviousBackupSetID", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@BackupSetID", SqlDbType.Int)).Value = BackupSetID;

                SqlParameter outParameter = new SqlParameter("@ID", SqlDbType.Int);
                outParameter.Direction = ParameterDirection.Output;
                sqlCmd.Parameters.Add(outParameter);

                sqlConn.Open();
                sqlCmd.ExecuteNonQuery();
                
                if(outParameter.Value !=null && outParameter.Value.ToString().Length >0)
                    returnValue = Int32.Parse(outParameter.Value.ToString());
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static int AddBackupSet(int DomainID)
        {
            int returnValue = -1;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddBackupSet", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@DomainID", SqlDbType.Int)).Value = DomainID;
                
                sqlConn.Open();
                returnValue = Int32.Parse(sqlCmd.ExecuteScalar().ToString());
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static int AddPolicyRegSetting(int PolicyBackupID, string RegPolicyType,
                                        string RegKey, string RegValue, string RegData,
                                        string RegType)
        {
            int returnValue = -1;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddPolicyRegSetting", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;
                sqlCmd.Parameters.Add(new SqlParameter("@RegPolicyType", SqlDbType.NVarChar, 50)).Value = RegPolicyType;
                sqlCmd.Parameters.Add(new SqlParameter("@RegKey", SqlDbType.NVarChar, 2048)).Value = RegKey;
                sqlCmd.Parameters.Add(new SqlParameter("@RegValue", SqlDbType.NVarChar, 255)).Value = RegValue;
                sqlCmd.Parameters.Add(new SqlParameter("@RegData", SqlDbType.NVarChar, RegData.Length)).Value = RegData;
                sqlCmd.Parameters.Add(new SqlParameter("@RegType", SqlDbType.NVarChar, 50)).Value = RegType;
                
                sqlConn.Open();
                returnValue = Int32.Parse(sqlCmd.ExecuteScalar().ToString());
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static int AddPolicyWMIFilter(int PolicyBackupID, string Query)
        {
            int returnValue = -1;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddPolicyWMIFilter", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;
                sqlCmd.Parameters.Add(new SqlParameter("@Query", SqlDbType.NVarChar, 2048)).Value = Query;

                sqlConn.Open();
                returnValue = Int32.Parse(sqlCmd.ExecuteScalar().ToString());
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static int AddPolicyLinkage(int PolicyBackupID, string Linkage)
        {
            int returnValue = -1;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddPolicyLinkage", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;
                sqlCmd.Parameters.Add(new SqlParameter("@Linkage", SqlDbType.NVarChar, 2048)).Value = Linkage;

                sqlConn.Open();
                returnValue = Int32.Parse(sqlCmd.ExecuteScalar().ToString());
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static int AddPolicyAssignment(int PolicyBackupID, string Assignment)
        {
            int returnValue = -1;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddPolicyAssignment", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;
                sqlCmd.Parameters.Add(new SqlParameter("@Assignment", SqlDbType.NVarChar, 2048)).Value = Assignment;

                sqlConn.Open();
                returnValue = Int32.Parse(sqlCmd.ExecuteScalar().ToString());
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static int AddPolicyIEAKData(int PolicyBackupID, string SectionName
           , string SectionValue, string SectionData,bool MachineSetting)
        {
            int returnValue = -1;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddPolicyIEAKData", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;
                sqlCmd.Parameters.Add(new SqlParameter("@SectionName", SqlDbType.NVarChar, 1024)).Value = SectionName;
                sqlCmd.Parameters.Add(new SqlParameter("@SectionValue", SqlDbType.NVarChar, 1024)).Value = SectionValue;
                sqlCmd.Parameters.Add(new SqlParameter("@SectionData", SqlDbType.NVarChar, SectionData.Length)).Value = SectionData;
                sqlCmd.Parameters.Add(new SqlParameter("@MachineSetting", SqlDbType.Bit)).Value = MachineSetting;

                sqlConn.Open();
                returnValue = Int32.Parse(sqlCmd.ExecuteScalar().ToString());
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static int AddPolicySecEditSectionData(int PolicyBackupID, string SectionName
            , string SectionValue, string SectionData)
        {
            int returnValue = -1;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddPolicySecEditSectionData", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;
                sqlCmd.Parameters.Add(new SqlParameter("@SectionName", SqlDbType.NVarChar, 1024)).Value = SectionName;
                sqlCmd.Parameters.Add(new SqlParameter("@SectionValue", SqlDbType.NVarChar, 1024)).Value = SectionValue;
                sqlCmd.Parameters.Add(new SqlParameter("@SectionData", SqlDbType.NVarChar, SectionData.Length)).Value = SectionData;

                sqlConn.Open();
                returnValue = Int32.Parse(sqlCmd.ExecuteScalar().ToString());
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static bool ClonePolicyComments(int SourcePolicy, int DestinationPolicy)
        {
            bool ReturnValue = false;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                
                sqlCmd = new SqlCommand("ClonePolicyComments", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.CommandTimeout = 600;

                sqlCmd.Parameters.Add(new SqlParameter("@SourcePolicyID", SqlDbType.Int)).Value = SourcePolicy;
                sqlCmd.Parameters.Add(new SqlParameter("@DestinationPolicyID", SqlDbType.Int)).Value = DestinationPolicy;


                sqlConn.Open();
                sqlCmd.ExecuteNonQuery();

                ReturnValue = true;

            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                //System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return ReturnValue;
        }

        public static bool AddPolicyRegSettingDeletion(int PolicyBackupID, string RegPolicyType,
                                        string RegKey, string RegValue, string RegData,
                                        string RegType)
        {
            bool returnValue = false;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddPolicyRegSettingDeletion", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;
                sqlCmd.Parameters.Add(new SqlParameter("@RegPolicyType", SqlDbType.NVarChar, 50)).Value = RegPolicyType;
                sqlCmd.Parameters.Add(new SqlParameter("@RegKey", SqlDbType.NVarChar, 2048)).Value = RegKey;
                sqlCmd.Parameters.Add(new SqlParameter("@RegValue", SqlDbType.NVarChar, 255)).Value = RegValue;
                sqlCmd.Parameters.Add(new SqlParameter("@RegData", SqlDbType.NVarChar, RegData.Length)).Value = RegData;
                sqlCmd.Parameters.Add(new SqlParameter("@RegType", SqlDbType.NVarChar, 50)).Value = RegType;
                
                sqlConn.Open();
                sqlCmd.ExecuteNonQuery();
                returnValue = true;
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static bool AddPolicyWMIFilterDeletion(int PolicyBackupID, string Query)
        {
            bool returnValue = false;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddPolicyWMIFilterDeletion", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;
                sqlCmd.Parameters.Add(new SqlParameter("@Query", SqlDbType.NVarChar, 2048)).Value = Query;

                sqlConn.Open();
                sqlCmd.ExecuteNonQuery();
                returnValue = true;
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static bool AddPolicyLinkageDeletion(int PolicyBackupID, string Linkage)
        {
            bool returnValue = false;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddPolicyLinkageDeletion", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;
                sqlCmd.Parameters.Add(new SqlParameter("@Linkage", SqlDbType.NVarChar, 2048)).Value = Linkage;

                sqlConn.Open();
                sqlCmd.ExecuteNonQuery();
                returnValue = true;
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static bool AddPolicyAssignmentDeletion(int PolicyBackupID, string Assignment)
        {
            bool returnValue = false;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddPolicyAssignmentDeletion", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;
                sqlCmd.Parameters.Add(new SqlParameter("@Assignment", SqlDbType.NVarChar, 2048)).Value = Assignment;

                sqlConn.Open();
                sqlCmd.ExecuteNonQuery();
                returnValue = true;
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static bool AddPolicySecEditSectionDataDeletion(int PolicyBackupID, string SectionName
            , string SectionValue, string SectionData)
        {
            bool returnValue = false;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddPolicySecEditSectionDataDeletion", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;
                sqlCmd.Parameters.Add(new SqlParameter("@SectionName", SqlDbType.NVarChar, 1024)).Value = SectionName;
                sqlCmd.Parameters.Add(new SqlParameter("@SectionValue", SqlDbType.NVarChar, 1024)).Value = SectionValue;
                sqlCmd.Parameters.Add(new SqlParameter("@SectionData", SqlDbType.NVarChar, SectionData.Length)).Value = SectionData;

                sqlConn.Open();
                sqlCmd.ExecuteNonQuery();
                returnValue = true;
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static bool AddPolicyIEAKDataDeletion(int PolicyBackupID, string SectionName
            , string SectionValue, string SectionData, bool MachineSetting)
        {
            bool returnValue = false;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddPolicyIEAKDataDeletion", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;
                sqlCmd.Parameters.Add(new SqlParameter("@SectionName", SqlDbType.NVarChar, 1024)).Value = SectionName;
                sqlCmd.Parameters.Add(new SqlParameter("@SectionValue", SqlDbType.NVarChar, 1024)).Value = SectionValue;
                sqlCmd.Parameters.Add(new SqlParameter("@SectionData", SqlDbType.NVarChar, SectionData.Length)).Value = SectionData;
                sqlCmd.Parameters.Add(new SqlParameter("@MachineSetting", SqlDbType.Bit)).Value = MachineSetting;

                sqlConn.Open();
                sqlCmd.ExecuteNonQuery();
                returnValue = true;
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static bool AddPolicyDeletion(int PolicyBackupID)
        {
            bool returnValue = false;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddPolicyDeletion", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;
                
                sqlConn.Open();
                sqlCmd.ExecuteNonQuery();
                returnValue = true;
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static bool SetBackupFinished(int BackupSetID)
        {
            bool returnValue = false;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("SetBackupFinished", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@BackupSetID", SqlDbType.Int)).Value = BackupSetID;
                
                sqlConn.Open();
                sqlCmd.ExecuteNonQuery();
                returnValue = true;
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }
       
        public static List<Policy> GetPolicies(int BackupSetID)
        {
            List<Policy> returnValue = new List<Policy>();

            try
            {
                
                String Query;

                Query = "SELECT pol.GUID as GUID ,pol.Name as Name ,pb.Version as Version, pb.Date as Date " +
                        " FROM PolicyBackup as pb,Policies as pol " +
                        " WHERE pb.PolicyID = pol.ID AND pb.BackupSetID = " + BackupSetID;

                SqlDataAdapter data = new SqlDataAdapter(Query, ConnStr);
                data.SelectCommand.CommandTimeout = 180;			//original value 90  

                DataSet ResultSet = new DataSet();
                data.Fill(ResultSet);
                DataView dview = new DataView(ResultSet.Tables[0]);

                for (int i = 0; i < dview.Count; i++)
                {
                    Policy newPOl = new Policy();

                    newPOl.GUID = dview[i]["GUID"].ToString();
                    newPOl.Name = dview[i]["Name"].ToString();
                    newPOl.Version = Int32.Parse(dview[i]["Version"].ToString());
                    newPOl.Date = DateTime.Parse(dview[i]["Date"].ToString());

                    returnValue.Add(newPOl);

                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }

            return returnValue;
        }

        public static int GetPoliciesBackupID(int BackupSetID,string GUID)
        {
            int returnValue = -1;

            try
            {
                String Query;

                Query = "SELECT  pb.ID as ID FROM PolicyBackup as pb, Policies as pol" +
                        " WHERE pb.PolicyID = pol.ID and pol.GUID = '" + GUID + "'" + 
                        " AND pb.BackupSetID = " + BackupSetID;

                SqlDataAdapter data = new SqlDataAdapter(Query, ConnStr);
                data.SelectCommand.CommandTimeout = 180;			//original value 90  

                DataSet ResultSet = new DataSet();
                data.Fill(ResultSet);
                DataView dview = new DataView(ResultSet.Tables[0]);

                for (int i = 0; i < dview.Count; i++)
                {
                    returnValue = Int32.Parse(dview[i]["ID"].ToString());

                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }

            return returnValue;
        }
              
                        
        public static List<PolReaderXMLProperty> GetPolicyPreferenceProperties( DataSet prefPropResultSet,
                                                                    int NodeID)
        {
            List<PolReaderXMLProperty> returnValue = new List<PolReaderXMLProperty>();

            try
            {

                DataView dview = prefPropResultSet.Tables[0].DefaultView;
                dview.RowFilter = "ParentNode = " + NodeID;

                for (int i = 0; i < dview.Count; i++)
                {
                    PolReaderXMLProperty newPolicyItem = new PolReaderXMLProperty();

                    newPolicyItem.DBID = Int32.Parse(dview[i]["ID"].ToString());
                    newPolicyItem.Name = dview[i]["Name"].ToString();                    
                    newPolicyItem.Value = dview[i]["Value"].ToString();
                    
                    returnValue.Add(newPolicyItem);

                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }

            return returnValue;
        }

        public static List<PolReaderXMLNode> GetPolicyPreferences(DataSet prefNodeResultSet
                                                                , DataSet prefPropResultSet,
                                                                    int ParentID , string Filter)
        {
            List<PolReaderXMLNode> returnValue = new List<PolReaderXMLNode>();

            try
            {

                DataView dview = prefNodeResultSet.Tables[0].DefaultView;
                dview.RowFilter = Filter + " AND ParentID = " + ParentID;

                for (int i = 0; i < dview.Count; i++)
                {
                    PolReaderXMLNode newPolicyItem = new PolReaderXMLNode();

                    newPolicyItem.DBID = Int32.Parse(dview[i]["ID"].ToString());
                    newPolicyItem.Name = dview[i]["Name"].ToString();    
                    newPolicyItem.XMLNodeID = Int32.Parse(dview[i]["XMLNodeID"].ToString());    
                                        
                    newPolicyItem.Properties = GetPolicyPreferenceProperties(prefPropResultSet, newPolicyItem.DBID);

                    returnValue.Add(newPolicyItem);

                }

                foreach (PolReaderXMLNode item in returnValue)
                {
                    item.Children = GetPolicyPreferences(prefNodeResultSet, prefPropResultSet,
                                                                    item.XMLNodeID, Filter);
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }

            return returnValue;
        }

        public static PolicyFile GetPoliciesPolicyFile(DataSet ResultSet)
        {
            PolicyFile returnValue = new PolicyFile();

            try
            {                
                DataView dview = ResultSet.Tables[0].DefaultView;

                for (int i = 0; i < dview.Count; i++)
                {
                    PolicyItem newPolicyItem = new PolicyItem();

                    newPolicyItem.Key = dview[i]["KeyName"].ToString();
                    newPolicyItem.Value = dview[i]["Value"].ToString();
                    newPolicyItem.SetData(dview[i]["Data"].ToString());
                    newPolicyItem.Type = newPolicyItem.ConvertType(dview[i]["Type"].ToString());

                    returnValue.PolicyItems.Add(newPolicyItem);

                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }

            return returnValue;
        }

        public static List<AssignmentItem> GetPoliciesAssignments(DataSet ResultSet, Policy Parent)
        {
            List<AssignmentItem> returnValue = new List<AssignmentItem>();

            try
            {
                DataView dview = ResultSet.Tables[0].DefaultView;

                for (int i = 0; i < dview.Count; i++)
                {
                    AssignmentItem assItem = new AssignmentItem();
                    assItem.Assignment = dview[i]["Assignment"].ToString();
                    assItem.ParentPolicy = Parent;
                    returnValue.Add(assItem);
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            return returnValue;
        }

        public static List<LinkageItem> GetPoliciesLinks(DataSet ResultSet, Policy Parent)
        {
            List<LinkageItem> returnValue = new List<LinkageItem>();

            try
            {

                DataView dview = ResultSet.Tables[0].DefaultView;

                for (int i = 0; i < dview.Count; i++)
                {
                    LinkageItem lnkItem = new LinkageItem();
                    lnkItem.Linkage = dview[i]["Linkage"].ToString();
                    lnkItem.ParentPolicy = Parent;

                    returnValue.Add(lnkItem);
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            return returnValue;
        }

        public static List<WMIItem> GetPoliciesWMIFilters(DataSet ResultSet,Policy Parent)
        {
            List<WMIItem> returnValue = new List<WMIItem>();

            try
            {
                DataView dview = ResultSet.Tables[0].DefaultView;

                for (int i = 0; i < dview.Count; i++)
                {
                    WMIItem wmiItem = new WMIItem();
                    wmiItem.WMIFilter = dview[i]["Query"].ToString();
                    wmiItem.ParentPolicy = Parent;

                    returnValue.Add(wmiItem);
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            return returnValue;
        }


        public static SecEditFile GetPoliciesSecEditSectionData(DataSet ResultSet)
        {
            SecEditFile returnValue = new SecEditFile();

            try
            {

                DataView dview = ResultSet.Tables[0].DefaultView;

                for (int i = 0; i < dview.Count; i++)
                {
                    returnValue.AddData(dview[i]["Section"].ToString(),
                        dview[i]["Value"].ToString(),
                        dview[i]["Data"].ToString());

                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }

            return returnValue;
        }

        public static SecEditFile GetPoliciesIEAKData(DataSet ResultSet)
        {
            SecEditFile returnValue = new SecEditFile();

            try
            {

                DataView dview = ResultSet.Tables[0].DefaultView;

                for (int i = 0; i < dview.Count; i++)
                {
                    returnValue.AddData(dview[i]["Section"].ToString(),
                        dview[i]["Value"].ToString(),
                        dview[i]["Data"].ToString());

                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }

            return returnValue;
        }









        public static DataSet GetAllPreferenceNodes(int BackupSetID, int BackupSetPolicyID)
        {        
            DataSet ResultSet = new DataSet();

            try
            {
                String Query;

                Query = "SELECT ppn.ID as ID" +
                        " ,[PolicyBackupID] as PolicyBackupID" +
                        " ,ParentID as ParentID" +
                        " ,XMLNodeID as XMLNodeID" +
                        " ,[Type] as Type" +
                        " , val.ValueName as Name" +
                        " , val2.ValueName as UniqueID" +
                        " FROM [PolicyPreferenceNodes] as ppn,XMLNodes as xn,SecEditValues as val,SecEditValues as val2" +
                        " WHERE ";

                if (BackupSetPolicyID > -1)
                {
                    Query += " ppn.PolicyBackupID = " + BackupSetPolicyID;
                }
                else
                {
                    Query += " PolicyBackupID IN " +
                                "        (Select ID FROM PolicyBackup Where BackupSetID = " + BackupSetID + ")";
                }

                Query += " AND ppn.XMLNodeID = xn.ID" +
                        " AND xn.UniqueID = val2.ID" +
                        " AND xn.NameID = val.ID";
                

                Query += " ORDER BY ParentID";

                SqlDataAdapter data = new SqlDataAdapter(Query, ConnStr);
                data.SelectCommand.CommandTimeout = 180;			//original value 90  


                data.Fill(ResultSet);
                
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }

            return ResultSet;
        }

        public static DataSet GetAllPreferenceProperties(int BackupSetID, int BackupSetPolicyID)
        {
            DataSet ResultSet = new DataSet();

            try
            {
                String Query;

                Query = "SELECT ppp.[ID]" +
                        " ,[ParentNode] as ParentNode" +
                        " ,val.ValueName as Name" +
                        " ,val2.ValueName as Value" +
                        " FROM PolicyPreferenceProperties as ppp,SecEditValues as val,SecEditValues as val2" +
                        "   WHERE [ParentNode] IN " +
                        "   (SELECT ID FROM [PolicyPreferenceNodes]" +
                        " WHERE ";
                        
                if (BackupSetPolicyID > -1)
                {
                    Query += " PolicyBackupID = " + BackupSetPolicyID;
                }
                else
                {
                    Query += " PolicyBackupID IN " +
                        "        (Select ID FROM PolicyBackup " +
                        "           Where BackupSetID = " + BackupSetID + ")";
                }

                Query += " )";
                Query += "   AND	ppp.NameID = val.ID" +
                        "   AND	ppp.ValueID = val2.ID" +
                        "   ORDER BY ParentNode";

                SqlDataAdapter data = new SqlDataAdapter(Query, ConnStr);
                data.SelectCommand.CommandTimeout = 180;			//original value 90  


                data.Fill(ResultSet);

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }

            return ResultSet;
        }

        public static DataSet GetAllRegSettings(int BackupSetID, int BackupSetPolicyID)
        {
            DataSet ResultSet = new DataSet();

            try
            {
                String Query;

                Query = "SELECT  prs.PolicyBackupID as policyBackupID, " + 
		                "        rk.KeyName as KeyName," + 
		                "        rv.ValueName as Value," + 
		                "        rd.Data as Data," + 
		                "        rt.Name as Type," + 
		                "        rpt.Type as PolType " + 
                        " FROM PolicyRegSettings as prs, " + 
		                "       RegKeys as rk,  " + 
		                "        RegValues as rv," + 
		                "        RegData as rd," + 
		                "        RegTypes as rt," + 
		                "        RegPolicyTypes as rpt  " + 
                        " WHERE " ;

                if (BackupSetPolicyID > -1)
                {
                    Query += " prs.PolicyBackupID = " + BackupSetPolicyID;
                }
                else
                {
                    Query += " prs.PolicyBackupID IN " + 
		                "        (Select ID FROM PolicyBackup Where BackupSetID = " + BackupSetID + ") ";
                }
                                                                       


	           Query += "    AND prs.RegPolicyTypeID = rpt.ID " + 
	                    "    AND prs.RegKeyID = rk.ID " + 
	                    "    AND prs.RegValueID = rv.ID " + 
	                    "    AND prs.RegDataID = rd.ID " +
                        "    AND prs.RegTypeID = rt.ID 	" +
                        " ORDER BY prs.PolicyBackupID,KeyName,Value ";

                SqlDataAdapter data = new SqlDataAdapter(Query, ConnStr);
                data.SelectCommand.CommandTimeout = 180;			//original value 90  


                data.Fill(ResultSet);
                
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }

            return ResultSet;
        }


        public static DataSet GetAllAssignments(int BackupSetID, int BackupSetPolicyID)
        {
            DataSet ResultSet = new DataSet();

            try
            {
                String Query;

                Query = "SELECT  pass.PolicyBackupID as policyBackupID,	" +
                        "        ass.Assignment as Assignment		" +
                        " FROM PolicyAssignments as pass, 	" +
                        "        Assignments as ass 	" +
                        " WHERE pass.PolicyBackupID IN 	" +
                        "        (Select ID FROM PolicyBackup Where BackupSetID = " + BackupSetID + ")" +
                        "    AND pass.AssignmentID = ass.ID 		";

                if (BackupSetPolicyID > -1)
                {
                    Query += " AND pass.PolicyBackupID = " + BackupSetPolicyID;
                }

                Query += " ORDER BY pass.PolicyBackupID,ass.Assignment	";

                SqlDataAdapter data = new SqlDataAdapter(Query, ConnStr);
                data.SelectCommand.CommandTimeout = 180;			//original value 90  
                
                data.Fill(ResultSet);

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            return ResultSet;
        }

        public static DataSet GetAllLinks(int BackupSetID, int BackupSetPolicyID)
        {
            DataSet ResultSet = new DataSet();

            try
            {
                String Query;

                Query = "SELECT  plink.PolicyBackupID as policyBackupID," +
                        "        lk.Linkage as Linkage	" +
                        " FROM PolicyLinkages as plink, Linkages as lk" +
                        " WHERE plink.PolicyBackupID IN 	" +
                        "        (Select ID FROM PolicyBackup Where BackupSetID = " + BackupSetID + ")" +
                        "    AND plink.LinkageID = lk.ID ";

                if (BackupSetPolicyID > -1)
                {
                    Query += " AND plink.PolicyBackupID = " + BackupSetPolicyID;
                }

                Query += " ORDER BY plink.PolicyBackupID,lk.Linkage";

                SqlDataAdapter data = new SqlDataAdapter(Query, ConnStr);
                data.SelectCommand.CommandTimeout = 180;			//original value 90  
                
                data.Fill(ResultSet);

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            return ResultSet;
        }

        public static DataSet GetAllWMIFilters(int BackupSetID, int BackupSetPolicyID)
        {
            DataSet ResultSet = new DataSet();

            try
            {
                String Query;

                Query = "SELECT  pwf.PolicyBackupID as policyBackupID, wf.Query as Query	" +
                        " FROM PolicyWMIFilters as pwf, WMIFilters as wf" +
                        " WHERE ";
                
                if(BackupSetPolicyID > -1)
                {
                        Query += " pwf.PolicyBackupID = " + BackupSetPolicyID;
                }
                else
                {
                        Query += " pwf.PolicyBackupID IN 	" +
                                 " (Select ID FROM PolicyBackup Where BackupSetID = " + BackupSetID + ")" ;
                }                
                 
               Query +=       " AND pwf.WMIFilterID = wf.ID" +
                                " ORDER BY pwf.PolicyBackupID,wf.Query";

                SqlDataAdapter data = new SqlDataAdapter(Query, ConnStr);
                data.SelectCommand.CommandTimeout = 180;			//original value 90  
               
                data.Fill(ResultSet);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            return ResultSet;
        }


        public static DataSet GetAllIEAKData(int BackupSetID, bool MachineSettings, int BackupSetPolicyID)
        {
            DataSet ResultSet = new DataSet();

            try
            {
                String Query;

                Query = "SELECT pieakdata.PolicyBackupID as policyBackupID,sdtsec.SectionName as Section,sdtval.ValueName as Value,sdtdat.Data as Data" +
                        " FROM PolicyIEAKData as pieakdata, SecEditSections as sdtsec, SecEditValues as sdtval, SecEditData as sdtdat" +
                        " WHERE pieakdata.PolicyBackupID IN " +
                        "        (Select ID FROM PolicyBackup Where BackupSetID = " + BackupSetID + ")" +
                        " AND pieakdata.SecEditSectionID = sdtsec.ID" +
                        " AND pieakdata.SecEditValueID = sdtval.ID" +
                        " AND pieakdata.SecEditDataID = sdtdat.ID";

                if (MachineSettings)
                {
                    Query += " AND pieakdata.MachineSetting = 1 ";
                }
                else
                {
                    Query += " AND pieakdata.MachineSetting = 0 ";
                }

                if (BackupSetPolicyID > -1)
                {
                    Query += " AND pieakdata.PolicyBackupID = " + BackupSetPolicyID;
                }

                Query += " ORDER BY pieakdata.PolicyBackupID,Section, Value ";

                SqlDataAdapter data = new SqlDataAdapter(Query, ConnStr);
                data.SelectCommand.CommandTimeout = 180;			//original value 90  
               
                data.Fill(ResultSet);

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }

            return ResultSet;
        }

        public static DataSet GetAllSecEditSectionData(int BackupSetID, int BackupSetPolicyID)
        {
            DataSet ResultSet = new DataSet();

            try
            {
                String Query;

                Query = "SELECT psecdata.PolicyBackupID as policyBackupID,sdtsec.SectionName as Section,sdtval.ValueName as Value,sdtdat.Data as Data" +
                        " FROM PolicySeceditData as psecdata, SecEditSections as sdtsec, SecEditValues as sdtval, SecEditData as sdtdat" +
                        " WHERE ";

                if (BackupSetPolicyID > -1)
                {
                    Query += " psecdata.PolicyBackupID = " + BackupSetPolicyID;
                }
                else
                {
                    Query += " psecdata.PolicyBackupID IN " +
		                "        (Select ID FROM PolicyBackup Where BackupSetID = " + BackupSetID + ")" ;
                }
                    

                 Query += " AND psecdata.SecEditSectionID = sdtsec.ID" +
                        " AND psecdata.SecEditValueID = sdtval.ID" +
                        " AND psecdata.SecEditDataID = sdtdat.ID" +
                        " ORDER BY psecdata.PolicyBackupID,Section, Value ";

                SqlDataAdapter data = new SqlDataAdapter(Query, ConnStr);
                data.SelectCommand.CommandTimeout = 180;			//original value 90  
               
                data.Fill(ResultSet);

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }

            return ResultSet;
        }

        public static bool AddPolicyPreferenceNodeDeletion(int PolicyBackupID, int DBID)
        {
            bool returnValue = false;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddPolicyPreferenceNodeDeletion", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;
                sqlCmd.Parameters.Add(new SqlParameter("@DBID", SqlDbType.Int)).Value = DBID;
                
                sqlConn.Open();
                sqlCmd.ExecuteNonQuery();
                returnValue = true;
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static bool AddPolicyPreferencePropertyDeletion(int BackupSetID, int DBID)
        {
            bool returnValue = false;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddPolicyPreferencePropertyDeletion", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = BackupSetID;
                sqlCmd.Parameters.Add(new SqlParameter("@DBID", SqlDbType.Int)).Value = DBID;

                sqlConn.Open();
                sqlCmd.ExecuteNonQuery();
                returnValue = true;
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static int CreateHistoricPolicyBackupID(int BackupSetID, string Name, string GUID)
        {
            int returnValue = -1;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("CreateHistoricPolicyBackupID", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.CommandTimeout = 600;

                sqlCmd.Parameters.Add(new SqlParameter("@BackupSetID", SqlDbType.Int)).Value = BackupSetID;
                sqlCmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100)).Value = Name;
                sqlCmd.Parameters.Add(new SqlParameter("@GUID", SqlDbType.NVarChar, 50)).Value = GUID;

                sqlConn.Open();
                returnValue = Int32.Parse(sqlCmd.ExecuteScalar().ToString());
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static bool AddPolicyPreferenceData(int BackupSetID,List<PolReaderXMLNode> PreferencesFiles, int Type,int ParentID)
        {
           
            foreach (PolReaderXMLNode item in PreferencesFiles)
            {
                string XMLProperties;                
                                
                XMLProperties = "<Root><Properties>";

                foreach(PolReaderXMLProperty Prop in item.Properties)
                {

                    XMLProperties += "<Property>";
                    XMLProperties += "<Name>" + ConvertToXml(Prop.Name) + "</Name>";
                    XMLProperties += "<Value>" + ConvertToXml(Prop.Value) + "</Value>";
                    XMLProperties += "</Property>";
                }

                XMLProperties += "</Properties></Root>";

                int XmlNodeID = AddPolicyPreferenceNode(BackupSetID,ParentID,item.UniqueName(),item.Name,XMLProperties,Type);

                AddPolicyPreferenceData(BackupSetID, item.Children, Type, XmlNodeID);

            }

            return true;

        }

        public static int AddPolicyPreferenceNode(int PolicyBackupID,int ParentXMLNodeID,string UniqueName,string Name,
                                                    string XMLProperties,int Type)
        {
            int returnValue = -1;
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;


            try
            {    
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("AddPolicyPreferenceData", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;
                sqlCmd.Parameters.Add(new SqlParameter("@ParentXMLNodeID", SqlDbType.Int)).Value = ParentXMLNodeID;
                sqlCmd.Parameters.Add(new SqlParameter("@UniqueName", SqlDbType.NVarChar, 1024)).Value = UniqueName;
                sqlCmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 255)).Value = Name;
                sqlCmd.Parameters.Add(new SqlParameter("@XMLProperties", SqlDbType.NText)).Value = XMLProperties;
                sqlCmd.Parameters.Add(new SqlParameter("@Type", SqlDbType.TinyInt)).Value = Type;
                
                sqlConn.Open();
                SqlDataReader reader = sqlCmd.ExecuteReader();

                while(reader.Read())
                {
                    returnValue = reader.GetInt32(0);
                }
                
            }
            catch (SqlException sqlEx)
            {
                throw sqlEx;
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }

            return returnValue;
        }

        public static System.Windows.Point ReportRegSettings(string ReportStortedProc, int PolicyBackupID, ExcelXML.WorkSheet xlWorkSheet, ADMX.GPOTemplates gpoInfo)
        {
            System.Windows.Point returnValue = new System.Windows.Point(0, 0);
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;

            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand(ReportStortedProc, sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;

                sqlConn.Open();

                SqlDataReader reader = sqlCmd.ExecuteReader();

                int RowCount = 0;

                ExcelXML.Style HeaderStyle = new ExcelXML.Style(ExcelXML.Style.HorizontalAlignmentType.Center,
                                                                   ExcelXML.Style.VerticalAlignmentType.Center,
                                                                   "#636594",
                                                                   "#7F7F7F",
                                                                   true, true, false, false,
                                                                   ExcelXML.Style.BorderWeightType.Medium,
                                                                   false);

                HeaderStyle.FontColor = "#FFFFFF";
                HeaderStyle.FontBold = true;


                ExcelXML.Style PropOddStyle = new ExcelXML.Style(ExcelXML.Style.HorizontalAlignmentType.Left,
                                                               ExcelXML.Style.VerticalAlignmentType.Center,
                                                               "#F9F9F9",
                                                               "#7F7F7F",
                                                               true, true, true, true,
                                                               ExcelXML.Style.BorderWeightType.Thin,
                                                               false);

                ExcelXML.Style PropEvenStyle = new ExcelXML.Style(ExcelXML.Style.HorizontalAlignmentType.Left,
                                                               ExcelXML.Style.VerticalAlignmentType.Center,
                                                               "#C0D2DE",
                                                               "#7F7F7F",
                                                               false, false, true, true,
                                                               ExcelXML.Style.BorderWeightType.Thin,
                                                               false);

                ExcelXML.Style blankStyle = new ExcelXML.Style("#939393");

                while (reader.Read())
                {

                    //Add Headers
                    if (RowCount == 0)
                    {
                        ExcelXML.Row HeaderRow = new ExcelXML.Row(HeaderStyle);

                        for (int x = 0; x < reader.FieldCount; x++)
                        {
                            ExcelXML.Cell Cell = new ExcelXML.Cell();

                            Cell.Data = reader.GetName(x);                            

                            HeaderRow.Cells.Add(Cell);

                            ExcelXML.Column column = new ExcelXML.Column();
                            column.AutoFitWidth = true;
                            xlWorkSheet.Columns.Add(column);

                            if (x + 1 > returnValue.X) returnValue.X = x + 1;

                        }

                        if (gpoInfo != null)
                        {
                            
                            ExcelXML.Cell CellHeaderCat = new ExcelXML.Cell();
                            CellHeaderCat.Data = "Policy Category";
                            HeaderRow.Cells.Add(CellHeaderCat);
                            ExcelXML.Column column1 = new ExcelXML.Column();
                            column1.AutoFitWidth = true;
                            xlWorkSheet.Columns.Add(column1);

                            ExcelXML.Cell CellHeaderDisplayName = new ExcelXML.Cell();
                            CellHeaderDisplayName.Data = "Policy Name";
                            HeaderRow.Cells.Add(CellHeaderDisplayName);
                            ExcelXML.Column column2 = new ExcelXML.Column();
                            column2.AutoFitWidth = true;
                            xlWorkSheet.Columns.Add(column2);

                            ExcelXML.Cell CellHeaderValue = new ExcelXML.Cell();
                            CellHeaderValue.Data = "Policy Value";
                            HeaderRow.Cells.Add(CellHeaderValue);
                            ExcelXML.Column column3 = new ExcelXML.Column();
                            column3.AutoFitWidth = true;
                            xlWorkSheet.Columns.Add(column3);

                            ExcelXML.Cell CellHeaderData = new ExcelXML.Cell();
                            CellHeaderData.Data = "Policy Data";
                            HeaderRow.Cells.Add(CellHeaderData);
                            ExcelXML.Column column4 = new ExcelXML.Column();
                            column4.AutoFitWidth = true;
                            xlWorkSheet.Columns.Add(column4);

                            ExcelXML.Cell CellHeaderExplain = new ExcelXML.Cell();
                            CellHeaderExplain.Data = "Policy Explaination";
                            HeaderRow.Cells.Add(CellHeaderExplain);
                            ExcelXML.Column column5 = new ExcelXML.Column();
                            column5.AutoFitWidth = true;
                            xlWorkSheet.Columns.Add(column5);
                          
                            returnValue.X += 5;

                        }
                        xlWorkSheet.Rows.Add(HeaderRow);
                        RowCount++;
                    }

                    ExcelXML.Row DataRow = new ExcelXML.Row(((RowCount & 1) == 1 ? PropEvenStyle : PropOddStyle));

                    for (int x = 0; x < reader.FieldCount; x++)
                    {

                        ExcelXML.Cell Cell = new ExcelXML.Cell();

                        if (!reader.IsDBNull(x))
                        {
                            Type FieldType = reader.GetFieldType(x);

                            if (FieldType != null)
                            {
                                if (FieldType.Equals(typeof(string)))
                                {
                                    Cell.Data = reader.GetString(x);
                                }
                                else if (FieldType.Equals(typeof(Int32)))
                                {
                                    Cell.Data = reader.GetInt32(x);
                                }
                                else if (FieldType.Equals(typeof(DateTime)))
                                {
                                    Cell.Data = reader.GetDateTime(x);
                                }
                                else if (FieldType.Equals(typeof(Byte)))
                                {
                                    Cell.Data = reader.GetByte(x);
                                }
                                else if (FieldType.Equals(null))
                                {
                                    //xlWorkSheet.Cells[RowCount + 1, x + 1] = reader.GetByte(x);
                                    Cell.Data = "";
                                }
                                else
                                {
                                    throw (new Exception("Unhandled Type in ReportSettings " + FieldType.ToString()));
                                }

                            }
                        }
                        else
                        {
                            Cell.Data = "";
                        }                        

                        DataRow.Cells.Add(Cell);
                        
                    }

                    if (gpoInfo != null)
                    {

                        ADMX.GPOTemplateItem gpoItem = gpoInfo.GetItemInfo(reader.GetString(1).Equals("Machine") ? ADMX.GPOTemplateItem.Machine : ADMX.GPOTemplateItem.User
                               , reader.GetString(2), reader.GetString(3));

                        if (gpoItem != null)
                        {
                            ExcelXML.Cell CellCat = new ExcelXML.Cell();
                            CellCat.Data = gpoItem.CategoryName;
                            DataRow.Cells.Add(CellCat);

                            ExcelXML.Cell CellDisplayName = new ExcelXML.Cell();
                            CellDisplayName.Data = gpoItem.DisplayName;
                            DataRow.Cells.Add(CellDisplayName);

                            ExcelXML.Cell CellText = new ExcelXML.Cell();
                            CellText.Data = gpoItem.Text;
                            DataRow.Cells.Add(CellText);

                            ExcelXML.Cell CellValue = new ExcelXML.Cell();
                            CellValue.Data = gpoItem.DataText(reader.GetString(4));
                            DataRow.Cells.Add(CellValue);

                            ExcelXML.Cell CellExplain = new ExcelXML.Cell();
                            CellExplain.Data = gpoItem.Explaination;
                            DataRow.Cells.Add(CellExplain);
                        }


                    }

                    xlWorkSheet.Rows.Add(DataRow);


                    RowCount++;
                }

                returnValue.Y = RowCount - 1;

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("ReportSettings : " + e.Message);
            }

            return returnValue;
        }

        public static System.Windows.Point ReportSettings(string ReportStortedProc, int PolicyBackupID, ExcelXML.WorkSheet xlWorkSheet)
        {

            System.Windows.Point returnValue = new System.Windows.Point(0, 0);
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;

            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand(ReportStortedProc, sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;

                sqlConn.Open();

                SqlDataReader reader = sqlCmd.ExecuteReader();

                int RowCount = 0;

                ExcelXML.Style HeaderStyle = new ExcelXML.Style(ExcelXML.Style.HorizontalAlignmentType.Center,
                                                                   ExcelXML.Style.VerticalAlignmentType.Center,
                                                                   "#636594",
                                                                   "#7F7F7F",
                                                                   true, true, false, false,
                                                                   ExcelXML.Style.BorderWeightType.Medium,
                                                                   false);

                HeaderStyle.FontColor = "#FFFFFF";
                HeaderStyle.FontBold = true;


                ExcelXML.Style PropOddStyle = new ExcelXML.Style(ExcelXML.Style.HorizontalAlignmentType.Left,
                                                               ExcelXML.Style.VerticalAlignmentType.Center,
                                                               "#F9F9F9",
                                                               "#7F7F7F",
                                                               true, true, true, true,
                                                               ExcelXML.Style.BorderWeightType.Thin,
                                                               false);

                ExcelXML.Style PropEvenStyle = new ExcelXML.Style(ExcelXML.Style.HorizontalAlignmentType.Left,
                                                               ExcelXML.Style.VerticalAlignmentType.Center,
                                                               "#C0D2DE",
                                                               "#7F7F7F",
                                                               false, false, true, true,
                                                               ExcelXML.Style.BorderWeightType.Thin,
                                                               false);

                ExcelXML.Style blankStyle = new ExcelXML.Style("#939393");

                while (reader.Read())
                {
                    
                    //Add Headers
                    if (RowCount == 0)
                    {
                        ExcelXML.Row HeaderRow = new ExcelXML.Row(HeaderStyle);

                        for (int x = 0; x < reader.FieldCount; x++)
                        {
                            ExcelXML.Cell Cell = new ExcelXML.Cell();

                            Cell.Data = reader.GetName(x);

                            if (x + 1 > returnValue.X) returnValue.X = x + 1;

                            HeaderRow.Cells.Add(Cell);

                            ExcelXML.Column column = new ExcelXML.Column();
                            column.AutoFitWidth = true;
                            xlWorkSheet.Columns.Add(column);

                        }
                        

                        xlWorkSheet.Rows.Add(HeaderRow);
                        RowCount++;
                    }

                    ExcelXML.Row DataRow = new ExcelXML.Row(((RowCount & 1) == 1 ? PropEvenStyle : PropOddStyle));

                    for (int x = 0; x < reader.FieldCount; x++)
                    {

                        ExcelXML.Cell Cell = new ExcelXML.Cell();

                        if (!reader.IsDBNull(x))
                        {
                            Type FieldType = reader.GetFieldType(x);

                            if (FieldType != null)
                            {
                                if (FieldType.Equals(typeof(string)))
                                {
                                    Cell.Data = reader.GetString(x);
                                }
                                else if (FieldType.Equals(typeof(Int32)))
                                {
                                    Cell.Data = reader.GetInt32(x);
                                }
                                else if (FieldType.Equals(typeof(DateTime)))
                                {
                                    Cell.Data = reader.GetDateTime(x);
                                }
                                else if (FieldType.Equals(typeof(Byte)))
                                {
                                    Cell.Data = reader.GetByte(x);
                                }
                                else if (FieldType.Equals(null))
                                {
                                    //xlWorkSheet.Cells[RowCount + 1, x + 1] = reader.GetByte(x);
                                    Cell.Data = "";
                                }
                                else
                                {
                                    throw (new Exception("Unhandled Type in ReportSettings " + FieldType.ToString()));
                                }

                            }
                        }
                        else
                        {
                            Cell.Data = "";
                        }

                        DataRow.Cells.Add(Cell);
                        
                        if (x+1 > returnValue.X) returnValue.X = x+1;
                    }

                    xlWorkSheet.Rows.Add(DataRow);
                                 
  
                    RowCount++;
                                        
                }
                returnValue.Y = RowCount-1;

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("ReportSettings : " + e.Message);
            }

            return returnValue;
        }

        public static PreferenceReportItem[] ReportPreferencePolicySettings(int PolicyBackupID)
        {

            ArrayList ReturnValue = new ArrayList();

            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;

            try
            {
                sqlConn = new SqlConnection(ConnStr);
                sqlCmd = new SqlCommand("ReportPreferencePolicySettings", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@PolicyBackupID", SqlDbType.Int)).Value = PolicyBackupID;

                sqlConn.Open();

                SqlDataReader reader = sqlCmd.ExecuteReader();

                while (reader.Read())
                {

                    PreferenceReportItem pri = new PreferenceReportItem();

                    pri.ID = reader.GetInt32(0);
                    pri.XMLNodeID = reader.GetInt32(1);
                    pri.Type = (int)reader.GetByte(2);
                    pri.ParentID = reader.GetInt32(3);
                    pri.Name = reader.GetString(4);
                    pri.NameProperty = reader.GetString(5);
                    pri.ClsidProperty = reader.GetString(6);
                    pri.PropName = reader.GetString(7);
                    pri.PropValue = reader.GetString(8);
                    pri.UserName = reader.GetString(9);
                    pri.Comments = reader.GetString(10);

                    ReturnValue.Add(pri);
                }

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("ReportPreferencePolicySettings : " + e.Message);
            }

            return (PreferenceReportItem[])ReturnValue.ToArray(typeof(PreferenceReportItem));
        }

        public static string ConvertToXml(string ConvertionText)
        {
            return ConvertionText.Replace("\"", "&quot;").Replace("'", "&apos;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("&", "&amp;");
         }

    }
}*/
