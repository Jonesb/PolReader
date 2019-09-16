using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExcelXML
{
    public class Style
    {
        public enum HorizontalAlignmentType {Automatic, Left, Center, Right, Fill, Justify, CenterAcrossSelection, Distributed,JustifyDistributed};
        public enum VerticalAlignmentType { Automatic, Top, Bottom, Center, Justify, Distributed, JustifyDistributed};

        public HorizontalAlignmentType HorizontalAlignment;
        public VerticalAlignmentType VerticalAlignment;

        public bool ShrinkToFit;
        public bool WrapText;

        public bool BorderTop;
        public bool BorderBottom;
        public bool BorderLeft;
        public bool BorderRight;

        public enum BorderLineType { None = 0, Continuous, Dash, Dot, DashDot, DashDotDot, SlantDashDot, Double };
        public enum BorderWeightType { Hairline = 0, Thin, Medium, Thick };

        BorderWeightType BorderWeight;
        BorderLineType BorderLine;
        string BorderColor;
        string BGColor;

        public string FontColor;
        public string FontName;
        public int FontSize;
        public bool FontBold;
          
        public Style()
        {
            BorderColor="";
            BGColor = "";
            FontColor = "";
            FontName = "";
        }

        public Style(string bgColor)
        {            
            BorderColor="";
            BGColor = bgColor;
            FontColor = "";
            FontName = "";
        }

        public Style(HorizontalAlignmentType horizontalAlignment, VerticalAlignmentType verticalAlignment, string bgColor)
        {
            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;

            BorderColor = "";
            BGColor = bgColor;
            FontColor = "";
            FontName = "";
        }

        public Style(string bgColor,string borderColor,bool borderTop, bool borderBottom,bool borderLeft,bool borderRight)
        {
            BorderColor=borderColor;
            BGColor = bgColor;
            FontColor = "";
            FontName = "";

            BorderTop = borderTop;
            BorderBottom = borderBottom;
            BorderLeft = borderLeft;
            BorderRight = borderRight;

            BorderLine = BorderLineType.Continuous;
        }

        public Style(HorizontalAlignmentType horizontalAlignment, VerticalAlignmentType verticalAlignment, string bgColor, string borderColor, bool borderTop, bool borderBottom, bool borderLeft, bool borderRight)
        {
            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;

            BorderColor = borderColor;
            BGColor = bgColor;
            FontColor = "";
            FontName = "";

            BorderTop = borderTop;
            BorderBottom = borderBottom;
            BorderLeft = borderLeft;
            BorderRight = borderRight;

            BorderLine = BorderLineType.Continuous;
        }

        public Style(string bgColor, string borderColor, bool borderTop, bool borderBottom, bool borderLeft, bool borderRight, BorderWeightType borderWeight)
        {
            BorderColor = borderColor;
            BGColor = bgColor;
            FontColor = "";
            FontName = "";
            
            BorderTop = borderTop;
            BorderBottom = borderBottom;
            BorderLeft = borderLeft;
            BorderRight = borderRight;

            BorderLine = BorderLineType.Continuous;
            BorderWeight = borderWeight;
        }

        public Style(HorizontalAlignmentType horizontalAlignment, VerticalAlignmentType verticalAlignment, string bgColor, string borderColor, bool borderTop, bool borderBottom, bool borderLeft, bool borderRight, BorderWeightType borderWeight, bool shrinkToFit)
        {
            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;

            BorderColor = borderColor;
            BGColor = bgColor;
            FontColor = "";
            FontName = "";

            BorderTop = borderTop;
            BorderBottom = borderBottom;
            BorderLeft = borderLeft;
            BorderRight = borderRight;

            BorderLine = BorderLineType.Continuous;
            BorderWeight = borderWeight;

            ShrinkToFit = shrinkToFit;
        }

        public bool Equals(Style compareStyle)
        {            
            return (HorizontalAlignment == compareStyle.HorizontalAlignment &&
                    VerticalAlignment == compareStyle.VerticalAlignment &&
                        ShrinkToFit == compareStyle.ShrinkToFit &&
                            WrapText == compareStyle.WrapText &&
                BorderTop == compareStyle.BorderTop &&
                BorderBottom == compareStyle.BorderBottom &&
                BorderLeft == compareStyle.BorderLeft &&
                BorderRight == compareStyle.BorderRight &&
                BorderWeight == compareStyle.BorderWeight &&
                BorderLine == compareStyle.BorderLine &&
                BGColor.Equals(compareStyle.BGColor) &&
                BorderColor.Equals(compareStyle.BorderColor) &&
                FontColor.Equals(compareStyle.FontColor) &&
                FontName.Equals(compareStyle.FontName) &&
                FontSize == compareStyle.FontSize &&
                FontBold == compareStyle.FontBold);
        }

        

        public string OutPut()
        {
            String ReturnValue = "";

            ReturnValue += "<ss:Alignment ";

            if (HorizontalAlignment > 0)
                ReturnValue += "ss:Horizontal=\"" + HorizontalAlignment + "\" ";

            if (VerticalAlignment > 0)
                ReturnValue += "ss:Vertical=\"" + VerticalAlignment + "\" ";

            if(ShrinkToFit)
                ReturnValue += "ss:ShrinkToFit=\"1\" ";

            if (WrapText)
                ReturnValue +=  "ss:WrapText=\"1\" ";
           
            ReturnValue += "/>\n";

            ReturnValue += "<Borders>\n";
            
            if(BorderTop)
            {
                ReturnValue += "<Border ss:Position=\"Top\" " +
                                "ss:LineStyle=\"" + BorderLine + "\" " +
                                "ss:Weight=\"" + (int)BorderWeight + "\" ";
                
                if(BorderColor.Length>0)
                    ReturnValue += "ss:Color=\"" + BorderColor + "\" ";

                ReturnValue += "/>\n";
            }

            if(BorderBottom)
            {
                ReturnValue += "<Border ss:Position=\"Bottom\" " +
                                "ss:LineStyle=\"" + BorderLine + "\" " +
                                "ss:Weight=\"" + (int)BorderWeight + "\" ";
                
                if(BorderColor.Length>0)
                    ReturnValue += "ss:Color=\"" + BorderColor + "\" ";

                ReturnValue += "/>\n";
            }

            if(BorderLeft)
            {
                ReturnValue += "<Border ss:Position=\"Left\" " +
                                "ss:LineStyle=\"" + BorderLine + "\" " +
                                "ss:Weight=\"" + (int)BorderWeight + "\" ";
                
                if(BorderColor.Length>0)
                    ReturnValue += "ss:Color=\"" + BorderColor + "\" ";
                
                ReturnValue += " />";
            }

            if(BorderRight)
            {
                ReturnValue += "<Border ss:Position=\"Right\" " +
                                "ss:LineStyle=\"" + BorderLine + "\" " +
                                "ss:Weight=\"" + (int)BorderWeight + "\" ";
                
                if(BorderColor.Length>0)
                    ReturnValue += "ss:Color=\"" + BorderColor + "\" ";

                ReturnValue += "/>\n";
            }

            ReturnValue += "</Borders>\n";

            if(BGColor.Length>0)
            {
                ReturnValue += "<ss:Interior ss:Color=\"" + BGColor + "\" ss:Pattern=\"Solid\" />";
            }

            ReturnValue += "<Font ";

            if(FontColor.Length >0)
                ReturnValue +=  "ss:Color=\"" + FontColor + "\"";

            if(FontName.Length >0)
                ReturnValue += " ss:FontName=\"" + FontName + "\"";

            if(FontSize>0)
                ReturnValue += " ss:Size=\"" + FontSize + "\"";

            if(FontBold)
                ReturnValue += " ss:Bold=\"1\"";


            ReturnValue += " />\n";
            

            return ReturnValue;
        }

    }
}
