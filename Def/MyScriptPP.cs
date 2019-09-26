using RealTimePPDisplayer.Displayer;
using RealTimePPDisplayer;
using Sync;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace r23142rtwdfasf33q3
{
    //pic:https://puu.sh/ElupZ/f3aa717d26.png
    public class C323123234vdfgg546j3423wfg
    {
        public void Clear()
        {
            hi_rtpp = 0;

            SmoothMath.SmoothClean("display.Pp.RealTimePP");
            SmoothMath.SmoothClean("display.Pp.FullComboPP");
        }

        string _round = null;
        string rd => _round ?? (_round = $"F{Setting.RoundDigits}");

        double hi_rtpp=0;

        public string fv(double d) => d.ToString(rd);
        public double smooth(string name, double val) => SmoothMath.SmoothVariable(name, val);

        public string Execute(DisplayerBase display)
        {
            hi_rtpp = Math.Max(display.Pp.RealTimePP, hi_rtpp);

            var rtpp = fv(smooth("display.Pp.RealTimePP", display.Pp.RealTimePP));
            var fcpp = fv(smooth("display.Pp.FullComboPP", display.Pp.FullComboPP));
            var hipp = fv(hi_rtpp);

            var difficult_complete = fv(display.BeatmapTuple.RealTimeStars/display.BeatmapTuple.Stars*100);

            return $"{rtpp}pp>>{fcpp}pp Hi:${hipp} {Environment.NewLine}Diff:{difficult_complete}";
        }

        public Func<DisplayerBase, string> ExecuteWrapper()
        {
            return new Func<DisplayerBase, string>(x => Execute(x));
        }
    }
}