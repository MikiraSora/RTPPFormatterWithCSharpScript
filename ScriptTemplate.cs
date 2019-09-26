using RealTimePPDisplayer.Displayer;
using RealTimePPDisplayer;
using Sync;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/*@@import@@*/

namespace r/*@@namespace@@*/
{
    public class C/*@@class@@*/
    {
        /*@@addition@@*/

        public string Execute(DisplayerBase display)
        {
            /*@@script@@*/
        }

        public Func<DisplayerBase, string> ExecuteWrapper()
        {
            return new Func<DisplayerBase, string>(x => Execute(x));
        }
    }
}