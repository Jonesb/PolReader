using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExcelXML
{
    public class Styles
    {
        static public List<Style> styles { get; set; }

        static Styles()
        {
            styles = new List<Style>();
        }

        static public int AddStyle(Style NewStyle)
        {
            int ReturnValue = -1;

            for(int c=0;c< styles.Count;c++)
            {
                if(NewStyle.Equals((Style)styles.ElementAt(c)))
                {
                    ReturnValue = c;
                    break;                    
                }
            }

            if (ReturnValue == -1)
            {
                styles.Add(NewStyle);
                ReturnValue = styles.IndexOf(NewStyle);
            }

            return ReturnValue;
        }

        static public string OutPut()
        {
            String ReturnValue = "";

            ReturnValue = "<Styles>\n";

            for(int c=0;c< styles.Count;c++)
            {                
                ReturnValue += "<Style ss:ID=\"sid" + c +  "\">\n";
                ReturnValue += styles.ElementAt(c).OutPut();
                ReturnValue += "</Style>\n";
            }                
            
            ReturnValue += "</Styles>\n";
            return ReturnValue;
        }
    }
}
