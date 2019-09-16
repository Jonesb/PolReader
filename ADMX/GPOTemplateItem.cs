using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ADMX
{
    public class GPOTemplateItem
    {
        public const int Machine = 0;
        public const int User = 1;

        public const int NoType = 0;
        public const int BooleanType = 1;
        public const int EnumType = 2;
        public const int TextType = 3;
        public const int DecimalType = 4;
        public const int ListType = 5;

        public string Key;
        public string ValueName;        
        public string DisplayName;
        public string Explaination;
        public int Type;
        public int ElementType;
        public string ElementString;
        public string ListGUID;
        public string EnumKey;

        public string CategoryName;

        public List<EnumValue> enumValues = new List<EnumValue>();

        public GPOTemplateItem()
        {
            Key = "";
            ValueName = "";     
            DisplayName = "";
            Explaination = "";
            ElementType = NoType;
            ElementString = "";
            CategoryName = "";
            ListGUID = "";
            EnumKey = "";
        }

        public String DataText(string Data)
        {
            string returnValue = Data;

            if (ElementType == EnumType || ElementType == ListType)
            {
                foreach (EnumValue enumVal in enumValues)
                {
                    if (enumVal.Value.Equals(Data))
                    {
                        returnValue = enumVal.DisplayName;
                        break;
                    }
                }
            }

            return returnValue;

        }

        public String Text
        {
            get
            {
                string ReturnValue;

                if(ElementType == ListType)
                {
                    ReturnValue = ElementString;
                }
                else if (ElementString.Length > 0 && (ElementType == BooleanType || ElementType == EnumType || ElementType == TextType || ElementType == DecimalType ))
                {
                    ReturnValue = ElementString;
                }
                else
                {
                    ReturnValue = ValueName;
                }

                return ReturnValue;
            }
        } 

    }
}
