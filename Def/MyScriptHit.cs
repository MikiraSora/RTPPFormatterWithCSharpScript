using RealTimePPDisplayer.Displayer;
using RealTimePPDisplayer;
using Sync;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace r23142rtwdfasfq3
{
    //pic:https://puu.sh/Eluqv/66e3457791.png
    public class C323123234vdfgg546j34wfg
    {
        public void Clear()
        {
            prev_combo = 0;
            break_time = 0;
        }

        int prev_combo = 0;
        int break_time = 0;


        public string Execute(DisplayerBase display)
        {
            var n100 = display.HitCount.Count100;
            var n50 = display.HitCount.Count50;
            var nmiss = display.HitCount.CountMiss;

            var cb = display.HitCount.Combo;

            if (cb<prev_combo)
                break_time++;

            prev_combo = cb;

            return $"{n100}x100 {n50}x50 {nmiss}xMoe {break_time}xBrk";
        }

        public Func<DisplayerBase, string> ExecuteWrapper()
        {
            return new Func<DisplayerBase, string>(x => Execute(x));
        }
    }
}