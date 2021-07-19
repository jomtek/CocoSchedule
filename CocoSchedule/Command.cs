using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CocoSchedule
{
    public static class Command
    {
        public static RoutedCommand TableZoomPlusCmd = new RoutedCommand();
        public static RoutedCommand TableZoomMinusCmd = new RoutedCommand();
    }
}
