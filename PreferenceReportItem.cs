using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolReader
{
    class PreferenceReportItem
    {
         public int ID;
         public int XMLNodeID;
         public int Type;
         public int ParentID;         
         public string Name;
         public string NameProperty;         
         public string ClsidProperty;
         public string PropValue;
         public string PropName;

         public string UserName;
         public string Comments;

         public PreferenceReportItem()
         {
             Name = "";   
             NameProperty = "";             
             ClsidProperty = "";
             PropValue = "";
             PropName = "";
             UserName = "";
             Comments = "";
         }
    }
}
