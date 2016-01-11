using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;

using Nop.Core.Domain.News;
using Nop.Core.Domain.Stores;
using Nop.Services.Events;
using System.Data;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Core.Domain.Common;
using Nop.Services.Security;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Localization;

namespace Nop.Services.News
{
    /// <summary>
    /// News service
    /// </summary>
    public partial class NewsService : INewsService
    {
        #region Fields

        private readonly IRepository<NewsItem> _newsItemRepository;
        private readonly IRepository<NewsComment> _newsCommentRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IRepository<AclRecord> _aclRepository;
        
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<NewsPicture> _newsPictureRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IDataProvider _dataProvider;
        private readonly IDbContext _dbContext;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILanguageService _languageService;
        private readonly CommonSettings _commonSettings;
        private readonly IAclService _aclService;

        #endregion

        #region Ctor

        public NewsService(IRepository<NewsItem> newsItemRepository,
            IRepository<NewsComment> newsCommentRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IRepository<AclRecord> aclRepository,
            
            IEventPublisher eventPublisher, 
            IRepository<NewsPicture> newsPictureRepository,
            IRepository<LocalizedProperty> localizedPropertyRepository,
            IDataProvider dataProvider,
            IDbContext dbContext,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILanguageService languageService,
            CommonSettings commonSettings, IAclService aclService)
        {
            this._newsItemRepository = newsItemRepository;
            this._newsCommentRepository = newsCommentRepository;
            this._storeMappingRepository = storeMappingRepository;
            this._aclRepository = aclRepository;
            
            this._eventPublisher = eventPublisher;
            this._newsPictureRepository = newsPictureRepository;
            this._localizedPropertyRepository = localizedPropertyRepository;
            this._dataProvider = dataProvider;
            this._dbContext = dbContext;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._commonSettings = commonSettings;
            this._languageService = languageService;
            this._commonSettings = commonSettings;
            this._aclService = aclService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a news
        /// </summary>
        /// <param name="newsItem">News item</param>
        public virtual void DeleteNews(NewsItem newsItem)
        {
            if (newsItem == null)
                throw new ArgumentNullException("newsItem");

            _newsItemRepository.Delete(newsItem);

            //event notification
            _eventPublisher.EntityDeleted(newsItem);
        }

        /// <summary>
        /// Gets a news
        /// </summary>
        /// <param name="newsId">The news identifier</param>
        /// <returns>News</returns>
        public virtual NewsItem GetNewsById(int newsId)
        {
            if (newsId == 0)
                return null;

            return _newsItemRepository.GetById(newsId);
        }

        /// <summary>
        /// Gets all news
        /// </summary>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>News items</returns>
        public virtual IPagedList<NewsItem> GetAllNews(int languageId, int storeId,
            int pageIndex, int pageSize, bool showHidden = false)
        {
            var query = _newsItemRepository.Table;
            if (languageId > 0)
                query = query.Where(n => languageId == n.LanguageId);
            if (!showHidden)
            {
                var utcNow = DateTime.UtcNow;
                query = query.Where(n => n.Published);
                query = query.Where(n => !n.StartDateUtc.HasValue || n.StartDateUtc <= utcNow);
                query = query.Where(n => !n.EndDateUtc.HasValue || n.EndDateUtc >= utcNow);
            }
            query = query.OrderByDescending(n => n.CreatedOnUtc);

            //Store mapping
            if (storeId > 0 )
            {
                query = from n in query
                        join sm in _storeMappingRepository.Table
                        on new { c1 = n.Id, c2 = "NewsItem" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into n_sm
                        from sm in n_sm.DefaultIfEmpty()
                        where !n.LimitedToStores || storeId == sm.StoreId
                        select n;

                //only distinct items (group by ID)
                query = from n in query
                        group n by n.Id
                            into nGroup
                            orderby nGroup.Key
                            select nGroup.FirstOrDefault();
                query = query.OrderByDescending(n => n.CreatedOnUtc);
            }

            var news = new PagedList<NewsItem>(query, pageIndex, pageSize);
            return news;
        }

        /// <summary>
        /// Inserts a news item
        /// </summary>
        /// <param name="news">News item</param>
        public virtual void InsertNews(NewsItem news)
        {
            if (news == null)
                throw new ArgumentNullException("news");

            _newsItemRepository.Insert(news);

            //event notification
            _eventPublisher.EntityInserted(news);
        }

        /// <summary>
        /// Updates the news item
        /// </summary>
        /// <param name="news">News item</param>
        public virtual void UpdateNews(NewsItem news)
        {
            if (news == null)
                throw new ArgumentNullException("news");

            _newsItemRepository.Update(news);

            //event notification
            _eventPublisher.EntityUpdated(news);
        }

        /// <summary>
        /// Gets all comments
        /// </summary>
        /// <param name="customerId">Customer identifier; 0 to load all records</param>
        /// <returns>Comments</returns>
        public virtual IList<NewsComment> GetAllComments(int customerId)
        {
            var query = from c in _newsCommentRepository.Table
                        orderby c.CreatedOnUtc
                        where (customerId == 0 || c.CustomerId == customerId)
                        select c;
            var content = query.ToList();
            return content;
        }

        /// <summary>
        /// Gets a news comment
        /// </summary>
        /// <param name="newsCommentId">News comment identifier</param>
        /// <returns>News comment</returns>
        public virtual NewsComment GetNewsCommentById(int newsCommentId)
        {
            if (newsCommentId == 0)
                return null;

            return _newsCommentRepository.GetById(newsCommentId);
        }

        /// <summary>
        /// Deletes a news comment
        /// </summary>
        /// <param name="newsComment">News comment</param>
        public virtual void DeleteNewsComment(NewsComment newsComment)
        {
            if (newsComment == null)
                throw new ArgumentNullException("newsComment");

            _newsCommentRepository.Delete(newsComment);
        }

        /// <summary>
        /// Search news
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Catalogues identifiers</param>
        /// <param name="featuredNews">A value indicating whether loaded news are marked as featured (relates only to categories and manufacturers). 0 to load featured news only, 1 to load not featured news only, null to load all news</param>
       /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in news descriptions</param>
        /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in news SKU</param>
        /// <param name="searchNewsTags">A value indicating whether to search by a specified "keyword" in news tags</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecs">Filtered news specification identifiers</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>News</returns>
        public virtual IPagedList<NewsItem> SearchNews(
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
            bool showHidden = false)
        {
            IList<int> filterableSpecificationAttributeOptionIds;
            return SearchNews(out filterableSpecificationAttributeOptionIds, false,
                pageIndex, pageSize, categoryIds, featuredNews, keywords, searchDescriptions,
                 languageId, filteredSpecs, orderBy, showHidden);
        }

        /// <summary>
        /// Search news
        /// </summary>
        /// <param name="filterableSpecificationAttributeOptionIds">The specification attribute option identifiers applied to loaded news (all pages)</param>
        /// <param name="loadFilterableSpecificationAttributeOptionIds">A value indicating whether we should load the specification attribute option identifiers applied to loaded news (all pages)</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Catalogues identifiers</param>        /// <param name="parentGroupedNewsId">Parent news identifier (used with grouped news); 0 to load all records</param>
        /// <param name="newsType">News type; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only news marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>     
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in news descriptions</param>
        /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in news SKU</param>
        /// <param name="searchNewsTags">A value indicating whether to search by a specified "keyword" in news tags</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecs">Filtered news specification identifiers</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>News</returns>  
        public virtual IPagedList<NewsItem> SearchNews(
            out IList<int> filterableSpecificationAttributeOptionIds,
            bool loadFilterableSpecificationAttributeOptionIds = false,
            int pageIndex = 0,
            int pageSize = 2147483647,  //Int32.MaxValue
            IList<int> categoryIds = null, 
            bool? featuredNews = null,
            string keywords = null,
            bool searchDescriptions = false,       
            int languageId = 0,
            IList<int> filteredSpecs = null,
            NewsSortingEnum orderBy = NewsSortingEnum.Position,
            bool showHidden = false)
        {
            filterableSpecificationAttributeOptionIds = new List<int>();

            //search by keyword
            bool searchLocalizedValue = false;
            if (languageId > 0)
            {
                if (showHidden)
                {
                    searchLocalizedValue = true;
                }
                else
                {
                    //ensure that we have at least two published languages
                    var totalPublishedLanguages = _languageService.GetAllLanguages(storeId: _storeContext.CurrentStore.Id).Count;
                    searchLocalizedValue = totalPublishedLanguages >= 2;
                }
            }

            //validate "categoryIds" parameter
            if (categoryIds != null && categoryIds.Contains(0))
                categoryIds.Remove(0);

            //Access control list. Allowed customer roles
            var allowedCustomerRolesIds = _workContext.CurrentCustomer.CustomerRoles
                .Where(cr => cr.Active).Select(cr => cr.Id).ToList();

            if (_commonSettings.UseStoredProceduresIfSupported && _dataProvider.StoredProceduredSupported)
            {
                //stored procedures are enabled and supported by the database. 
                //It's much faster than the LINQ implementation below 

                #region Use stored procedure

                //pass category identifiers as comma-delimited string
                string commaSeparatedCatalogueIds = "";
                if (categoryIds != null)
                {
                    for (int i = 0; i < categoryIds.Count; i++)
                    {
                        commaSeparatedCatalogueIds += categoryIds[i].ToString();
                        if (i != categoryIds.Count - 1)
                        {
                            commaSeparatedCatalogueIds += ",";
                        }
                    }
                }


                //pass customer role identifiers as comma-delimited string
                string commaSeparatedAllowedCustomerRoleIds = "";
                for (int i = 0; i < allowedCustomerRolesIds.Count; i++)
                {
                    commaSeparatedAllowedCustomerRoleIds += allowedCustomerRolesIds[i].ToString();
                    if (i != allowedCustomerRolesIds.Count - 1)
                    {
                        commaSeparatedAllowedCustomerRoleIds += ",";
                    }
                }


                //pass specification identifiers as comma-delimited string
                string commaSeparatedSpecIds = "";
                if (filteredSpecs != null)
                {
                    ((List<int>)filteredSpecs).Sort();
                    for (int i = 0; i < filteredSpecs.Count; i++)
                    {
                        commaSeparatedSpecIds += filteredSpecs[i].ToString();
                        if (i != filteredSpecs.Count - 1)
                        {
                            commaSeparatedSpecIds += ",";
                        }
                    }
                }

                //some databases don't support int.MaxValue
                if (pageSize == int.MaxValue)
                    pageSize = int.MaxValue - 1;

                //prepare parameters
                var pCatalogueIds = _dataProvider.GetParameter();
                pCatalogueIds.ParameterName = "CatalogueIds";
                pCatalogueIds.Value = commaSeparatedCatalogueIds != null ? (object)commaSeparatedCatalogueIds : DBNull.Value;
                pCatalogueIds.DbType = DbType.String;
                  

                var pFeaturedNews = _dataProvider.GetParameter();
                pFeaturedNews.ParameterName = "FeaturedNews";
                pFeaturedNews.Value = featuredNews.HasValue ? (object)featuredNews.Value : DBNull.Value;
                pFeaturedNews.DbType = DbType.Boolean;


                var pKeywords = _dataProvider.GetParameter();
                pKeywords.ParameterName = "Keywords";
                pKeywords.Value = keywords != null ? (object)keywords : DBNull.Value;
                pKeywords.DbType = DbType.String;

                var pSearchDescriptions = _dataProvider.GetParameter();
                pSearchDescriptions.ParameterName = "SearchDescriptions";
                pSearchDescriptions.Value = searchDescriptions;
                pSearchDescriptions.DbType = DbType.Boolean;


                var pUseFullTextSearch = _dataProvider.GetParameter();
                pUseFullTextSearch.ParameterName = "UseFullTextSearch";
                pUseFullTextSearch.Value = _commonSettings.UseFullTextSearch;
                pUseFullTextSearch.DbType = DbType.Boolean;

                var pFullTextMode = _dataProvider.GetParameter();
                pFullTextMode.ParameterName = "FullTextMode";
                pFullTextMode.Value = (int)_commonSettings.FullTextMode;
                pFullTextMode.DbType = DbType.Int32;

                var pFilteredSpecs = _dataProvider.GetParameter();
                pFilteredSpecs.ParameterName = "FilteredSpecs";
                pFilteredSpecs.Value = commaSeparatedSpecIds != null ? (object)commaSeparatedSpecIds : DBNull.Value;
                pFilteredSpecs.DbType = DbType.String;

                var pLanguageId = _dataProvider.GetParameter();
                pLanguageId.ParameterName = "LanguageId";
                pLanguageId.Value = searchLocalizedValue ? languageId : 0;
                pLanguageId.DbType = DbType.Int32;

                var pOrderBy = _dataProvider.GetParameter();
                pOrderBy.ParameterName = "OrderBy";
                pOrderBy.Value = (int)orderBy;
                pOrderBy.DbType = DbType.Int32;

                var pAllowedCustomerRoleIds = _dataProvider.GetParameter();
                pAllowedCustomerRoleIds.ParameterName = "AllowedCustomerRoleIds";
                pAllowedCustomerRoleIds.Value = commaSeparatedAllowedCustomerRoleIds;
                pAllowedCustomerRoleIds.DbType = DbType.String;

                var pPageIndex = _dataProvider.GetParameter();
                pPageIndex.ParameterName = "PageIndex";
                pPageIndex.Value = pageIndex;
                pPageIndex.DbType = DbType.Int32;

                var pPageSize = _dataProvider.GetParameter();
                pPageSize.ParameterName = "PageSize";
                pPageSize.Value = pageSize;
                pPageSize.DbType = DbType.Int32;

                var pShowHidden = _dataProvider.GetParameter();
                pShowHidden.ParameterName = "ShowHidden";
                pShowHidden.Value = showHidden;
                pShowHidden.DbType = DbType.Boolean;

                var pLoadFilterableSpecificationAttributeOptionIds = _dataProvider.GetParameter();
                pLoadFilterableSpecificationAttributeOptionIds.ParameterName = "LoadFilterableSpecificationAttributeOptionIds";
                pLoadFilterableSpecificationAttributeOptionIds.Value = loadFilterableSpecificationAttributeOptionIds;
                pLoadFilterableSpecificationAttributeOptionIds.DbType = DbType.Boolean;

                var pFilterableSpecificationAttributeOptionIds = _dataProvider.GetParameter();
                pFilterableSpecificationAttributeOptionIds.ParameterName = "FilterableSpecificationAttributeOptionIds";
                pFilterableSpecificationAttributeOptionIds.Direction = ParameterDirection.Output;
                pFilterableSpecificationAttributeOptionIds.Size = int.MaxValue - 1;
                pFilterableSpecificationAttributeOptionIds.DbType = DbType.String;

                var pTotalRecords = _dataProvider.GetParameter();
                pTotalRecords.ParameterName = "TotalRecords";
                pTotalRecords.Direction = ParameterDirection.Output;
                pTotalRecords.DbType = DbType.Int32;

                //invoke stored procedure
                var news = _dbContext.ExecuteStoredProcedureList<NewsItem>(
                    "NewsLoadAllPaged",
                    pCatalogueIds,
                    pFeaturedNews,
                    pKeywords,
                    pSearchDescriptions,                 
                    pUseFullTextSearch,
                    pFullTextMode,
                    pFilteredSpecs,
                    pLanguageId,
                    pOrderBy,
                    pAllowedCustomerRoleIds,
                    pPageIndex,
                    pPageSize,
                    pShowHidden,
                    pLoadFilterableSpecificationAttributeOptionIds,
                    pFilterableSpecificationAttributeOptionIds,
                    pTotalRecords);
                //get filterable specification attribute option identifier
                string filterableSpecificationAttributeOptionIdsStr = (pFilterableSpecificationAttributeOptionIds.Value != DBNull.Value) ? (string)pFilterableSpecificationAttributeOptionIds.Value : "";
                if (loadFilterableSpecificationAttributeOptionIds &&
                    !string.IsNullOrWhiteSpace(filterableSpecificationAttributeOptionIdsStr))
                {
                    filterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIdsStr
                       .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(x => Convert.ToInt32(x.Trim()))
                       .ToList();
                }
                //return news
                int totalRecords = (pTotalRecords.Value != DBNull.Value) ? Convert.ToInt32(pTotalRecords.Value) : 0;
                return new PagedList<NewsItem>(news, pageIndex, pageSize, totalRecords);

                #endregion
            }
            else
            {
                //stored procedures aren't supported. Use LINQ

                #region Search news

                //news
                var query = _newsItemRepository.Table;
                query = query.Where(p => !p.Deleted);
                if (!showHidden)
                {
                    query = query.Where(p => p.Published);
                }   

                //The function 'CurrentUtcDateTime' is not supported by SQL Server Compact. 
                //That's why we pass the date value
                var nowUtc = DateTime.UtcNow;

                if (!showHidden)
                {
                    //available dates
                    query = query.Where(p =>
                        (!p.AvailableStartDateTimeUtc.HasValue || p.AvailableStartDateTimeUtc.Value < nowUtc) &&
                        (!p.AvailableEndDateTimeUtc.HasValue || p.AvailableEndDateTimeUtc.Value > nowUtc));
                }

                if (!showHidden)
                {
                    //ACL (access control list)
                    query = from p in query
                            join acl in _aclRepository.Table
                            on new { c1 = p.Id, c2 = "News" } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into p_acl
                            from acl in p_acl.DefaultIfEmpty()
                            where !p.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
                            select p;
                }


                //category filtering
                if (categoryIds != null && categoryIds.Count > 0)
                {
                    query = from p in query
                            from pc in p.NewsCatalogues.Where(pc => categoryIds.Contains(pc.CatalogueId))
                            where (!featuredNews.HasValue || featuredNews.Value == pc.IsFeaturedNews)
                            select p;
                }

                //related news filtering
                //if (relatedToNewsId > 0)
                //{
                //    query = from p in query
                //            join rp in _relatedNewsRepository.Table on p.Id equals rp.NewsId2
                //            where (relatedToNewsId == rp.NewsId1)
                //            select p;
                //}



                //only distinct news (group by ID)
                //if we use standard Distinct() method, then all fields will be compared (low performance)
                //it'll not work in SQL Server Compact when searching news by a keyword)
                query = from p in query
                        group p by p.Id
                            into pGroup
                            orderby pGroup.Key
                            select pGroup.FirstOrDefault();

                //sort news
                if (orderBy == NewsSortingEnum.Position && categoryIds != null && categoryIds.Count > 0)
                {
                    //category position
                    var firstCatalogueId = categoryIds[0];
                    query = query.OrderBy(p => p.NewsCatalogues.FirstOrDefault(pc => pc.CatalogueId == firstCatalogueId).DisplayOrder);
                }              
                else if (orderBy == NewsSortingEnum.Position)
                {
                    //otherwise sort by name
                    query = query.OrderBy(p => p.Title);
                }
                else if (orderBy == NewsSortingEnum.NameAsc)
                {
                    //Name: A to Z
                    query = query.OrderBy(p => p.Title);
                }
                else if (orderBy == NewsSortingEnum.NameDesc)
                {
                    //Name: Z to A
                    query = query.OrderByDescending(p => p.Title);
                }
                else if (orderBy == NewsSortingEnum.CreatedOn)
                {
                    //creation date
                    query = query.OrderByDescending(p => p.CreatedOnUtc);
                }
                else
                {
                    //actually this code is not reachable
                    query = query.OrderBy(p => p.Title);
                }
                var news = new PagedList<NewsItem>(query, pageIndex, pageSize);
                //return news
                return news;

                #endregion
            }
        }


        #region News pictures

        /// <summary>
        /// Deletes a news picture
        /// </summary>
        /// <param name="newsPicture">News picture</param>
        public virtual void DeleteNewsPicture(NewsPicture newsPicture)
        {
            if (newsPicture == null)
                throw new ArgumentNullException("newsPicture");

            _newsPictureRepository.Delete(newsPicture);

            //event notification
            _eventPublisher.EntityDeleted(newsPicture);
        }

        /// <summary>
        /// Gets a news pictures by news identifier
        /// </summary>
        /// <param name="newsId">The news identifier</param>
        /// <returns>News pictures</returns>
        public virtual IList<NewsPicture> GetNewsPicturesByNewsId(int newsId)
        {
            var query = from pp in _newsPictureRepository.Table
                        where pp.NewsId == newsId
                        orderby pp.DisplayOrder
                        select pp;
            var newsPictures = query.ToList();
            return newsPictures;
        }

        /// <summary>
        /// Gets a news picture
        /// </summary>
        /// <param name="newsPictureId">News picture identifier</param>
        /// <returns>News picture</returns>
        public virtual NewsPicture GetNewsPictureById(int newsPictureId)
        {
            if (newsPictureId == 0)
                return null;

            return _newsPictureRepository.GetById(newsPictureId);
        }

        /// <summary>
        /// Inserts a news picture
        /// </summary>
        /// <param name="newsPicture">News picture</param>
        public virtual void InsertNewsPicture(NewsPicture newsPicture)
        {
            if (newsPicture == null)
                throw new ArgumentNullException("newsPicture");

            _newsPictureRepository.Insert(newsPicture);

            //event notification
            _eventPublisher.EntityInserted(newsPicture);
        }

        /// <summary>
        /// Updates a news picture
        /// </summary>
        /// <param name="newsPicture">News picture</param>
        public virtual void UpdateNewsPicture(NewsPicture newsPicture)
        {
            if (newsPicture == null)
                throw new ArgumentNullException("newsPicture");

            _newsPictureRepository.Update(newsPicture);

            //event notification
            _eventPublisher.EntityUpdated(newsPicture);
        }

        #endregion
        #endregion




       
    }
}
