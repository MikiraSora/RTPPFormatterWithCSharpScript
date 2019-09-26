using RealTimePPDisplayer;
using Sync.Plugins;
using Sync.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTPPFormatterWithCSharpScript
{
    public class RTPPFormatterWithCSharpScriptPlugin : Plugin
    {
        private Logger logger = new Logger<RTPPFormatterWithCSharpScriptPlugin>();

        public RTPPFormatterWithCSharpScriptPlugin() : base(nameof(RTPPFormatterWithCSharpScriptPlugin), "DarkProjector")
        {
            EventBus.BindEvent<PluginEvents.LoadCompleteEvent>(OnLoaded);
        }

        private void OnLoaded(PluginEvents.LoadCompleteEvent @event)
        {
            if (@event.Host.EnumPluings().OfType<RealTimePPDisplayerPlugin>().FirstOrDefault() is RealTimePPDisplayerPlugin rtpp)
            {
                rtpp.RegisterFormatter("rtppfmt-csharp-script", f=>new RtppCSSFormatter(f), "return \"0.000pp\";");
                logger.LogInfomation("register RTPP formatter:rtppfmt-csharp-script");
            }
        }
    }
}
