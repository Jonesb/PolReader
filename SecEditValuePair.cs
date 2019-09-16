using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace PolReader
{
    [Serializable()]
    public class SecEditValuePair : ISerializable
    {
        public String Name { get; set; }
        public String Value { get; set; }

        public SecEditValuePair()
        {
            Name = "";
            Value = "";
        }

        public SecEditValuePair(SerializationInfo info, StreamingContext ctxt)
        {
            this.Name = (string)info.GetValue("Name", typeof(string));
            this.Value = (string)info.GetValue("Value", typeof(string));           
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Name", this.Name);
            info.AddValue("Value", this.Value);
        }

        public SecEditValuePair(string WholeLineEntry)
        {
            if (WholeLineEntry.Contains("="))
            {
                int FirstEquals = WholeLineEntry.IndexOf("=");

                Name = WholeLineEntry.Substring(0, FirstEquals);
                Value = WholeLineEntry.Substring(FirstEquals + 1);

            }
            else if (WholeLineEntry.Contains(","))
            {
                int FirstEquals = WholeLineEntry.IndexOf(",");

                Name = WholeLineEntry.Substring(0, FirstEquals);
                Value = WholeLineEntry.Substring(FirstEquals + 1);
            }
            else
            {
                Name = "";
                Value = "";
            }
        }
               
          
    }
}
