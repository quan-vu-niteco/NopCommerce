using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Core;

namespace Nop.Plugin.Widgets.Support.Models
{
    /// <summary>
    /// Represents a support
    /// </summary>
    public partial class SupportItemModel : BaseEntity
    {

        [NopResourceDisplayName("Plugins.Widgets.Support.Fields.Language")]
        public int LanguageId { get; set; }
        
        //[UIHint("ImageStandAlone")]
        [NopResourceDisplayName("Plugins.Widgets.Support.Fields.Image")]
        public string Image { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Support.Fields.YahooId")]
        public string YahooId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Support.Fields.SkypeId")]
        public string SkypeId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Support.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Support.Fields.Phone")]
        public string Phone { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Support.Fields.HotLine")]
        public string HotLine { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Support.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

    }
}