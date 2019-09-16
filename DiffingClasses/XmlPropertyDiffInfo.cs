using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PolReader.DiffingClasses
{
    [Serializable()]
    public class XmlPropertyDiffInfo : ISerializable
    {

        public const int NEW_POLICY_ITEM = 1;
        public const int DELETED_POLICY_ITEM = 2;
        public const int UPDATED_POLICY_ITEM = 3;
        public const int UNCHANGED_POLICY_ITEM = 4;

        public PolReaderXMLProperty OldItem { get; set; }
        public PolReaderXMLProperty NewItem { get; set; }

        public int Type { get; set; }
        public int DBID { get; set; }

        public BaseDiffViewItem GetViewItems(BaseDiffViewItem parent)
        {

            BaseDiffViewItem Item = new BaseDiffViewItem(parent);

            Item.IconString = IconString;
            Item.Name = InfoText;

            return Item;

        }


        public XmlPropertyDiffInfo()
        {
            OldItem = new PolReaderXMLProperty();
            NewItem = new PolReaderXMLProperty();            
        }

        public XmlPropertyDiffInfo(SerializationInfo info, StreamingContext ctxt)
        {
            this.Type = (int)info.GetValue("Type", typeof(int));
            this.OldItem = (PolReaderXMLProperty)info.GetValue("OldItem", typeof(PolReaderXMLProperty));
            this.NewItem = (PolReaderXMLProperty)info.GetValue("NewItem", typeof(PolReaderXMLProperty)); 
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Type", this.Type);
            info.AddValue("OldItem", this.OldItem);
            info.AddValue("NewItem", this.NewItem);
        }


        public string DisplayName
        {
            get
            {

                return Type == DELETED_POLICY_ITEM?OldItem.Name:NewItem.Name;
            }
        }
        public string DisplayValue
        {
            get
            {
                return Type == DELETED_POLICY_ITEM ? OldItem.Value : NewItem.Value;
            }
        }
        public string DisplayExValue
        {
            get
            {
                return Type == UPDATED_POLICY_ITEM ? OldItem.Value:"";
            }
        }

        public String InfoText
        {
            get
            {
                String ReturnValue = "";

                if (Type == NEW_POLICY_ITEM )
                {
                    ReturnValue = NewItem.Name + "=" + NewItem.Value;
                }
                else if (Type == DELETED_POLICY_ITEM )
                {
                    ReturnValue = OldItem.Name + "=" + OldItem.Value; 
                }
                else if (Type == UPDATED_POLICY_ITEM )
                {
                    ReturnValue = NewItem.Name + "=" + NewItem.Value +
                        "\tOLD:" + OldItem.Value;
                }

                return ReturnValue;
            }

        }

        public PolReaderXMLProperty BaseItem
        {
            get
            {
                PolReaderXMLProperty ReturnValue;

                if (Type == DELETED_POLICY_ITEM)
                {
                    ReturnValue = OldItem;
                }
                else 
                {
                    ReturnValue = NewItem;
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
