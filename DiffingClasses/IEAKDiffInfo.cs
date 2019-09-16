using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PolReader.DiffingClasses
{
    [Serializable()]
    public class IEAKDiffInfo:SecEditDiffInfo
    {

        public bool MachineSetting { get; set; }

        public IEAKDiffInfo(bool machineSetting)
        {
            Sections = new List<SecEditDiffSectionInfo>();

            MachineSetting = machineSetting;
        }

        public override String Name
        {
            get
            {
                if (MachineSetting)
                    return "Machine IEAK";
                else
                    return "User IEAK";
            }
        }


        public IEAKDiffInfo(SerializationInfo info, StreamingContext ctxt)
        {
            this.Type = (int)info.GetValue("Type", typeof(int));
            this.Sections = (List<SecEditDiffSectionInfo>)info.GetValue("Sections", typeof(List<SecEditDiffSectionInfo>));
            this.MachineSetting = (bool)info.GetValue("MachineSetting", typeof(bool));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Type", this.Type);            
            info.AddValue("Sections", this.Sections);
            info.AddValue("MachineSetting", this.MachineSetting);
        }

        public override String IconString
        {
            get
            {
                string ReturnType = "";

                switch (Type)
                {
                    case NEW_POLICY_ITEM:
                        ReturnType = "IEAKadd.png";
                        break;
                    case DELETED_POLICY_ITEM:
                        ReturnType = "IEAKdelete.png";
                        break;
                    case UPDATED_POLICY_ITEM:
                        ReturnType = "IEAKrefresh.png";
                        break;
                    case UNCHANGED_POLICY_ITEM:
                        ReturnType = "IEAK.png";
                        break;
                    default:
                        break;

                }
                return ReturnType;
            }

        }
    }
}
