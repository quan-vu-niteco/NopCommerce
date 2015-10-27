using Nop.Core.Configuration;

namespace Nop.Core.Domain.News
{
    public class CataloguesSettings : ISettings
    {

     

        /// <summary>
        /// Gets or sets a value indicating whether to display GTIN of a product
        /// </summary>
        public bool ShowGtin { get; set; }

       

        /// <summary>
        /// Gets or sets a value indicating whether product sorting is enabled
        /// </summary>
        public bool AllowNewsSorting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to change product view mode
        /// </summary>
        public bool AllowNewsViewModeChanging { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to change product view mode
        /// </summary>
        public string DefaultViewMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a category details page should include products from subcategories
        /// </summary>
        public bool ShowNewsFromSubcategories { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether number of products should be displayed beside each category
        /// </summary>
        public bool ShowCataloguesNewsNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we include subcategories (when 'ShowCataloguesNewsNumber' is 'true')
        /// </summary>
        public bool ShowCataloguesNewsNumberIncludingSubcategories { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether category breadcrumb is enabled
        /// </summary>
        public bool CataloguesBreadcrumbEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether a 'Share button' is enabled
        /// </summary>
        public bool ShowShareButton { get; set; }


        /// <summary>
        /// Gets or sets a value indicating product reviews must be approved
        /// </summary>
        public bool NewsReviewsMustBeApproved { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the default rating value of the product reviews
        /// </summary>
        public int DefaultNewsRatingValue { get; set; }       

        /// <summary>
        /// Gets or sets a number of "Recently added products"
        /// </summary>
        public int RecentlyAddedNewsNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether "Recently added products" feature is enabled
        /// </summary>
        public bool RecentlyAddedNewsEnabled { get; set; }

        public bool NewsSearchAutoCompleteEnabled { get; set; }

        /// <summary>
        /// Gets or sets a number of products to return when using "autocomplete" feature
        /// </summary>
        public int NewsSearchAutoCompleteNumberOfNews { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to show product images in the auto complete search
        /// </summary>
        public bool ShowNewsImagesInSearchAutoComplete { get; set; }       

        /// <summary>
        /// Gets or sets a number of products per page on the search products page
        /// </summary>
        public int SearchPageNewsPerPage { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to select page size on the search products page
        /// </summary>
        public bool SearchPageAllowCustomersToSelectPageSize { get; set; }
        /// <summary>
        /// Gets or sets the available customer selectable page size options on the search products page
        /// </summary>
        public string SearchPagePageSizeOptions { get; set; }

    
        /// <summary>
        /// Gets or sets a value indicating whether dynamic price update is enabled
        /// </summary>
        public bool EnableDynamicPriceUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should load new price using AJAX when changing product attributes. This option works only with "EnableDynamicPriceUpdate" enabled
        /// </summary>
        public bool DynamicPriceUpdateAjax { get; set; }

        /// <summary>
        /// Gets or sets a number of product tags that appear in the tag cloud
        /// </summary>
        public int NumberOfNewsTags { get; set; }

        /// <summary>
        /// Gets or sets a number of products per page on 'products by tag' page
        /// </summary>
        public int NewsByTagPageSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers can select the page size for 'products by tag'
        /// </summary>
        public bool NewsByTagAllowCustomersToSelectPageSize { get; set; }

        /// <summary>
        /// Gets or sets the available customer selectable page size options for 'products by tag'
        /// </summary>
        public string NewsByTagPageSizeOptions { get; set; }
     
        /// <summary>
        /// Gets or sets a value indicating whether to ignore ACL rules (side-wide). It can significantly improve performance when enabled.
        /// </summary>
        public bool IgnoreAcl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore "limit per store" rules (side-wide). It can significantly improve performance when enabled.
        /// </summary>
        public bool IgnoreStoreLimitations { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to cache product prices. It can significantly improve performance when enabled.
        /// </summary>
        public bool CacheNewsPrices { get; set; }

        /// <summary>
        /// Gets or sets the default value to use for Catalogues page size options (for new Categories)
        /// </summary>
        public string DefaultCataloguesPageSizeOptions { get; set; }


        /// <summary>
        /// Gets or sets the value indicating how many subcategory levels to display in the top menu with categories
        /// </summary>
        public int TopCataloguesMenuSubcategoryLevelsToDisplay { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether we should load all subcategories in the left menu with categories.
        /// It can be used by third-party theme developers.
        /// </summary>
        public bool LoadAllSideCataloguesMenuSubcategories { get; set; }
       

      
    }
}