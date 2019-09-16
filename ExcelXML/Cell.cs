using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExcelXML
{
    public class Cell
    {
        public object Data;
        public int StyleID;

        public Cell()
        {
            StyleID = -1;
        }

        public Cell(Style styleItem)
        {
            StyleID = Styles.AddStyle(styleItem);            
        }

        public string OutPut()
        {
            String ReturnValue = "";

            ReturnValue = "<Cell";

            if (StyleID > -1)
                ReturnValue += " ss:StyleID=\"sid" + StyleID.ToString() + "\" ";

            ReturnValue += ">\n";

            ReturnValue += "<Data ss:Type=\"" + GetDataType() + "\">" + ConvertXMLString(Data.ToString()) + "</Data>\n";

            ReturnValue += "</Cell>\n";

            return ReturnValue;
        }

        public static string ConvertXMLString(string Data)
        {
            return Data.Replace("&", "&amp;").Replace("<", "&lt;");
        }

        public string GetDataType()
        {
            string ReturnValue;


            if (IsNumber(Data))
            {
                ReturnValue = "Number";
            }
            else if (IsDate(Data))
            {
                ReturnValue = "DateTime";
            }
            else
            {
                ReturnValue = "String";
            }

            return ReturnValue;

        }

        private bool IsNumber(object Data)
        {
            bool ReturnValue = false;

            try
            {
                if (Int32.Parse(Data.ToString()).ToString().Equals(Data.ToString()))
                {
                    ReturnValue = true;
                }
            }
            catch (Exception e)
            {                
            }

            return ReturnValue;
        }

        private bool IsDate(object Data)
        {
            bool ReturnValue = false;

            try
            {
                if (DateTime.Parse(Data.ToString()).ToString().Equals(Data.ToString()))
                {
                    ReturnValue = true;
                }
            }
            catch (Exception e)
            {
            }

            return ReturnValue;
        }
    }
}
