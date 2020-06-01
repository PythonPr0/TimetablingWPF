﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimetablingWPF
{
    static class TimetableSettings
    {
        public static int TeacherMaxPeriods { get; set; } = TimetableStructure.TotalSchedulable;
        public static int DelayBeforeSearching { get; set; } = 300;
        public static int RecentListSize { get; set; } = 6;
    }
}
