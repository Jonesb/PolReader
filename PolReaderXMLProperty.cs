using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PolReader
{
    [Serializable()]
    public class PolReaderXMLProperty : ISerializable
    {
        public String Name { get; set; }
        public String Value { get; set; }
        
        public int DBID { get; set; }

        public PolReaderXMLProperty()
        {
            Name = "";
            Value = "";
        }

        public PolReaderXMLProperty(SerializationInfo info, StreamingContext ctxt)
        {
            this.Name = (string)info.GetValue("Name", typeof(string));
            this.Value = (string)info.GetValue("Value", typeof(string));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Name", this.Name);
            info.AddValue("Value", this.Value);
        }


    }
}
