using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExcelXML
{
    public class Row
    {
        public List<Cell> Cells { get; set; }
        public int StyleID;
        public int Height;
        public bool AutoFitHeight;

        public Row()
        {
            Cells = new List<Cell>();
            StyleID = -1;
            Height = -1;
        }

        public Row(Style styleItem)
        {
            Cells = new List<Cell>();
            StyleID = Styles.AddStyle(styleItem);
            Height = -1;
        }


        public string OutPut()
        {
            String ReturnValue = "";

            ReturnValue = "<Row";

            if (StyleID > -1)
                ReturnValue += " ss:StyleID=\"sid" + StyleID.ToString() + "\" ";

            if (Height > -1)
                ReturnValue += " ss:Height=\"" + Height + "\"";

            if(AutoFitHeight)
                ReturnValue += " ss:AutoFitHeight=\"1\"";

            ReturnValue +=  ">\n" ;

            foreach (Cell cell in Cells)
	        {
		        ReturnValue += cell.OutPut();
	        }

            ReturnValue += "</Row>\n";

            return ReturnValue;
        }

        public void AddCell(object Data)
        {
            Cell cell = new Cell();
            cell.Data = Data;
            Cells.Add(cell);
        }

        public void AddCell(object Data, Style cellStyle)
        {
            Cell cell = new Cell(cellStyle);
            cell.Data = Data;
            Cells.Add(cell);
        }
    }
}
