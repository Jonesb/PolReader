using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Excel = Microsoft.Office.Interop.Excel; 

namespace PolReader
{
    class PreferenceReport
    {
        public List<RootNodePreferenceReportHolder> Items;

        public PreferenceReport()
        {
            Items = new List<RootNodePreferenceReportHolder>();
        }

        public void AddRootNode(PreferenceReportItem pri)
        {
            if(pri.ClsidProperty.Equals("{9CD4B2F4-923D-47f5-A062-E897DD1DAD50}")|| //Regitry Item
                                    pri.ClsidProperty.Equals("{78570023-8373-4a19-BA80-2F150738EA19}")||  //EnvironmentVariable
                                    pri.ClsidProperty.Equals("{2B130A62-fc14-4572-91C3-5435C6A0C3FC}"))  //Power Option
            {
                if (!Exists(pri))
                {
                    RootNodePreferenceReportHolder rnprh = new RootNodePreferenceReportHolder();

                    rnprh.CLSID = pri.ClsidProperty;
                    rnprh.ID = pri.ID;
                    rnprh.Name = pri.Name;
                    rnprh.NameProperty = pri.NameProperty;
                    rnprh.Type = pri.Type;
                    rnprh.XMLNodeID = pri.XMLNodeID;

                    Items.Add(rnprh);
                }
            }
            
        }

        public bool Exists(PreferenceReportItem pri)
        {
            bool found = false;

            foreach (RootNodePreferenceReportHolder item in Items)
            {
                if (item.ID == pri.ID)
                {
                    found = true;
                    break;
                }
            }

            return found;
        }


