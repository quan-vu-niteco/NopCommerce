using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Widgets.Support.Models
{
    public class SupportItemListModel : BaseNopModel
    {
        public SupportItemListModel()
        {
            SupportItems = new List<SupportItemModel>();
        }
        public IList<SupportItemModel> SupportItems { get; set; }
        
    }
}