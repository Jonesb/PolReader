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
    public class PolicyFile : ISerializable
    {
        public const int POLICY_FILE_TYPE_MACHINE = 1;
        public const int POLICY_FILE_TYPE_USER = 2;

        const int POSITION_REG_END = 0;
        const int POSITION_REG_KEY = 1;
        const int POSITION_REG_VALUE = 2;
        const int POSITION_REG_TYPE = 3;
        const int POSITION_REG_SIZE = 4;
        const int POSITION_REG_DATA = 5;

        public int Signature;
        public int FileVersion;
        public int Type;
        public string Name;

        public List<PolicyItem> PolicyItems { get; set; }

        public PolicyFile()
        {
            PolicyItems = new List<PolicyItem>();
            Name = "";
        }

        public string StringType
        {
            get
            {
                string ReturnValue = "";
                switch (Type)
                {
                    case POLICY_FILE_TYPE_MACHINE:
                        ReturnValue = "Machine";
                        break;
                    case POLICY_FILE_TYPE_USER:
                        ReturnValue = "User";
                        break;                    
                    default:
                        break;
                }

                return ReturnValue;
            }

        }

        public PolicyFile(SerializationInfo info, StreamingContext ctxt)
        {
            this.Signature = (int)info.GetValue("Signature", typeof(int));
            this.FileVersion = (int)info.GetValue("FileVersion", typeof(int));
            this.Type = (int)info.GetValue("Type", typeof(int));
            this.Name = (string)info.GetValue("Name", typeof(string));

            this.PolicyItems = (List<PolicyItem>)info.GetValue("PolicyItems", typeof(List<PolicyItem>));
           

        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Signature", this.Signature);
            info.AddValue("FileVersion", this.FileVersion);
            info.AddValue("Type", this.Type);
            info.AddValue("Name", this.Name);

            info.AddValue("PolicyItems", this.PolicyItems);
            
        }

        public PolicyFile(string FileName, int PolicyType)
        {
            Type = PolicyType;
            Name = FileName;
            PolicyItems = new List<PolicyItem>();
            AddPolicy(FileName);
        }

        public void AddPolicy(string FileName)
        {
            BinaryReader binaryStream = new BinaryReader(File.OpenRead(FileName),Encoding.Unicode);

            PolicyItem PolItem = new PolicyItem() ;

            int length = (int)binaryStream.BaseStream.Length;
            int pos=0;
            int CurrentPos = POSITION_REG_END;

            if (length >= sizeof(int) * 2)
            {
                Signature = binaryStream.ReadInt32();
                FileVersion = binaryStream.ReadInt32();
                pos += sizeof(int) * 2;
            }

            while (pos < length)
            {
                if (CurrentPos == POSITION_REG_END || CurrentPos == POSITION_REG_KEY ||
                    CurrentPos == POSITION_REG_VALUE)
                {
                    char NextChar = binaryStream.ReadChar();
                    pos += sizeof(char);

                    if (NextChar == '[' && CurrentPos == POSITION_REG_END)
                    {
                        PolItem = new PolicyItem();
                        CurrentPos = POSITION_REG_KEY;
                    }
                    else if (NextChar == ']' && CurrentPos == POSITION_REG_END)
                    {                        
                        PolicyItems.Add(PolItem);
                    }
                    else if (NextChar != ';' && CurrentPos == POSITION_REG_KEY)
                    {
                        if (NextChar != '\0')                     
                            PolItem.Key += NextChar;
                    }
                    else if (NextChar == ';' && CurrentPos == POSITION_REG_KEY)
                    {
                        CurrentPos = POSITION_REG_VALUE;                        
                    }
                    else if (NextChar != ';' && CurrentPos == POSITION_REG_VALUE)
                    {
                        if(NextChar != '\0')
                            PolItem.Value += NextChar;
                    }
                    else if (NextChar == ';' && CurrentPos == POSITION_REG_VALUE)
                    {
                        CurrentPos = POSITION_REG_TYPE;
                    }
                }
                else if (CurrentPos == POSITION_REG_TYPE)
                {
                    PolItem.Type = binaryStream.ReadInt32();
                    pos += sizeof(int);
                    CurrentPos = POSITION_REG_SIZE;

                    if (pos < length)
                    {
                        binaryStream.ReadChar();
                        pos += sizeof(char);
                    }

                }
                else if (CurrentPos == POSITION_REG_SIZE)
                {
                    PolItem.Size = binaryStream.ReadInt32();
                    pos += sizeof(int);
                    CurrentPos = POSITION_REG_DATA;

                    if (pos < length)
                    {
                        binaryStream.ReadChar();
                        pos += sizeof(char);
                    }

                    if ((pos + PolItem.Size) < length)
                    {
                        PolItem.SetData(binaryStream.ReadBytes(PolItem.Size), PolItem.Size);                        
                        CurrentPos = POSITION_REG_END;
                        pos += PolItem.Size;
                    }

                }
            }


            binaryStream.Close();
                        
        }

        public List<RegDiffItemInfo> Compare(PolicyFile OldPolicyFile)
        {
            List<RegDiffItemInfo> returnValue = new List<RegDiffItemInfo>();

            foreach (PolicyItem polItem in PolicyItems)
            {
                bool found = false;

                foreach (PolicyItem oldPolItem in OldPolicyFile.PolicyItems)
                {
                    if (polItem.Key.ToUpper().Trim().Equals(oldPolItem.Key.ToUpper().Trim()) &&
                        polItem.Value.ToUpper().Trim().Equals(oldPolItem.Value.ToUpper().Trim()) && 
                        polItem.Type == oldPolItem.Type)
                    {

                        if (!polItem.Data.Trim().Equals(oldPolItem.Data.Trim()))
                        {
                            RegDiffItemInfo DiffItem = new RegDiffItemInfo();
                            DiffItem.Type = RegDiffItemInfo.UPDATED_POLICY_ITEM;
                            DiffItem.OldItem = oldPolItem;
                            DiffItem.NewItem = polItem;

                            returnValue.Add(DiffItem);
                        }

                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    RegDiffItemInfo DiffItem = new RegDiffItemInfo();
                    DiffItem.Type = RegDiffItemInfo.NEW_POLICY_ITEM;                    
                    DiffItem.NewItem = polItem;
                    returnValue.Add(DiffItem);
                }
            }

            foreach (PolicyItem oldPolItem in OldPolicyFile.PolicyItems)
            {
                bool found = false;

                foreach (PolicyItem polItem in PolicyItems)
                {
                    if (polItem.Key.ToUpper().Trim().Equals(oldPolItem.Key.ToUpper().Trim()) &&
                        polItem.Value.ToUpper().Trim().Equals(oldPolItem.Value.ToUpper().Trim()) &&
                        polItem.Type == oldPolItem.Type)
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    RegDiffItemInfo DiffItem = new RegDiffItemInfo();
                    DiffItem.Type = RegDiffItemInfo.DELETED_POLICY_ITEM;
                    DiffItem.OldItem = oldPolItem;                    
                    returnValue.Add(DiffItem);
                }
            }

            return returnValue;

        }
    }

}