        public void AddToParent(PreferenceReportItem pri)
        {
            if (pri.Name == "Properties")
            {
                foreach (RootNodePreferenceReportHolder item in Items)
	            {
                    if (item.XMLNodeID == pri.ParentID &&
                        item.Type == pri.Type)
                    {
                        item.Properties.Add(pri);
                        if(pri.PropName.Equals("NameGuid",StringComparison.CurrentCultureIgnoreCase)
                            || pri.PropName.Equals("Name", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (item.AddedBy.Length == 0)
                            {
                                item.AddedBy = pri.UserName;
                                item.Comment = pri.Comments;
                            }

                        }

                        break;
                    }

	            }
            }
        }

        public System.Windows.Point OutPutToWorkSheet(ExcelXML.WorkSheet xlWorkSheet)
        {
            System.Windows.Point returnValue = new System.Windows.Point(0,0);

            ExcelXML.Style HeaderStyle = new ExcelXML.Style(ExcelXML.Style.HorizontalAlignmentType.Center,
                                                                    ExcelXML.Style.VerticalAlignmentType.Center, 
                                                                    "#636594",
                                                                    "#7F7F7F", 
                                                                    true, true, false, false,
                                                                    ExcelXML.Style.BorderWeightType.Medium, 
                                                                    true);

            ExcelXML.Style HeaderStyleNoShrink = new ExcelXML.Style(ExcelXML.Style.HorizontalAlignmentType.Center,
                                                                    ExcelXML.Style.VerticalAlignmentType.Center,
                                                                    "#636594",
                                                                    "#7F7F7F",
                                                                    true, true, false, false,
                                                                    ExcelXML.Style.BorderWeightType.Medium,
                                                                    false);

            HeaderStyle.FontColor = "#FFFFFF";
            HeaderStyle.FontBold = true;

            HeaderStyleNoShrink.FontColor = "#FFFFFF";
            HeaderStyleNoShrink.FontBold = true;
            HeaderStyleNoShrink.WrapText = true;


            ExcelXML.Style PropOddStyle = new ExcelXML.Style(ExcelXML.Style.HorizontalAlignmentType.Left,
                                                           ExcelXML.Style.VerticalAlignmentType.Center,
                                                           "#F9F9F9",
                                                           "#7F7F7F",
                                                           true, true, true, true,
                                                           ExcelXML.Style.BorderWeightType.Thin,
                                                           true);

            ExcelXML.Style PropEvenStyle = new ExcelXML.Style(ExcelXML.Style.HorizontalAlignmentType.Left,
                                                           ExcelXML.Style.VerticalAlignmentType.Center,
                                                           "#C0D2DE",
                                                           "#7F7F7F",
                                                           false, false, true, true,
                                                           ExcelXML.Style.BorderWeightType.Thin,
                                                           true);

            ExcelXML.Style PropOddStyleNoWrap = new ExcelXML.Style(ExcelXML.Style.HorizontalAlignmentType.Left,
                                                          ExcelXML.Style.VerticalAlignmentType.Center,
                                                          "#F9F9F9",
                                                          "#7F7F7F",
                                                          true, true, true, true,
                                                          ExcelXML.Style.BorderWeightType.Thin,
                                                          false);

            ExcelXML.Style PropEvenStyleNoWrap = new ExcelXML.Style(ExcelXML.Style.HorizontalAlignmentType.Left,
                                                           ExcelXML.Style.VerticalAlignmentType.Center,
                                                           "#C0D2DE",
                                                           "#7F7F7F",
                                                           false, false, true, true,
                                                           ExcelXML.Style.BorderWeightType.Thin,
                                                           false);

            ExcelXML.Style blankStyle = new ExcelXML.Style("#939393");


            for (int x = 0; x < 8; x++)
            {
                ExcelXML.Column column = new ExcelXML.Column();
                column.AutoFitWidth = true;                
                xlWorkSheet.Columns.Add(column);
            }
            

            foreach (RootNodePreferenceReportHolder item in Items)
            {
                if (item.Properties.Count > 0)
                {
                                       
                    returnValue.Y++;
                    returnValue.X = 7;

                    double StartRow = returnValue.Y;
                    

                    ExcelXML.Row HeaderRow = new ExcelXML.Row(HeaderStyle);

                    HeaderRow.Height = 40;
                    HeaderRow.AutoFitHeight = true;
                    
                    HeaderRow.AddCell(item.Type == 0 ? "User" : "Machine");                    
                    HeaderRow.AddCell(item.Name);
                    HeaderRow.AddCell(item.NameProperty);                
                    HeaderRow.AddCell(item.AddedBy);
                    HeaderRow.AddCell(item.Comment, HeaderStyleNoShrink);

                    xlWorkSheet.Rows.Add(HeaderRow);

                    int c = 0;
                    foreach (PreferenceReportItem prop in item.Properties)
                    {
                        ExcelXML.Row DataRow = new ExcelXML.Row(blankStyle);

                        returnValue.Y++;
                        
                        DataRow.AddCell("", blankStyle);

                        DataRow.AddCell(prop.PropName, ((c & 1) == 1 ? PropEvenStyle : PropOddStyle));
                        DataRow.AddCell(prop.PropValue, ((c & 1) == 1 ? PropEvenStyle : PropOddStyle));

                        if (!item.Comment.Equals(prop.Comments))
                        {
                            DataRow.AddCell(prop.UserName, ((c & 1) == 1 ? PropEvenStyle : PropOddStyle));
                            DataRow.AddCell(prop.Comments, ((c & 1) == 1 ? PropEvenStyleNoWrap : PropOddStyleNoWrap));
                        }
                        

                        xlWorkSheet.Rows.Add(DataRow);
                        c++;
                    }
                    

                    //range = xlWorkSheet.get_Range(xlWorkSheet.Cells[StartRow, 1], xlWorkSheet.Cells[StartRow, 7]);
                    //range.Interior.ColorIndex = 36;                     
                    //range.BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlHairline, Excel.XlColorIndex.xlColorIndexAutomatic, 1);

                    //range.EntireRow.Font.Bold = true;  
                    //releaseObject(range);
                    
                }

            }

            return returnValue;
        }

       
    }
}
