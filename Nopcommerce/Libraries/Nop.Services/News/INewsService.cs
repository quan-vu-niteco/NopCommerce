using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.News;

namespace Nop.Services.News
{
    /// <summary>
    /// News service interface
    /// </summary>
    public partial interface INewsService
    {
        /// <summary>
        /// Deletes a news
        /// </summary>
        /// <param name="newsItem">News item</param>
        void DeleteNews(NewsItem newsItem);

        /// <summary>
        /// Gets a news
        /// </summary>
        /// <param name="newsId">The news identifier</param>
        /// <returns>News</returns>
        NewsItem GetNewsById(int newsId);

        /// <summary>
        /// Gets all news
        /// </summary>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>News items</returns>
        IPagedList<NewsItem> GetAllNews(int languageId, int storeId,
            int pageIndex, int pageSize, bool showHidden = false);

        /// <summary>
        /// Inserts a news item
        /// </summary>
        /// <param name="news">News item</param>
        void InsertNews(NewsItem news);

        /// <summary>
        /// Updates the news item
        /// </summary>
        /// <param name="news">News item</param>
        void UpdateNews(NewsItem news);

        /// <summary>
        /// Gets all comments
        /// </summary>
        /// <param name="customerId">Customer identifier; 0 to load all records</param>
        /// <returns>Comments</returns>
        IList<NewsComment> GetAllComments(int customerId);

        /// <summary>
        /// Gets a news comment
        /// </summary>
        /// <param name="newsCommentId">News comment identifier</param>
        /// <returns>News comment</returns>
        NewsComment GetNewsCommentById(int newsCommentId);

        /// <summary>
        /// Deletes a news comment
        /// </summary>
        /// <param name="newsComment">News comment</param>
        void DeleteNewsComment(NewsComment newsComment);


        /// <summary>
        /// Search news
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="featuredNews">A value indicating whether loaded news are marked as featured (relates only to categories and manufacturers). 0 to load featured news only, 1 to load not featured news only, null to load all news</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in news descriptions</param>  
        /// <param name="searchNewsTags">A value indicating whether to search by a specified "keyword" in news tags</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecs">Filtered news specification identifiers</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>News</returns>
        IPagedList<NewsItem> SearchNews(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,           
            bool? featuredNews = null,          
            int newsTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            int languageId = 0,
            IList<int> filteredSpecs = null,
            NewsSortingEnum orderBy = NewsSortingEnum.Position,
            bool showHidden = false);

        /// <summary>
        /// Search news
        /// </summary>
        /// <param name="filterableSpecificationAttributeOptionIds">The specification attribute option identifiers applied to loaded news (all pages)</param>
        /// <param name="loadFilterableSpecificationAttributeOptionIds">A value indicating whether we should load the specification attribute option identifiers applied to loaded news (all pages)</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
        /// <param name="parentGroupedNewsId">Parent news identifier (used with grouped news); 0 to load all records</param>
        /// <param name="newsType">News type; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only news marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="featuredNews">A value indicating whether loaded news are marked as featured (relates only to categories and manufacturers). 0 to load featured news only, 1 to load not featured news only, null to load all news</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="newsTagId">News tag identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in news descriptions</param>
        /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in news SKU</param>
        /// <param name="searchNewsTags">A value indicating whether to search by a specified "keyword" in news tags</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecs">Filtered news specification identifiers</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>News</returns>
        IPagedList<NewsItem> SearchNews(
            out IList<int> filterableSpecificationAttributeOptionIds,
            bool loadFilterableSpecificationAttributeOptionIds = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
             IList<int> categoryIds = null, 
            bool? featuredNews = null,
            string keywords = null,
            bool searchDescriptions = false,
            int languageId = 0,
            IList<int> filteredSpecs = null,
            NewsSortingEnum orderBy = NewsSortingEnum.Position,
            bool showHidden = false);


        #region News pictures

        /// <summary>
        /// Deletes a news picture
        /// </summary>
        /// <param name="newsPicture">News picture</param>
        void DeleteNewsPicture(NewsPicture newsPicture);

        /// <summary>
        /// Gets a news pictures by news identifier
        /// </summary>
        /// <param name="newsId">The news identifier</param>
        /// <returns>News pictures</returns>
        IList<NewsPicture> GetNewsPicturesByNewsId(int newsId);

        /// <summary>
        /// Gets a news picture
        /// </summary>
        /// <param name="newsPictureId">News picture identifier</param>
        /// <returns>News picture</returns>
        NewsPicture GetNewsPictureById(int newsPictureId);

        /// <summary>
        /// Inserts a news picture
        /// </summary>
        /// <param name="newsPicture">News picture</param>
        void InsertNewsPicture(NewsPicture newsPicture);

        /// <summary>
        /// Updates a news picture
        /// </summary>
        /// <param name="newsPicture">News picture</param>
        void UpdateNewsPicture(NewsPicture newsPicture);

        #endregion

    }
}
