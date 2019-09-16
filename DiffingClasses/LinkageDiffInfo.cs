using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PolReader.DiffingClasses
{
    [Serializable()]
    public class LinkageDiffInfo : ISerializable
    {

        public const int UNCHANGED_POLICY_ITEM = 0;
        public const int UPDATED_POLICY_ITEM = 1;

        public int Type { get; set; }

        public List<LinkageDiffInfoItem> Linkages { get; set; }

        public string Name
        {
            get
            {
                return "Linkages";
            }
        }

        public LinkageDiffInfo()
        {
            Linkages = new List<LinkageDiffInfoItem>();
        }

        public BaseDiffViewItem GetViewItems(BaseDiffViewItem parent)
        {

            BaseDiffViewItem Item = new BaseDiffViewItem(parent);

            Item.IconString = IconString;
            Item.Name = Name;

            foreach (LinkageDiffInfoItem linkageItem in Linkages)
            {
                Item.Children.Add(linkageItem.GetViewItems(Item));
            }

            return Item;
        }


        public LinkageDiffInfo(SerializationInfo info, StreamingContext ctxt)
        {
            this.Type = (int)info.GetValue("Type", typeof(int));
            this.Linkages = (List<LinkageDiffInfoItem>)info.GetValue("Linkages", typeof(List<LinkageDiffInfoItem>));
            
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Type", this.Type);
            info.AddValue("Linkages", this.Linkages);
        }

        public String IconString
        {
            get
            {
                string ReturnType = "";

                switch (Type)
                {
                    case UNCHANGED_POLICY_ITEM:
                        ReturnType = "link.png";
                        break;
                    case UPDATED_POLICY_ITEM:
                        ReturnType = "linkrefresh.png";
                        break;                    
                    default:
                        break;

                }
                return ReturnType;
            }

        }
    }
}
