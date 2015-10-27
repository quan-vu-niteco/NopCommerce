using FluentValidation.Attributes;
using Nop.Core;
using Nop.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.TrialTracker.Domain
{
    [Validator(typeof(TrialTrackerValidator))]
    public class TrialTrackerRecord : BaseEntity
    {
        public int TrialTrackerId { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.TrialTracker.NameLabel")]
        public string CustomerName { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.TrialTracker.EmailLabel")]
        public string CustomerEmail { get; set; }
        public string ProductName { get; set; }
        public int Productid { get; set; }
        public string DownloadDate { get; set; }
        public bool OnMailingList { get; set; }
    }
}
