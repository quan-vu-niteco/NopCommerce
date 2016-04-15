using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.News
{
    public partial class CatalogueListModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.ContentManagement.Categories.List.SearchCategoryName")]
        [AllowHtml]
        public string SearchCatalogueName { get; set; }
    }
}