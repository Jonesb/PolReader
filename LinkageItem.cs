using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PolReader
{
    [Serializable()]
    public class LinkageItem : ISerializable
    {
        public Policy ParentPolicy { get; set; }
        public String Linkage { get; set; }

        public LinkageItem()
        {

        }

        public LinkageItem(SerializationInfo info, StreamingContext ctxt)
        {
            this.ParentPolicy = (Policy)info.GetValue("ParentPolicy", typeof(Policy));
            this.Linkage = (String)info.GetValue("Linkage", typeof(String));                      

        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("ParentPolicy", this.ParentPolicy);
            info.AddValue("Linkage", this.Linkage);            
            
        }
    }
}
