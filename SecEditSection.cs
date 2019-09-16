using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using PolReader.DiffingClasses;


namespace PolReader
{
    [Serializable()]
    public class SecEditSection : ISerializable
    {
        public String Name { get; set; }
        public List<SecEditValuePair> Entries { get; set; }

        public SecEditSection(SerializationInfo info, StreamingContext ctxt)
        {
            this.Name = (string)info.GetValue("Name", typeof(string));
            this.Entries = (List<SecEditValuePair>)info.GetValue("Entries", typeof(List<SecEditValuePair>)); ;           
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Name", this.Name);
            info.AddValue("Entries", this.Entries);
        }

        public SecEditSection()
        {
            this.Name = "";
            Entries = new List<SecEditValuePair>();
        }

        public SecEditSection(string SectionName)
        {
            this.Name = SectionName;
            Entries = new List<SecEditValuePair>();
        }


        public List<SecEditDiffValueInfo> Compare(SecEditSection OldSecEditSection)
        {
            List<SecEditDiffValueInfo> returnValue = new List<SecEditDiffValueInfo>();

            foreach (SecEditValuePair secItem in Entries)
            {
                bool found = false;
                bool foundmatch = false;

                SecEditDiffValueInfo DiffItemMatch = new SecEditDiffValueInfo();

                foreach (SecEditValuePair oldsecItem in OldSecEditSection.Entries)
                {
                    if (secItem.Name.ToUpper().Trim().Equals(oldsecItem.Name.ToUpper().Trim()))
                    {
                        if (!secItem.Value.Trim().Equals(oldsecItem.Value.Trim()))
                        {
                            DiffItemMatch.Type = SecEditDiffValueInfo.UPDATED_POLICY_ITEM;
                            DiffItemMatch.OldItem = oldsecItem;
                            DiffItemMatch.NewItem = secItem;

                            found = true;
                        }
                        else
                        {
                            foundmatch = true;
                            found = true;
                            break;
                        }                        
                        
                    }
                }

                if (found && !foundmatch)
                {
                    returnValue.Add(DiffItemMatch);
                }
                else if (!found)
                {
                    SecEditDiffValueInfo DiffItem = new SecEditDiffValueInfo();
                    DiffItem.Type = SecEditDiffValueInfo.NEW_POLICY_ITEM;
                    DiffItem.NewItem = secItem;

                    returnValue.Add(DiffItem);
                }
            }

            foreach (SecEditValuePair oldsecItem in OldSecEditSection.Entries)
            {
                bool found = false;

                foreach (SecEditValuePair secItem in Entries)
                {
                    if (secItem.Name.ToUpper().Trim().Equals(oldsecItem.Name.ToUpper().Trim()))
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    SecEditDiffValueInfo DiffItem = new SecEditDiffValueInfo();
                    DiffItem.Type = SecEditDiffValueInfo.DELETED_POLICY_ITEM;
                    DiffItem.OldItem = oldsecItem;

                    returnValue.Add(DiffItem);
                }
            }

            return returnValue;

        }
    }
}
