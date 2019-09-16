using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ExcelXML
{
    public class WorkBook
    {
        public List<WorkSheet> WorkSheets { get; set; }

        public WorkBook()
        {
            WorkSheets = new List<WorkSheet>();
            UpdateProgViewMethodInstance = new UpdateProgViewDelegate(UpdateProgView);
        }

        public delegate void LogUpdate(int Total, int Current, string Details);
        public event LogUpdate LogUpdateMessage;

        public void UpdateProgView(int Total, int Current, string Detail)
        {
            if (LogUpdateMessage != null)
            {
                LogUpdateMessage(Total, Current, Detail);
            }
        }

        public delegate void UpdateProgViewDelegate(int Total, int Count, string Message);
        public UpdateProgViewDelegate UpdateProgViewMethodInstance;

        public void Save(string FileName)
        {
                TextWriter textWriter = File.CreateText(FileName);
                
                textWriter.WriteLine("<?xml version=\"1.0\"?>\n");
                textWriter.WriteLine("<?mso-application progid=\"Excel.Sheet\"?>\n");
                textWriter.WriteLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"\n");
                textWriter.WriteLine("xmlns:o=\"urn:schemas-microsoft-com:office:office\"\n");
                textWriter.WriteLine("xmlns:x=\"urn:schemas-microsoft-com:office:excel\"\n");
                textWriter.WriteLine("xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"\n");
                textWriter.WriteLine("xmlns:html=\"http://www.w3.org/TR/REC-html40\">\n");

                textWriter.Write(Styles.OutPut());

                foreach (WorkSheet sheet in WorkSheets)
                {
                    sheet.LogUpdateMessage += new WorkSheet.LogUpdate(UpdateProgView);
                    textWriter.Write(sheet.OutPut());                    
                }                

                textWriter.WriteLine("</Workbook>\n");

                textWriter.Close();
                  
        }
    }
}
