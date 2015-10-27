using Nop.Web.Framework.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.TrialTracker.Infrastructure
{
    public class CustomViewEngine : ThemeableRazorViewEngine
    {
        public CustomViewEngine()
        {
            ViewLocationFormats = new[] { "~/Plugins/Misc.TrialTracker/Views/{0}.cshtml" };
            PartialViewLocationFormats = new[] { "~/Plugins/Misc.TrialTracker/Views/{0}.cshtml" };
        }
    }
}
