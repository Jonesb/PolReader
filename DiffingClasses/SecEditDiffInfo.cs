using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PolReader.DiffingClasses
{
    [Serializable()]
    public class SecEditDiffInfo : ISerializable
    {

        public const int UNCHANGED_POLICY_ITEM = 0;
        public const int NEW_POLICY_ITEM = 1;
        public const int DELETED_POLICY_ITEM = 2;
        public const int UPDATED_POLICY_ITEM = 3;
        
        public int Type { get; set; }

        public virtual  String Name
        {
            get
            {
                return "Secedit File";
            }
        }


        public List<SecEditDiffSectionInfo> Sections { get; set; }

        public BaseDiffViewItem GetViewItems(BaseDiffViewItem parent)
        {

            BaseDiffViewItem Item = new BaseDiffViewItem(parent);

            Item.IconString = IconString;
            Item.Name = Name;

            foreach (SecEditDiffSectionInfo section in Sections)
            {
                Item.Children.Add(section.GetViewItems(Item));
            }

            return Item;
        }

        public SecEditDiffInfo()
        {
            Sections = new List<SecEditDiffSectionInfo>();            
        }

        public SecEditDiffInfo(SerializationInfo info, StreamingContext ctxt)
        {
            this.Type = (int)info.GetValue("Type", typeof(int));
            this.Sections = (List<SecEditDiffSectionInfo>)info.GetValue("Sections", typeof(List<SecEditDiffSectionInfo>)); 
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Type", this.Type);            
            info.AddValue("Sections", this.Sections);
        }

        public virtual  String IconString
        {
            get
            {
                string ReturnType = "";

                switch (Type)
                {
                    case NEW_POLICY_ITEM:
                        ReturnType = "SeceditAdd.png";
                        break;
                    case DELETED_POLICY_ITEM:
                        ReturnType = "SeceditDelete.png";
                        break;
                    case UPDATED_POLICY_ITEM:
                        ReturnType = "SeceditRefresh.png";
                        break;
                    case UNCHANGED_POLICY_ITEM:
                        ReturnType = "Secedit.png";
                        break;
                    default:
                        break;

                }
                return ReturnType;
            }

        }
    }
}
