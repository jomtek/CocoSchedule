using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CocoSchedule.Converters
{
    public class HeightToDescFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var height = (double)value;

            if (height < 25)
            {
                return 0;
            }

            /*
            double candidateFontSize = height / 8d;

            if (candidateFontSize < 22.5)
            {
                return 22.5;
            }
            else if (candidateFontSize > 22.5)
            {
                return 22.5;
            }
            else
            {
                return candidateFontSize;
            }
            */
            return 32;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
