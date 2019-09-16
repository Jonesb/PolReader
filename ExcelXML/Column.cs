using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExcelXML
{
    public class Column
    {        
        public int StyleID;
        public string Caption;
        public int Width;
        public bool AutoFitWidth;
        
        public Column()
        {            
            StyleID = -1;
            Caption = "";
            Width = -1;
        }

        public Column(Style styleItem)
        {            
            StyleID = Styles.AddStyle(styleItem);
            Caption = "";
            Width = -1;
        }


        public string OutPut()
        {
            String ReturnValue = "";

            ReturnValue = "<Column";

            if (StyleID > -1)
                ReturnValue += " ss:StyleID=\"sid" + StyleID.ToString() + "\" ";

            if(AutoFitWidth)
                ReturnValue += " ss:AutoFitWidth=\"1\"";

            if (Width > -1)
                ReturnValue += " ss:Width=\"" + Math.Min(255,Width) + "\"";

            if(Caption.Length>0)
                ReturnValue += " ss:Caption=\"" + Caption + "\"";


            ReturnValue += " />\n";

            return ReturnValue;
        }
    }
}
