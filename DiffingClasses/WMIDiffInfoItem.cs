using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PolReader.DiffingClasses
{
    [Serializable()]
    public class WMIDiffInfoItem : ISerializable
    {
        public const int NEW_POLICY_ITEM = 1;
        public const int DELETED_POLICY_ITEM = 2;

        public int Type { get; set; }
        public String WMIQuery { get; set; }

        public WMIDiffInfoItem()
        {
            WMIQuery = "";
        }

        public BaseDiffViewItem GetViewItems(BaseDiffViewItem parent)
        {

            BaseDiffViewItem Item = new BaseDiffViewItem(parent);

            Item.IconString = IconString;
            Item.Name = WMIQuery;

            return Item;
        }

        public WMIDiffInfoItem(SerializationInfo info, StreamingContext ctxt)
        {
            this.Type = (int)info.GetValue("Type", typeof(int));
            this.WMIQuery = (string)info.GetValue("WMIQuery", typeof(string));
            
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Type", this.Type);
            info.AddValue("WMIQuery", this.WMIQuery);
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
                    default:
                        break;

                }
                return ReturnType;
            }

        }
    }
}
