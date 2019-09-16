using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32;


namespace PolReader
{
    public class PolicyItemCheckItem
    {
        public int Exists { get; set; }
        public string ExistsInfo { get; set; }
        public string LocalValue { get; set; }

        public string ExistsText()
        {
            if (Exists == PolicyItem.ITEM_NOT_APPLICABLE)
            {
                return "NOT APPLICABLE";
            }
            else if (Exists == PolicyItem.ITEM_EXISTS_SAME)
            {
                return "VALUES ARE THE SAME";
            }
            else if (Exists == PolicyItem.ITEM_EXISTS_TYPE_DIFFERS)
            {
                return "TYPES ARE DIFFERENT";
            }
            else if (Exists == PolicyItem.ITEM_EXISTS_VALUE_DIFFERS)
            {
                return "VALUES ARE DIFFERENT";
            }
            else
            {
                return "NOT SET";
            }
        
        }
    }

    public class PolicyItemLocalCheck
    {
        public PolicyItemCheckItem ActualPolicy { get; set; }
        public PolicyItemCheckItem DiffContextPolicy { get; set; }
        public PolicyItemCheckItem NonPolicy { get; set; }
        public PolicyItemCheckItem DiffContextNonPolicy { get; set; }

        public PolicyItemLocalCheck()
        {
            ActualPolicy = new PolicyItemCheckItem();
            DiffContextPolicy = new PolicyItemCheckItem();
            NonPolicy = new PolicyItemCheckItem();
            DiffContextNonPolicy = new PolicyItemCheckItem();
        }
        
    }

    [Serializable()]
    public class PolicyItem : ISerializable
    {

        public const int ITEM_NOT_APPLICABLE = 0;
        public const int ITEM_EXISTS_SAME = 1;
        public const int ITEM_EXISTS_VALUE_DIFFERS = 2;
        public const int ITEM_EXISTS_TYPE_DIFFERS = 3;
        public const int ITEM_MISSING = 4;

        public String Key { get; set; }
        public String Value { get; set; }
        //public int Type { get; set; }
        public int Size { get; set; }

        public PolicyItemLocalCheck LocalCompare { get; set; }

        private string _StringData;

        public PolicyItem()
        {
            Key = "";
            Value = "";
            _StringData = "";
            Size = 0;
            LocalCompare = new PolicyItemLocalCheck();
        }

        public PolicyItem(SerializationInfo info, StreamingContext ctxt)
        {
            this.Key = (String)info.GetValue("Key", typeof(string));
            this.Value = (String)info.GetValue("Value", typeof(string));
            this._Type = (int)info.GetValue("_Type", typeof(int));
            this.Size = (int)info.GetValue("Size", typeof(int));
            this._StringData = (string)info.GetValue("_StringData", typeof(string));

            LocalCompare = new PolicyItemLocalCheck();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Key", this.Key);
            info.AddValue("Value", this.Value);
            info.AddValue("_Type", this._Type);
            info.AddValue("Size", this.Size);
            info.AddValue("_StringData", this._StringData);
            
        }

        public String Data 
        {
            get
            {
                return _StringData;
            }

        }

