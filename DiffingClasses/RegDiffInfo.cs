using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PolReader.DiffingClasses
{
    [Serializable()]
    public class RegDiffInfo : ISerializable
    {

        public const int NEW_POLICY_ITEM = 1;
        public const int DELETED_POLICY_ITEM = 2;
        public const int UPDATED_POLICY_ITEM = 3;
        public const int UNCHANGED_POLICY_ITEM = 4;


        public const int MACHINE_POLICY_TYPE = 1;
        public const int USER_POLICY_TYPE = 2;

        public int PolicyType { get; set; }
        public int Type { get; set; }
        public String Name
        {
            get
            {
                if (PolicyType == USER_POLICY_TYPE)
                {
                    return "User Policies";
                }
                else
                {
                    return "Machine Policies";
                }
            }
        }

        public List<RegDiffItemInfo> Items { get; set; }

        public RegDiffInfo()
        {
            Type = UNCHANGED_POLICY_ITEM;
            PolicyType = MACHINE_POLICY_TYPE;

            Items = new List<RegDiffItemInfo>();
        }

        public BaseDiffViewItem GetViewItems(BaseDiffViewItem parent)
        {

            BaseDiffViewItem Item = new BaseDiffViewItem(parent);

            Item.IconString = IconString;
            Item.Name = Name;

            foreach (RegDiffItemInfo regItem in Items)
            {
                Item.Children.Add(regItem.GetViewItems(Item));
            }

            return Item;
        }


        public RegDiffInfo(SerializationInfo info, StreamingContext ctxt)
        {
            this.Type = (int)info.GetValue("Type", typeof(int));
            this.PolicyType = (int)info.GetValue("PolicyType", typeof(int));            
            this.Items = (List<RegDiffItemInfo>)info.GetValue("Items", typeof(List<RegDiffItemInfo>));            
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("PolicyType", this.PolicyType);
            info.AddValue("Type", this.Type);
            info.AddValue("Items", this.Items);            
        }
               

        public String IconString
        {
            get
            {
                string ReturnType = "";

                switch (Type)
                {
                    case NEW_POLICY_ITEM:
                        if (PolicyType == USER_POLICY_TYPE)
                        {
                            ReturnType = "useradd.png";
                        }
                        else
                        {
                            ReturnType = "machineadd.png";
                        }
                        break;
                    case DELETED_POLICY_ITEM:
                        if (PolicyType == USER_POLICY_TYPE)
                        {
                            ReturnType = "userdelete.png";
                        }
                        else
                        {
                            ReturnType = "machinedelete.png";
                        }
                        break;
                    case UPDATED_POLICY_ITEM:
                        if (PolicyType == USER_POLICY_TYPE)
                        {
                            ReturnType = "userrefresh.png";
                        }
                        else
                        {
                            ReturnType = "machinerefresh.png";
                        }
                        break;
                    case UNCHANGED_POLICY_ITEM:
                        if (PolicyType == USER_POLICY_TYPE)
                        {
                            ReturnType = "user.png";
                        }
                        else
                        {
                            ReturnType = "computer.png";
                        }
                        break;
                    default:
                        break;

                }
                return ReturnType;
            }

        }
    }
}
