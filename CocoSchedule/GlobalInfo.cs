using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoSchedule.Data;

namespace CocoSchedule
{
    public static class GlobalInfo
    {
        public static double HourHeight;
        public static Dictionary<TaskColor, string> TaskColorResKeys = new Dictionary<TaskColor, string>()
        {
            [TaskColor.GREEN]  = "CellColor_Green",
            [TaskColor.BLUE]   = "CellColor_Blue",
            [TaskColor.BLUE2]  = "CellColor_Blue2",
            [TaskColor.YELLOW] = "CellColor_Yellow",
            [TaskColor.ORANGE] = "CellColor_Orange",
            [TaskColor.RED]    = "CellColor_Red",
            [TaskColor.PINK]   = "CellColor_Pink",
            [TaskColor.PURPLE] = "CellColor_Purple",
            [TaskColor.BEIGE]  = "CellColor_Beige",
            [TaskColor.GRAY]   = "CellColor_Gray",
            [TaskColor.GRAY2]  = "CellColor_Gray2"
        };
    }
}