        public void Write(bool MachineSide = true)
        {
            RegistryKey reg = MachineSide?Registry.LocalMachine.OpenSubKey(Key,true):Registry.CurrentUser.OpenSubKey(Key,true);
            
            if(reg == null)
            {
                reg = MachineSide ? Registry.LocalMachine.CreateSubKey(Key) : Registry.CurrentUser.CreateSubKey(Key);
            }

            switch (_Type)
            {
                case 0:

                    break;
                case 1:

                    if (Value.StartsWith("**Del", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (Value.StartsWith("**DelVals", StringComparison.CurrentCultureIgnoreCase))
                        {
                            reg.Dispose();

                            if (MachineSide)
                            {
                                Registry.LocalMachine.DeleteSubKeyTree(Key,false);
                            }
                            else
                            {
                                Registry.CurrentUser.DeleteSubKeyTree(Key, false);
                            }
                        }
                        else if (Value.StartsWith("**Del.", StringComparison.CurrentCultureIgnoreCase))
                        {
                            object itemtoDelete = reg.GetValue(Value.ToUpper().Replace("**DEL.", ""));

                            if (itemtoDelete != null)
                            {
                                reg.DeleteValue(Value.ToUpper().Replace("**DEL.", ""));
                            }
                        }
                    }
                    else
                    {
                        reg.SetValue(Value, Data, RegistryValueKind.String);
                    }

                    break;
                case 2:
                    reg.SetValue(Value, Data, RegistryValueKind.ExpandString);
                    break;
                case 3:

                    byte[] bytes = new byte[(Data.Length / 2)];

                    for (int i = 0; i < Data.Length /2; i++)
                    {
                        bytes[i] = byte.Parse(Data.Substring(i * 2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                    }

                    reg.SetValue(Value, bytes, RegistryValueKind.Binary);


                    break;
                case 4:

                    reg.SetValue(Value, Int32.Parse(Data), RegistryValueKind.DWord);

                    break;
                case 5:

                    reg.SetValue(Value, Int32.Parse(Data), RegistryValueKind.DWord);

                    break;
                case 6:



                    break;
                case 7:

                    string[] regItems = Data.Split('\0').Where(t=>t.Length>0).ToArray();
                    reg.SetValue(Value, regItems, RegistryValueKind.MultiString);

                    break;
                case 8:

                    break;
                case 9:

                    break;
                case 10:

                    break;
                case 11:


                    break;
                case 12:


                    break;
                default:
                    break;
            }
        }


        public bool IsKeyDeletion
        {
            get
            {
                return Value.StartsWith("**DelVals", StringComparison.CurrentCultureIgnoreCase);
            }
        }

        public string RegFileString()
        {
            string returnValue = "";

            switch (_Type)
            {
                case 0:

                    break;
                case 1:

                    if (Value.StartsWith("**Del", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (Value.StartsWith("**DelVals", StringComparison.CurrentCultureIgnoreCase))
                        {
                            
                        }
                        else if (Value.StartsWith("**Del.", StringComparison.CurrentCultureIgnoreCase))
                        {
                            returnValue = "\"" + Value.ToUpper().Replace("**DEL.", "") + "\"=-";
                        }
                    }
                    else
                    {
                        returnValue = "\"" + Value + "\"=\"" + Data + "\"";

                    }

                    break;
                case 2:

                    returnValue = "\"" + Value + "\"=hex(2):";

                    byte [] unicodeValues = Encoding.Unicode.GetBytes(Data);

                    for (int i = 0; i < unicodeValues.Length; i++)
                    {
                        if (i > 0)
                        {
                            returnValue += ",";
                        }
                        
                        returnValue += unicodeValues[i].ToString("X2");
                    }

                    if (unicodeValues.Length > 0)
                    {
                        returnValue += ",00,00";
                    }
                    else
                    {
                        returnValue += "00,00";
                    }

                    break;
                case 3:

                    returnValue = "\"" + Value + "\"=hex:";

                    for (int i = 0; i < Data.Length; i++,i++)
                    {
                        if (i > 0)
                        {
                            returnValue += ",";
                        }

                        returnValue += Data.Substring(i,2);                        
                    }
                    

                    break;
                case 4:
                    
                    returnValue = "\"" + Value + "\"=dword:" + Int32.Parse(Data).ToString("X8");

                    break;
                case 5:
                    

                    break;
                case 6:



                    break;
                case 7:

                    returnValue = "\"" + Value + "\"=hex(7):";

                    byte [] multiunicodeValues = Encoding.Unicode.GetBytes(Data);

                    for (int i = 0; i < multiunicodeValues.Length; i++)
                    {
                        if (i > 0)
                        {
                            returnValue += ",";
                        }

                        returnValue += multiunicodeValues[i].ToString("X2");
                    }

                    break;
                case 8:

                    break;
                case 9:

                    break;
                case 10:

                    break;
                case 11:


                    break;
                case 12:


                    break;
                default:
                    break;
            }

            return returnValue;
        }

        public void SetData(string Data)
        {
            _StringData = Data;
        }
        public void SetData(Byte[] Data, int size)
        {            

            switch (_Type)
            {
                case 0:
                    _StringData = "";
                    break;
                case 1:
                    //Data = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Data);
                    _StringData = Encoding.Unicode.GetString(Data, 0, Size-2);
                    break;
                case 2:
                    _StringData = Encoding.Unicode.GetString(Data, 0, Size-2);
                    break;
                case 3:

                    for (int c = 0; c < size; c++)
                    {
                        _StringData += Data[c].ToString("X2");
                    }

                    break;
                case 4:

                    _StringData = BitConverter.ToInt32(Data, 0).ToString();                    

                    break;
                case 5:

                    _StringData = BitConverter.ToInt32(Data, 0).ToString();   

                    break;
                case 6:
                    
                    for (int c = 0; c < size; c++)
                    {
                        _StringData += Data[c].ToString("X");
                    }

                    break;
                case 7:
                    _StringData = Encoding.Unicode.GetString(Data, 0, Size);
                    break;
                case 8:
                    for (int c = 0; c < size; c++)
                    {
                        _StringData += Data[c].ToString("X");
                    }
                    break;
                case 9:
                    for (int c = 0; c < size; c++)
                    {
                        _StringData += Data[c].ToString("X");
                    }
                    break;
                case 10:
                    for (int c = 0; c < size; c++)
                    {
                        _StringData += Data[c].ToString("X");
                    }
                    break;
                case 11:

                    _StringData = BitConverter.ToInt64(Data, 0).ToString();  
                    
                    break;
                case 12:

                    _StringData = BitConverter.ToInt64(Data, 0).ToString();  

                    break;
                default:
                    break;
            }

        }

        private int _Type;
        public int Type
        {
            get { return _Type; }
            set
            {
                _Type = value;               
            }
        }

        public string StringType
        {
            get 
            { 
                string ReturnValue = "";
                switch(_Type)
                {
                    case 0:
                        ReturnValue = "REG_NONE";
                        break;
                    case 1:
                        ReturnValue = "REG_SZ";
                        break;
                    case 2:
                        ReturnValue = "REG_EXPAND_SZ";
                        break;
                    case 3:
                        ReturnValue = "REG_BINARY";
                        break;
                    case 4:
                        ReturnValue = "REG_DWORD";
                        break;
                    case 5:
                        ReturnValue = "REG_DWORD_BIG_ENDIAN";
                        break;
                    case 6:
                        ReturnValue = "REG_LINK";
                        break;
                    case 7:
                        ReturnValue = "REG_MULTI_SZ";
                        break;
                    case 8:
                        ReturnValue = "REG_RESOURCE_LIST";
                        break;
                    case 9:
                        ReturnValue = "REG_FULL_RESOURCE_DESCRIPTOR";
                        break;
                    case 10:
                        ReturnValue = "REG_RESOURCE_REQUIREMENTS_LIST";
                        break;
                    case 11:
                        ReturnValue = "REG_QWORD";
                        break;
                    case 12:
                        ReturnValue = "REG_QWORD_LITTLE_ENDIAN";
                        break;
                    default:
                        break;
                }
              
                return ReturnValue; 
            }
            
        }

        public int ConvertType(String Type)
        {
            
                int ReturnValue = 0;
                
                if (Type.Equals("REG_NONE"))
                    ReturnValue = 0;                        
                else if (Type.Equals( "REG_SZ"))
                    ReturnValue = 1;
                else if (Type.Equals(  "REG_EXPAND_SZ"))
                    ReturnValue = 2;
                else if (Type.Equals(  "REG_BINARY"))
                    ReturnValue = 3;
                else if (Type.Equals(  "REG_DWORD"))
                    ReturnValue = 4;
                else if (Type.Equals(  "REG_DWORD_BIG_ENDIAN"))
                    ReturnValue = 5;
                else if (Type.Equals(  "REG_LINK"))
                    ReturnValue = 6;
                else if (Type.Equals(  "REG_MULTI_SZ"))
                    ReturnValue = 7;
                else if (Type.Equals(  "REG_RESOURCE_LIST"))
                    ReturnValue = 8;
                else if (Type.Equals(  "REG_FULL_RESOURCE_DESCRIPTOR"))
                    ReturnValue = 9;
                else if (Type.Equals(  "REG_RESOURCE_REQUIREMENTS_LIST"))
                    ReturnValue = 10;
                else if (Type.Equals(  "REG_QWORD"))
                    ReturnValue = 11;
                else if (Type.Equals("REG_QWORD_LITTLE_ENDIAN"))
                    ReturnValue = 12;

                return ReturnValue;           

        }
               
    }
}
