using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExcelXML
{
    public class WorkSheet
    {
        public String Name { get; set; }
        public List<Row> Rows { get; set; }
        public List<Column> Columns { get; set; }

        public delegate void LogUpdate(int Total, int Current, string Details);
        public event LogUpdate LogUpdateMessage;

        private void LogProgress(int Count, int Current, string Details)
        {
            if (LogUpdateMessage != null)
            {
                LogUpdateMessage(Count, Current, Details);
            }
        }

        public WorkSheet(string name)
        {
            Rows = new List<Row>();
            Columns = new List<Column>();
            Name = name;
        }

        public string OutPut()
        {
            String ReturnValue = "";

            ReturnValue = "<Worksheet ss:Name=\"" + Cell.ConvertXMLString(Name) + "\">\n";
            ReturnValue += "<Table>\n";

            int c = 0;
            foreach (Column column in Columns)
            {
                if (column.AutoFitWidth)
                {
                    foreach (Row row in Rows)
                    {
                        if (row.Cells.Count > c)
                        {
                            if ((row.Cells.ElementAt(c).Data.ToString().Length * 15) > column.Width)
                            {
                                column.Width = row.Cells.ElementAt(c).Data.ToString().Length * 12;
                            }
                        }
                    }
                }

                ReturnValue += column.OutPut();
                c++;
            }

            int count = Rows.Count;
            int current = 0;

            foreach (Row row in Rows)
	        {
                current++;
                LogUpdateMessage(count, current, Name);
		        ReturnValue += row.OutPut();
	        }

            ReturnValue += "</Table>\n";            
            ReturnValue += "</Worksheet>\n";
            return ReturnValue;
        }




    }
}
