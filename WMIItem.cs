using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PolReader
{
    [Serializable()]
    public class WMIItem : ISerializable
    {
        public Policy ParentPolicy { get; set; }
        public String WMIFilter { get; set; }

        public WMIItem()
        {

        }

        public WMIItem(SerializationInfo info, StreamingContext ctxt)
        {
            this.ParentPolicy = (Policy)info.GetValue("ParentPolicy", typeof(Policy));
            this.WMIFilter = (String)info.GetValue("WMIFilter", typeof(String));                      

        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("ParentPolicy", this.ParentPolicy);
            info.AddValue("WMIFilter", this.WMIFilter);            
            
        }
    }
}
