using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PolReader.DiffingClasses
{
    [Serializable()]
    public class XmlNodeDiffInfo : ISerializable
    {

        public const int UNCHANGED_POLICY = 0;
        public const int NEW_POLICY = 1;
        public const int DELETED_POLICY = 2;
        public const int UPDATED_POLICY = 3;

        //public int PolicyType { get; set; }
        public int Type { get; set; }
        
        //Used when get stuff from the DB
        public int DBID { get; set; }

        public List<XmlNodeDiffInfo> nodeItems { get; set; }
        public List<XmlPropertyDiffInfo> propItems { get; set; }

        public string Name { get; set; }

        public XmlNodeDiffInfo()
        {
            Type = UNCHANGED_POLICY;
           // PolicyType = 0;
            Name = "";

            nodeItems = new List<XmlNodeDiffInfo>();
            propItems = new List<XmlPropertyDiffInfo>();
        }

        public XmlNodeDiffInfo(SerializationInfo info, StreamingContext ctxt)
        {
            this.Type = (int)info.GetValue("Type", typeof(int));
            //this.PolicyType = (int)info.GetValue("PolicyType", typeof(int));
            this.Name = (string)info.GetValue("Name", typeof(string));

            this.nodeItems = (List<XmlNodeDiffInfo>)info.GetValue("nodeItems", typeof(List<XmlNodeDiffInfo>));
            this.propItems = (List<XmlPropertyDiffInfo>)info.GetValue("propItems", typeof(List<XmlPropertyDiffInfo>));

        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            //info.AddValue("PolicyType", this.PolicyType);
            info.AddValue("Type", this.Type);
            info.AddValue("Name", this.Name);

            info.AddValue("nodeItems", this.nodeItems);
            info.AddValue("propItems", this.propItems); 
        }
       

        public BaseDiffViewItem GetViewItems(BaseDiffViewItem parent)
        {

            BaseDiffViewItem Item = new BaseDiffViewItem(parent);

            Item.IconString = IconString;

            Item.Name = Name;

            foreach (XmlPropertyDiffInfo propInfo in propItems)
            {
                Item.Children.Add(propInfo.GetViewItems(Item));
            }

            foreach (XmlNodeDiffInfo nodeItem in nodeItems)
            {
                Item.Children.Add(nodeItem.GetViewItems(Item));
            }

            return Item;
        }

        public String IconString
        {
            get
            {
                string ReturnType = "";

                switch (Type)
                {
                    case UNCHANGED_POLICY:
                        ReturnType = "node.png";
                        break;
                    case NEW_POLICY:
                        ReturnType = "nodeAdd.png";
                        break;
                    case DELETED_POLICY:
                        ReturnType = "nodeDelete.png";
                        break;
                    case UPDATED_POLICY:
                        ReturnType = "nodeRefresh.png";
                        break;
                    default:
                        break;

                }

                return ReturnType;
            }

        }
    }
}
