using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Common
{
    public partial class FooterModel : BaseNopModel
    {
        public string StoreName { get; set; }
        public string FacebookLink { get; set; }
        public string TwitterLink { get; set; }
        public string YoutubeLink { get; set; }
        public string GooglePlusLink { get; set; }
        public bool WishlistEnabled { get; set; }
  
  
        public bool HideOrders { get; set; }
        public bool SitemapEnabled { get; set; }
        public bool NewsEnabled { get; set; }
        public bool CompareProductsEnabled { get; set; }
        public bool AllowPrivateMessages { get; set; }
        public int WorkingLanguageId { get; set; }
    }
}