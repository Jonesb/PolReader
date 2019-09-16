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

namespace PolReader
{
    class LinkageDisplayConverter  : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, System.Globalization.CultureInfo culture)
        {
            
            try
            {
                string ReturnValue = "N/A";

                if ((int)value > -1)
                {
                    ReturnValue = value.ToString();
                }

                return ReturnValue;

            }
            catch
            {

                return "N/A";
            }
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

