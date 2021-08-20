using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoSchedule.Data
{
    [Serializable]
    public class TaskDescription
    {
        public DayOfWeek Day { get; set; }
        public TimeSpan When { get; set; }
        public TimeSpan Duration { get; set; }
        public string TitleText { get; set; }
        public string DescriptionText { get; set; }
        public TaskColor Color { get; set; }

        public TaskDescription(
            DayOfWeek day, TimeSpan when, TimeSpan duration, string title, string description, TaskColor color)
        {
            Day = day;
            When = when;
            Duration = duration;
            TitleText = title;
            DescriptionText = description;
            Color = color;
        }
    }
}
