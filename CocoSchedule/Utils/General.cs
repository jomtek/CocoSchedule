using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CocoSchedule.Utils
{
    public static class General
    {
        #region Time
        public static double TimespanToHeight(TimeSpan ts)
        {
            return (ts.TotalSeconds * GlobalInfo.HourHeight) / 3600d;
        }

        public static TimeSpan HeightToTimespan(double height)
        {
            return TimeSpan.FromSeconds((height * 3600) / GlobalInfo.HourHeight);
        }

        public static TimeSpan RoundTimespanToNearest(TimeSpan ts, TimeSpan d)
        {
            var delta = ts.Ticks % d.Ticks;
            bool roundUp = delta > d.Ticks / 2;
            var offset = roundUp ? d.Ticks : 0;

            return TimeSpan.FromTicks(ts.Ticks + offset - delta);
        }

        // Credits: Jon Skeet
        public static bool IsTimespanInRange(TimeSpan candidate, TimeSpan start, TimeSpan end)
        {
            if (start < end) 
            {
                // Normal case, e.g. 8am-2pm
                return start <= candidate && candidate < end;
            }
            else
            {
                // TODO: Is this case necessary ?
                // Reverse case, e.g. 10pm-2am
                return start <= candidate || candidate < end;
            }
        }

        public static bool CheckTimespanOverlap(TimeSpan start1, TimeSpan end1, TimeSpan start2, TimeSpan end2)
        {
            return start1 < end2 && start2 < end1;
        }

        public static DateTime GetActualDay()
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        }
        #endregion

        #region Graphics
        public static bool DetectCollisions(Visual reference, FrameworkElement elem1, FrameworkElement elem2)
        {
            GeneralTransform t1 = elem1.TransformToVisual(reference);
            GeneralTransform t2 = elem2.TransformToVisual(reference);
            Rect r1 = t1.TransformBounds(new Rect() { X = 0, Y = 0, Width = elem1.ActualWidth, Height = elem1.ActualHeight });
            Rect r2 = t2.TransformBounds(new Rect() { X = 0, Y = 0, Width = elem2.ActualWidth, Height = elem2.ActualHeight });
            return r1.IntersectsWith(r2);
        }
        #endregion

        #region Other
        // TODO: sort this out

        public static void RearrangeDays(int first, ref Grid container)
        {
            // Copy the items to a temporary list
            UIElement[] daysCopy = new UIElement[7];
            container.Children.CopyTo(daysCopy, 0);
            
            var elems = (IEnumerable<UIElement>)daysCopy;
            elems = elems.Skip(first-1).Concat(elems.Take(first-1));

            container.Children.Clear();


            int counter = 0;
            foreach (UIElement day in elems)
            {
                day.SetValue(Grid.ColumnProperty, counter);
                container.Children.Add(day);
                counter++;
            }
        }

        #endregion
    }
}