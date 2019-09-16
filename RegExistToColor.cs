using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Media;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace PolReader
{
    class RegExistToColor : IValueConverter
    {
        public object Convert(object value, Type targetType,
                            object parameter, System.Globalization.CultureInfo culture)
        {

            try
            {

                System.Windows.Media.Color color = System.Windows.Media.Colors.Red;
                
                int Value = (int) value;

                if (Value == PolicyItem.ITEM_NOT_APPLICABLE)
                {
                    color = System.Windows.Media.Colors.Black;
                }
                else if (Value == PolicyItem.ITEM_EXISTS_SAME)
                {
                    color = System.Windows.Media.Colors.Green;
                }
                else if (Value == PolicyItem.ITEM_EXISTS_TYPE_DIFFERS)
                {                
                    color = System.Windows.Media.Colors.Yellow;
                }
                else if (Value == PolicyItem.ITEM_EXISTS_VALUE_DIFFERS)
                {
                    color = System.Windows.Media.Colors.PaleVioletRed;
                }                                

                return color;

            }
            catch
            {

                return System.Windows.Media.Colors.Red;
            }
        }

       

        public object ConvertBack(object value, Type targetType,
                                  object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
