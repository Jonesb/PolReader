using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace PolReader.DiffingClasses
{
    [Serializable()]
    public class RegDiffItemInfo : ISerializable
    {
        public const int NEW_POLICY_ITEM = 1;
        public const int DELETED_POLICY_ITEM = 2;
        public const int UPDATED_POLICY_ITEM = 3;
        public const int UNCHANGED_POLICY_ITEM = 4;

        public PolicyItem OldItem { get; set; }
        public PolicyItem NewItem { get; set; }

        public int Type { get; set; }

        public BaseDiffViewItem GetViewItems(BaseDiffViewItem parent)
        {

            BaseDiffViewItem Item = new BaseDiffViewItem(parent);

            Item.IconString = IconString;
            Item.Name = InfoText;

            return Item;
        }


        public RegDiffItemInfo()
        {
            OldItem = new PolicyItem();
            NewItem = new PolicyItem();            
        }

        public RegDiffItemInfo(SerializationInfo info, StreamingContext ctxt)
        {
            this.Type = (int)info.GetValue("Type", typeof(int));
            this.OldItem = (PolicyItem)info.GetValue("OldItem", typeof(PolicyItem));
            this.NewItem = (PolicyItem)info.GetValue("NewItem", typeof(PolicyItem)); 
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Type", this.Type);
            info.AddValue("OldItem", this.OldItem);
            info.AddValue("NewItem", this.NewItem);
        }

        public string DisplayKey
        {
            get
            {
                String ReturnValue = "";

                if (Type == DELETED_POLICY_ITEM && OldItem.Key.Length > 0)
                {
                    ReturnValue =  OldItem.Key;
                }
                else if (NewItem.Key.Length > 0)
                {
                    ReturnValue =  NewItem.Key;
                }

                return ReturnValue;
            }


        }

        public string DisplayValue
        {
            get
            {
                String ReturnValue = "";

                if (Type == DELETED_POLICY_ITEM && OldItem.Value.Length > 0)
                {
                    ReturnValue = OldItem.Value;
                }
                else if (NewItem.Value.Length > 0)
                {
                    ReturnValue = NewItem.Value;
                }

                return ReturnValue;
            }
        }

        public string DisplayType
        {
            get
            {
                String ReturnValue = "";

                if (Type == DELETED_POLICY_ITEM && OldItem.StringType.Length > 0)
                {
                    ReturnValue = OldItem.StringType;
                }
                else if (NewItem.StringType.Length > 0)
                {
                    ReturnValue = NewItem.StringType;
                }

                return ReturnValue;
            }
        }

        public string DisplayData
        {
            get
            {
                String ReturnValue = "";

                if (Type == DELETED_POLICY_ITEM && OldItem.Data.Length > 0)
                {
                    ReturnValue = OldItem.Data;
                }
                else if (NewItem.Data.Length > 0)
                {
                    ReturnValue = NewItem.Data;
                }

                return ReturnValue;
            }
        }

        public string DisplayExData
        {
            get
            {
                String ReturnValue = "";

                if (Type == UPDATED_POLICY_ITEM && OldItem.Data.Length > 0)
                {
                    ReturnValue = OldItem.Data;
                }                

                return ReturnValue;
            }
        }

        public String InfoText
        {
            get
            {
                String ReturnValue = "";

                if (Type == NEW_POLICY_ITEM && NewItem.Key.Length > 0)
                {
                    ReturnValue = NewItem.Key + "\t" + NewItem.Value
                        + "\t" + NewItem.StringType + "\t" + NewItem.Data;
                }
                else if (Type == DELETED_POLICY_ITEM && OldItem.Key.Length > 0)
                {
                    ReturnValue = OldItem.Key + "\t" + OldItem.Value
                        + "\t" + OldItem.StringType + "\t" + OldItem.Data;
                }
                else if (Type == UPDATED_POLICY_ITEM && NewItem.Key.Length > 0
                    && OldItem.Key.Length > 0)
                {
                    ReturnValue = NewItem.Key + "\t" + NewItem.Value
                        + "\t" + NewItem.StringType + "\t" +
                        "\t" + NewItem.Data +
                        "\tOLD:" + OldItem.Data;
                }

                return ReturnValue;
            }

        }

        public String IconString
        {
            get
            {
                string ReturnType = "";

                switch (Type)
                {
                    case NEW_POLICY_ITEM:
                        ReturnType = "add.png";
                        break;
                    case DELETED_POLICY_ITEM:
                        ReturnType = "delete.png";
                        break;
                    case UPDATED_POLICY_ITEM:
                        ReturnType = "refresh.png";
                        break;
                    case UNCHANGED_POLICY_ITEM:
                        ReturnType = "nochange.png";
                        break;
                    default:
                        break;

                }
                return ReturnType;
            }

        }
    }
}
