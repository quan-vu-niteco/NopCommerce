using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Models.Customers;
using Nop.Admin.Models.Stores;
using Nop.Admin.Validators.News;
using Nop.Web.Framework;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.News
{
    [Validator(typeof(CataloguesValidator))]
    public partial class CataloguesModel : BaseNopEntityModel, ILocalizedModel<CataloguesLocalizedModel>
    {
        public CataloguesModel()
        {
            if (PageSize < 1)
            {
                PageSize = 5;
            }
            Locales = new List<CataloguesLocalizedModel>();      
            AvailableCatalogues = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.Parent")]
        public int ParentCatalogueId { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.Picture")]
        public int PictureId { get; set; }


        [UIHint("ImageStandAlone")]
        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.Image")]
        public string Image { get; set; }


        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.PageSize")]
        public int PageSize { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.AllowCustomersToSelectPageSize")]
        public bool AllowCustomersToSelectPageSize { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.PageSizeOptions")]
        public string PageSizeOptions { get; set; }
       

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.ShowOnHomePage")]
        public bool ShowOnHomePage { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.IncludeInTopMenu")]
        public bool IncludeInTopMenu { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.Deleted")]
        public bool Deleted { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
        
        public IList<CataloguesLocalizedModel> Locales { get; set; }

        public string Breadcrumb { get; set; }

        //ACL
        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.SubjectToAcl")]
        public bool SubjectToAcl { get; set; }
        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.AclCustomerRoles")]
        public List<CustomerRoleModel> AvailableCustomerRoles { get; set; }
        public int[] SelectedCustomerRoleIds { get; set; }
       

        public IList<SelectListItem> AvailableCatalogues { get; set; }      


        #region Nested classes

        public partial class CataloguesNewsModel : BaseNopEntityModel
        {
            public int CatalogueId { get; set; }

            public int NewsId { get; set; }

            [NopResourceDisplayName("Admin.ContentManagement.News.News.Fields.News")]
            public string NewsTitle { get; set; }

            [NopResourceDisplayName("Admin.ContentManagement.News.News.Fields.IsFeaturedNews")]
            public bool IsFeaturedNews { get; set; }

            [NopResourceDisplayName("Admin.ContentManagement.News.News.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }
        }

        public partial class AddCataloguesNewsModel : BaseNopModel
        {
            public AddCataloguesNewsModel()
            {
                AvailableCatalogues = new List<SelectListItem>(); 
                AvailableVendors = new List<SelectListItem>();
                AvailableNewsTypes = new List<SelectListItem>();
            }

            [NopResourceDisplayName("Admin.ContentManagement.News.List.SearchNewsTitle")]
            [AllowHtml]
            public string SearchNewsTitle { get; set; }
            [NopResourceDisplayName("Admin.ContentManagement.News.List.SearchCatalogues")]
            public int SearchCatalogueId { get; set; }
            [NopResourceDisplayName("Admin.ContentManagement.News.List.SearchManufacturer")]
            public int SearchManufacturerId { get; set; }
            [NopResourceDisplayName("Admin.ContentManagement.News.List.SearchStore")]
            public int SearchStoreId { get; set; }
            [NopResourceDisplayName("Admin.ContentManagement.News.List.SearchVendor")]
            public int SearchVendorId { get; set; }
            [NopResourceDisplayName("Admin.ContentManagement.News.List.SearchNewsType")]
            public int SearchNewsTypeId { get; set; }

            public IList<SelectListItem> AvailableCatalogues { get; set; } 
            public IList<SelectListItem> AvailableVendors { get; set; }
            public IList<SelectListItem> AvailableNewsTypes { get; set; }

            public int CatalogueId { get; set; }

            public int[] SelectedNewsIds { get; set; }
        }

        #endregion
    }

    public partial class CataloguesLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.Description")]
        [AllowHtml]
        public string Description {get;set;}

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.News.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }
    }
}