using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PolReader.DiffingClasses
{
    [Serializable()]
    public class AssignmentsDiffInfo : ISerializable
    {

        public const int UNCHANGED_POLICY_ITEM = 0;
        public const int UPDATED_POLICY_ITEM = 1;
        
        public int Type { get; set; }
        
        public List<AssignmentsDiffInfoItem> Assignmments { get; set; }

        public string Name
        {
            get
            {
                return "Assignments";
            }
        }

        public AssignmentsDiffInfo()
        {
            Assignmments = new List<AssignmentsDiffInfoItem>();
        }

        public BaseDiffViewItem GetViewItems(BaseDiffViewItem parent)
        {

            BaseDiffViewItem Item = new BaseDiffViewItem(parent);

            Item.IconString = IconString;
            Item.Name = Name;

            foreach (AssignmentsDiffInfoItem assignedItem in Assignmments)
            {
                Item.Children.Add(assignedItem.GetViewItems(Item));
            }

            return Item;
        }

        public AssignmentsDiffInfo(SerializationInfo info, StreamingContext ctxt)
        {
            this.Type = (int)info.GetValue("Type", typeof(int));
            this.Assignmments = (List<AssignmentsDiffInfoItem>)info.GetValue("Assignmments", typeof(List<AssignmentsDiffInfoItem>));            
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Type", this.Type);
            info.AddValue("Assignmments", this.Assignmments);
        }

        public String IconString
        {
            get
            {
                string ReturnType = "";

                switch (Type)
                {
                    case UNCHANGED_POLICY_ITEM:
                        ReturnType = "users.png";
                        break;
                    case UPDATED_POLICY_ITEM:
                        ReturnType = "usersrefresh.png";
                        break;
                    default:
                        break;

                }

                return ReturnType;
            }

        }
    }
}
