using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PolReader.DiffingClasses
{
    [Serializable()]
    public class SecEditDiffSectionInfo : ISerializable
    {

        public const int NEW_POLICY_ITEM = 1;
        public const int DELETED_POLICY_ITEM = 2;
        public const int UPDATED_POLICY_ITEM = 3;

        public int Type { get; set; }
        public String Name { get; set; }

        public List<SecEditDiffValueInfo> Values { get; set; }

        public SecEditDiffSectionInfo()
        {
            Values = new List<SecEditDiffValueInfo>();
        }

        public BaseDiffViewItem GetViewItems(BaseDiffViewItem parent)
        {

            BaseDiffViewItem Item = new BaseDiffViewItem(parent);

            Item.IconString = IconString;
            Item.Name = Name;

            foreach (SecEditDiffValueInfo secItem in Values)
            {
                Item.Children.Add(secItem.GetViewItems(Item));
            
            }

            return Item;
        }

        public SecEditDiffSectionInfo(SerializationInfo info, StreamingContext ctxt)
        {
            this.Type = (int)info.GetValue("Type", typeof(int));
            this.Name = (string)info.GetValue("Name", typeof(string));
            this.Values = (List<SecEditDiffValueInfo>)info.GetValue("Values", typeof(List<SecEditDiffValueInfo>));
            
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Type", this.Type);
            info.AddValue("Name", this.Name);
            info.AddValue("Values", this.Values);
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
    }
}
