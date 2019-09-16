using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PolReader.DiffingClasses
{
    [Serializable()]
    public class SecEditDiffValueInfo : ISerializable
    {

        public const int NEW_POLICY_ITEM = 1;
        public const int DELETED_POLICY_ITEM = 2;
        public const int UPDATED_POLICY_ITEM = 3;

        public int Type { get; set; }
        public String Name { get; set; }

        public SecEditValuePair OldItem { get; set; }
        public SecEditValuePair NewItem { get; set; }

        public SecEditDiffValueInfo()
        {
            OldItem = new SecEditValuePair();
            NewItem = new SecEditValuePair();
            Name = "";
        }

        public BaseDiffViewItem GetViewItems(BaseDiffViewItem parent)
        {

            BaseDiffViewItem Item = new BaseDiffViewItem(parent);

            Item.IconString = IconString;
            Item.Name = InfoText;

            return Item;
        }

        public SecEditDiffValueInfo(SerializationInfo info, StreamingContext ctxt)
        {
            this.Type = (int)info.GetValue("Type", typeof(int));
            this.Name = (string)info.GetValue("Name", typeof(string));

            this.OldItem = (SecEditValuePair)info.GetValue("OldItem", typeof(SecEditValuePair));
            this.NewItem = (SecEditValuePair)info.GetValue("NewItem", typeof(SecEditValuePair));   
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Type", this.Type);
            info.AddValue("Name", this.Name);

            info.AddValue("NewItem", this.NewItem);
            info.AddValue("OldItem", this.OldItem);
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
                    default:
                        break;

                }
                return ReturnType;
            }

        }

        public string DisplayName
        {
            get
            {
                String ReturnValue = "";

                if ((Type == NEW_POLICY_ITEM || Type == UPDATED_POLICY_ITEM) && NewItem.Name.Length > 0)
                {
                    ReturnValue = NewItem.Name;
                }
                else if (Type == DELETED_POLICY_ITEM && OldItem.Name.Length > 0)
                {
                    ReturnValue = OldItem.Name;
                }                

                return ReturnValue;
            }
        }
        public string DisplayValue
        {
            get
            {
                String ReturnValue = "";

                if ((Type == NEW_POLICY_ITEM || Type == UPDATED_POLICY_ITEM) && NewItem.Value.Length > 0)
                {
                    ReturnValue = NewItem.Value;
                }
                else if (Type == DELETED_POLICY_ITEM && OldItem.Value.Length > 0)
                {
                    ReturnValue = OldItem.Value;
                }

                return ReturnValue;
            }
        }
        public string DisplayExValue
        {
            get
            {
                String ReturnValue = "";

                if (Type == UPDATED_POLICY_ITEM && OldItem.Value.Length > 0)
                {
                    ReturnValue = OldItem.Value;
                }

                return ReturnValue;
            }
        }

        public String InfoText
        {
            get
            {
                String ReturnValue = "";

                if (Type == NEW_POLICY_ITEM && NewItem.Name.Length > 0)
                {
                    ReturnValue = NewItem.Name + "\t" + NewItem.Value;
                }
                else if (Type == DELETED_POLICY_ITEM && OldItem.Name.Length > 0)
                {
                    ReturnValue = OldItem.Name + "\t" + OldItem.Value;
                }
                else if (Type == UPDATED_POLICY_ITEM && NewItem.Name.Length > 0
                    && OldItem.Name.Length > 0)
                {
                    ReturnValue = NewItem.Name + "\t" +
                        "\t" + NewItem.Value +
                        "\nOLD:" + OldItem.Value;
                }

                return ReturnValue;
            }

        }
    }
}
