using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Services.Events;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Services.News
{
    /// <summary>
    /// Catalogues service
    /// </summary>
    public partial class CataloguesService : ICataloguesService
    {
        #region Constants
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : catalogues ID
        /// </remarks>
        private const string CATALOGUES_BY_ID_KEY = "Nop.catalogues.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : parent catalogues ID
        /// {1} : show hidden records?
        /// {2} : current customer ID
        /// {3} : store ID
        /// </remarks>
        private const string CATALOGUES_BY_PARENT_CATEGORY_ID_KEY = "Nop.catalogues.byparent-{0}-{1}-{2}-{3}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : catalogues ID
        /// {2} : page index
        /// {3} : page size
        /// {4} : current customer ID
        /// {5} : store ID
        /// </remarks>
        private const string NEWSCATALOGUES_ALLBYCATEGORYID_KEY = "Nop.newscatalogues.allbyCatalogueId-{0}-{1}-{2}-{3}-{4}-{5}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : news ID
        /// {2} : current customer ID
        /// {3} : store ID
        /// </remarks>
        private const string NEWSCATALOGUES_ALLBYNEWSID_KEY = "Nop.newscatalogues.allbynewsid-{0}-{1}-{2}-{3}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CATALOGUES_PATTERN_KEY = "Nop.catalogues.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string NEWSCATALOGUES_PATTERN_KEY = "Nop.newscatalogues.";

        #endregion

        #region Fields

        private readonly IRepository<Catalogues> _cataloguesRepository;
        private readonly IRepository<NewsCatalogues> _newsCataloguesRepository;
        private readonly IRepository<NewsItem> _newsRepository;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;   

        #endregion
        
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="cataloguesRepository">Catalogues repository</param>
        /// <param name="newsCataloguesRepository">NewsCatalogues repository</param>
        /// <param name="newsRepository">News repository</param>
        /// <param name="aclRepository">ACL record repository</param>
        /// <param name="storeMappingRepository">Store mapping repository</param>
        /// <param name="workContext">Work context</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="catalogSettings">Catalog settings</param>
        public CataloguesService(ICacheManager cacheManager,
            IRepository<Catalogues> cataloguesRepository,
            IRepository<NewsCatalogues> newsCataloguesRepository,
            IRepository<NewsItem> newsRepository,
            IRepository<AclRecord> aclRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IWorkContext workContext,
            IStoreContext storeContext,
            IEventPublisher eventPublisher,
            IStoreMappingService storeMappingService,
            IAclService aclService
          )
        {
            this._cacheManager = cacheManager;
            this._cataloguesRepository = cataloguesRepository;
            this._newsCataloguesRepository = newsCataloguesRepository;
            this._newsRepository = newsRepository;
            this._aclRepository = aclRepository;
            this._storeMappingRepository = storeMappingRepository;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._eventPublisher = eventPublisher;
            this._storeMappingService = storeMappingService;
            this._aclService = aclService;        
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete catalogues
        /// </summary>
        /// <param name="catalogues">Catalogues</param>
        public virtual void DeleteCatalogues(Catalogues catalogues)
        {
            if (catalogues == null)
                throw new ArgumentNullException("catalogues");

            catalogues.Deleted = true;
            UpdateCatalogues(catalogues);

            //reset a "Parent catalogues" property of all child subcategories
            var subcategories = GetAllCataloguesByParentCatalogueId(catalogues.Id, true);
            foreach (var subcatalogues in subcategories)
            {
                subcatalogues.ParentCatalogueId = 0;
                UpdateCatalogues(subcatalogues);
            }
        }
        
        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="cataloguesName">Catalogues name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Catalogues</returns>
        public virtual IPagedList<Catalogues> GetAllCatalogues(string cataloguesName = "", 
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = _cataloguesRepository.Table;
            if (!showHidden)
                query = query.Where(c => c.Published);
            if (!String.IsNullOrWhiteSpace(cataloguesName))
                query = query.Where(c => c.Name.Contains(cataloguesName));
            query = query.Where(c => !c.Deleted);
            query = query.OrderBy(c => c.ParentCatalogueId).ThenBy(c => c.DisplayOrder);
            
            if (!showHidden )
            {

                //only distinct categories (group by ID)
                query = from c in query
                        group c by c.Id
                        into cGroup
                        orderby cGroup.Key
                        select cGroup.FirstOrDefault();
                query = query.OrderBy(c => c.ParentCatalogueId).ThenBy(c => c.DisplayOrder);
            }
            
            var unsortedCatalogues = query.ToList();

            //sort categories
            var sortedCatalogues = unsortedCatalogues.SortCataloguesForTree();

            //paging
            return new PagedList<Catalogues>(sortedCatalogues, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all categories filtered by parent catalogues identifier
        /// </summary>
        /// <param name="ParentCatalogueId">Parent catalogues identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Catalogues collection</returns>
        public virtual IList<Catalogues> GetAllCataloguesByParentCatalogueId(int ParentCatalogueId,
            bool showHidden = false)
        {
            string key = string.Format(CATALOGUES_BY_PARENT_CATEGORY_ID_KEY, ParentCatalogueId, showHidden, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var query = _cataloguesRepository.Table;
                if (!showHidden)
                    query = query.Where(c => c.Published);
                query = query.Where(c => c.ParentCatalogueId == ParentCatalogueId);
                query = query.Where(c => !c.Deleted);
                query = query.OrderBy(c => c.DisplayOrder);

                if (!showHidden)
                {
                    //only distinct categories (group by ID)
                    query = from c in query
                            group c by c.Id
                            into cGroup
                            orderby cGroup.Key
                            select cGroup.FirstOrDefault();
                    query = query.OrderBy(c => c.DisplayOrder);
                }

                var categories = query.ToList();
                return categories;
            });

        }
        
        /// <summary>
        /// Gets all categories displayed on the home page
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Catalogues</returns>
        public virtual IList<Catalogues> GetAllCataloguesDisplayedOnHomePage(bool showHidden = false)
        {
            var query = from c in _cataloguesRepository.Table
                        orderby c.DisplayOrder
                        where c.Published &&
                        !c.Deleted && 
                        c.ShowOnHomePage
                        select c;

            var categories = query.ToList();
            if (!showHidden)
            {
                categories = categories
                    .Where(c => _aclService.Authorize(c))
                    .ToList();
            }

            return categories;
        }
                
        /// <summary>
        /// Gets a catalogues
        /// </summary>
        /// <param name="catalogueId">Catalogues identifier</param>
        /// <returns>Catalogues</returns>
        public virtual Catalogues GetCatalogueById(int catalogueId)
        {
            if (catalogueId == 0)
                return null;
            
            string key = string.Format(CATALOGUES_BY_ID_KEY, catalogueId);
            return _cacheManager.Get(key, () => _cataloguesRepository.GetById(catalogueId));
        }

        /// <summary>
        /// Inserts catalogues
        /// </summary>
        /// <param name="catalogues">Catalogues</param>
        public virtual void InsertCatalogues(Catalogues catalogues)
        {
            if (catalogues == null)
                throw new ArgumentNullException("catalogues");

            _cataloguesRepository.Insert(catalogues);

            //cache
            _cacheManager.RemoveByPattern(CATALOGUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSCATALOGUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(catalogues);
        }

        /// <summary>
        /// Updates the catalogues
        /// </summary>
        /// <param name="catalogues">Catalogues</param>
        public virtual void UpdateCatalogues(Catalogues catalogues)
        {
            if (catalogues == null)
                throw new ArgumentNullException("catalogues");

            //validate catalogues hierarchy
            var parentCatalogues = GetCatalogueById(catalogues.ParentCatalogueId);
            while (parentCatalogues != null)
            {
                if (catalogues.Id == parentCatalogues.Id)
                {
                    catalogues.ParentCatalogueId = 0;
                    break;
                }
                parentCatalogues = GetCatalogueById(parentCatalogues.ParentCatalogueId);
            }

            _cataloguesRepository.Update(catalogues);

            //cache
            _cacheManager.RemoveByPattern(CATALOGUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSCATALOGUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(catalogues);
        }
        
      
        /// <summary>
        /// Deletes a news catalogues mapping
        /// </summary>
        /// <param name="newsCatalogues">News catalogues</param>
        public virtual void DeleteNewsCatalogues(NewsCatalogues newsCatalogues)
        {
            if (newsCatalogues == null)
                throw new ArgumentNullException("newsCatalogues");

            _newsCataloguesRepository.Delete(newsCatalogues);

            //cache
            _cacheManager.RemoveByPattern(CATALOGUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSCATALOGUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(newsCatalogues);
        }

        /// <summary>
        /// Gets news catalogues mapping collection
        /// </summary>
        /// <param name="catalogueId">Catalogues identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>News a catalogues mapping collection</returns>
        public virtual IPagedList<NewsCatalogues> GetNewsCataloguesByCatalogueId(int catalogueId, int pageIndex, int pageSize, bool showHidden = false)
        {
            if (catalogueId == 0)
                return new PagedList<NewsCatalogues>(new List<NewsCatalogues>(), pageIndex, pageSize);

            string key = string.Format(NEWSCATALOGUES_ALLBYCATEGORYID_KEY, showHidden, catalogueId, pageIndex, pageSize, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in _newsCataloguesRepository.Table
                            join p in _newsRepository.Table on pc.NewsId equals p.Id
                            where pc.CatalogueId == catalogueId &&
                                  !p.Deleted &&
                                  (showHidden || p.Published)
                            orderby pc.DisplayOrder
                            select pc;

                if (!showHidden )
                {
                    //only distinct categories (group by ID)
                    query = from c in query
                            group c by c.Id
                            into cGroup
                            orderby cGroup.Key
                            select cGroup.FirstOrDefault();
                    query = query.OrderBy(pc => pc.DisplayOrder);
                }

                var newsCatalogues = new PagedList<NewsCatalogues>(query, pageIndex, pageSize);
                return newsCatalogues;
            });
        }

        /// <summary>
        /// Gets a news catalogues mapping collection
        /// </summary>
        /// <param name="newsId">News identifier</param>
        /// <param name="showHidden"> A value indicating whether to show hidden records</param>
        /// <returns> News catalogues mapping collection</returns>
        public virtual IList<NewsCatalogues> GetNewsCataloguesByNewsId(int newsId, bool showHidden = false)
        {
            return GetNewsCataloguesByNewsId(newsId, _storeContext.CurrentStore.Id, showHidden);
        }
        /// <summary>
        /// Gets a news catalogues mapping collection
        /// </summary>
        /// <param name="newsId">News identifier</param>
        /// <param name="storeId">Store identifier (used in multi-store environment). "showHidden" parameter should also be "true"</param>
        /// <param name="showHidden"> A value indicating whether to show hidden records</param>
        /// <returns> News catalogues mapping collection</returns>
        public virtual IList<NewsCatalogues> GetNewsCataloguesByNewsId(int newsId, int storeId, bool showHidden = false)
        {
            if (newsId == 0)
                return new List<NewsCatalogues>();

            string key = string.Format(NEWSCATALOGUES_ALLBYNEWSID_KEY, showHidden, newsId, _workContext.CurrentCustomer.Id, storeId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in _newsCataloguesRepository.Table
                            join c in _cataloguesRepository.Table on pc.CatalogueId equals c.Id
                            where pc.NewsId == newsId &&
                                  !c.Deleted &&
                                  (showHidden || c.Published)
                            orderby pc.DisplayOrder
                            select pc;

                var allNewsCatalogues = query.ToList();
                var result = new List<NewsCatalogues>();
                if (!showHidden)
                {
                    foreach (var pc in allNewsCatalogues)
                    {
                        //ACL (access control list) and store mapping
                        var catalogues = pc.Catalogues;
                        if (_aclService.Authorize(catalogues) )
                            result.Add(pc);
                    }
                }
                else
                {
                    //no filtering
                    result.AddRange(allNewsCatalogues);
                }
                return result;
            });
        }

        /// <summary>
        /// Gets a news catalogues mapping 
        /// </summary>
        /// <param name="newsCatalogueId">News catalogues mapping identifier</param>
        /// <returns>News catalogues mapping</returns>
        public virtual NewsCatalogues GetNewsCataloguesById(int newsCatalogueId)
        {
            if (newsCatalogueId == 0)
                return null;

            return _newsCataloguesRepository.GetById(newsCatalogueId);
        }

        /// <summary>
        /// Inserts a news catalogues mapping
        /// </summary>
        /// <param name="newsCatalogues">>News catalogues mapping</param>
        public virtual void InsertNewsCatalogues(NewsCatalogues newsCatalogues)
        {
            if (newsCatalogues == null)
                throw new ArgumentNullException("newsCatalogues");
            
            _newsCataloguesRepository.Insert(newsCatalogues);

            //cache
            _cacheManager.RemoveByPattern(CATALOGUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSCATALOGUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(newsCatalogues);
        }

        /// <summary>
        /// Updates the news catalogues mapping 
        /// </summary>
        /// <param name="newsCatalogues">>News catalogues mapping</param>
        public virtual void UpdateNewsCatalogues(NewsCatalogues newsCatalogues)
        {
            if (newsCatalogues == null)
                throw new ArgumentNullException("newsCatalogues");

            _newsCataloguesRepository.Update(newsCatalogues);

            //cache
            _cacheManager.RemoveByPattern(CATALOGUES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSCATALOGUES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(newsCatalogues);
        }

        #endregion

    }
}
