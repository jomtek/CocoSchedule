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
    public class HeightToTitleFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var height = (double)value;
            if (height < 25)
            {
                return System.Convert.ToInt32(height / 1.4d);
            }

            return 20;
            //Console.WriteLine("yes");
            //return System.Convert.ToInt32(Utils.TimespanToHeight(new TimeSpan(2, 0, 0)) / 6d); // experiment
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
