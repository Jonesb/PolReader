using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PolReader.DiffingClasses
{
    [Serializable()]
    public class WMIDiffInfo : ISerializable
    {

        public const int UNCHANGED_POLICY_ITEM = 0;
        public const int UPDATED_POLICY_ITEM = 1;
                
        public int Type { get; set; }

        public List<WMIDiffInfoItem> WMIItems { get; set; }

        public string Name
        {
            get
            {
                return "WMI Filters";
            }
        }

        public WMIDiffInfo()
        {
            WMIItems = new List<WMIDiffInfoItem>();
        }

        public BaseDiffViewItem GetViewItems(BaseDiffViewItem parent)
        {

            BaseDiffViewItem Item = new BaseDiffViewItem(parent);

            Item.IconString = IconString;
            Item.Name = Name;

            foreach (WMIDiffInfoItem wmiItem in WMIItems)
            {
                Item.Children.Add(wmiItem.GetViewItems(Item));
            }

            return Item;
        }

        public WMIDiffInfo(SerializationInfo info, StreamingContext ctxt)
        {
            this.Type = (int)info.GetValue("Type", typeof(int));
            this.WMIItems = (List<WMIDiffInfoItem>)info.GetValue("WMIItems", typeof(List<WMIDiffInfoItem>));
            
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Type", this.Type);
            info.AddValue("WMIItems", this.WMIItems);
        }

        public String IconString
        {
            get
            {
                string ReturnType = "";

                switch (Type)
                {
                    case UNCHANGED_POLICY_ITEM:
                        ReturnType = "filter.png";
                        break;
                    case UPDATED_POLICY_ITEM:
                        ReturnType = "filterrefresh.png";
                        break;
                    default:
                        break;

                }
                return ReturnType;
            }

        }
    }
}
