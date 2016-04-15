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
    /// Catalogue service
    /// </summary>
    public partial class Catalogueervice : ICatalogueervice
    {
        #region Constants
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : Catalogue ID
        /// </remarks>
        private const string Catalogue_BY_ID_KEY = "Nop.Catalogue.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : parent Catalogue ID
        /// {1} : show hidden records?
        /// {2} : current customer ID
        /// {3} : store ID
        /// </remarks>
        private const string Catalogue_BY_PARENT_CATEGORY_ID_KEY = "Nop.Catalogue.byparent-{0}-{1}-{2}-{3}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : Catalogue ID
        /// {2} : page index
        /// {3} : page size
        /// {4} : current customer ID
        /// {5} : store ID
        /// </remarks>
        private const string NEWSCatalogue_ALLBYCATEGORYID_KEY = "Nop.newsCatalogue.allbyCatalogueId-{0}-{1}-{2}-{3}-{4}-{5}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : news ID
        /// {2} : current customer ID
        /// {3} : store ID
        /// </remarks>
        private const string NEWSCatalogue_ALLBYNEWSID_KEY = "Nop.newsCatalogue.allbynewsid-{0}-{1}-{2}-{3}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string Catalogue_PATTERN_KEY = "Nop.Catalogue.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string NEWSCatalogue_PATTERN_KEY = "Nop.newsCatalogue.";

        #endregion

        #region Fields

        private readonly IRepository<Catalogue> _CatalogueRepository;
        private readonly IRepository<NewsCatalogue> _newsCatalogueRepository;
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
        /// <param name="CatalogueRepository">Catalogue repository</param>
        /// <param name="newsCatalogueRepository">NewsCatalogue repository</param>
        /// <param name="newsRepository">News repository</param>
        /// <param name="aclRepository">ACL record repository</param>
        /// <param name="storeMappingRepository">Store mapping repository</param>
        /// <param name="workContext">Work context</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="catalogSettings">Catalog settings</param>
        public Catalogueervice(ICacheManager cacheManager,
            IRepository<Catalogue> CatalogueRepository,
            IRepository<NewsCatalogue> newsCatalogueRepository,
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
            this._CatalogueRepository = CatalogueRepository;
            this._newsCatalogueRepository = newsCatalogueRepository;
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
        /// Delete Catalogue
        /// </summary>
        /// <param name="catalogue">Catalogue</param>
        public virtual void DeleteCatalogue(Catalogue catalogue)
        {
            if (catalogue == null)
                throw new ArgumentNullException("catalogue");

            catalogue.Deleted = true;
            UpdateCatalogue(catalogue);

            //reset a "Parent Catalogue" property of all child subcategories
            var subcategories = GetAllCatalogueByParentCatalogueId(catalogue.Id, true);
            foreach (var subCatalogue in subcategories)
            {
                subCatalogue.ParentCatalogueId = 0;
                UpdateCatalogue(subCatalogue);
            }
        }
        
        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="CatalogueName">Catalogue name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Catalogue</returns>
        public virtual IPagedList<Catalogue> GetAllCatalogue(string CatalogueName = "", 
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = _CatalogueRepository.Table;
            if (!showHidden)
                query = query.Where(c => c.Published);
            if (!String.IsNullOrWhiteSpace(CatalogueName))
                query = query.Where(c => c.Name.Contains(CatalogueName));
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
            
            var unsortedCatalogue = query.ToList();

            //sort categories
            var sortedCatalogue = unsortedCatalogue.SortCatalogueForTree();

            //paging
            return new PagedList<Catalogue>(sortedCatalogue, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all categories filtered by parent Catalogue identifier
        /// </summary>
        /// <param name="ParentCatalogueId">Parent Catalogue identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Catalogue collection</returns>
        public virtual IList<Catalogue> GetAllCatalogueByParentCatalogueId(int ParentCatalogueId,
            bool showHidden = false)
        {
            string key = string.Format(Catalogue_BY_PARENT_CATEGORY_ID_KEY, ParentCatalogueId, showHidden, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var query = _CatalogueRepository.Table;
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
        /// <returns>Catalogue</returns>
        public virtual IList<Catalogue> GetAllCatalogueDisplayedOnHomePage(bool showHidden = false)
        {
            var query = from c in _CatalogueRepository.Table
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
        /// Gets a Catalogue
        /// </summary>
        /// <param name="catalogueId">Catalogue identifier</param>
        /// <returns>Catalogue</returns>
        public virtual Catalogue GetCatalogueById(int catalogueId)
        {
            if (catalogueId == 0)
                return null;
            
            string key = string.Format(Catalogue_BY_ID_KEY, catalogueId);
            return _cacheManager.Get(key, () => _CatalogueRepository.GetById(catalogueId));
        }

        /// <summary>
        /// Inserts Catalogue
        /// </summary>
        /// <param name="catalogue">Catalogue</param>
        public virtual void InsertCatalogue(Catalogue catalogue)
        {
            if (catalogue == null)
                throw new ArgumentNullException("catalogue");

            _CatalogueRepository.Insert(catalogue);

            //cache
            _cacheManager.RemoveByPattern(Catalogue_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSCatalogue_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(catalogue);
        }

        /// <summary>
        /// Updates the Catalogue
        /// </summary>
        /// <param name="catalogue">Catalogue</param>
        public virtual void UpdateCatalogue(Catalogue catalogue)
        {
            if (catalogue == null)
                throw new ArgumentNullException("catalogue");

            //validate Catalogue hierarchy
            var parentCatalogue = GetCatalogueById(catalogue.ParentCatalogueId);
            while (parentCatalogue != null)
            {
                if (catalogue.Id == parentCatalogue.Id)
                {
                    catalogue.ParentCatalogueId = 0;
                    break;
                }
                parentCatalogue = GetCatalogueById(parentCatalogue.ParentCatalogueId);
            }

            _CatalogueRepository.Update(catalogue);

            //cache
            _cacheManager.RemoveByPattern(Catalogue_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSCatalogue_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(catalogue);
        }
        
      
        /// <summary>
        /// Deletes a news Catalogue mapping
        /// </summary>
        /// <param name="newsCatalogue">News Catalogue</param>
        public virtual void DeleteNewsCatalogue(NewsCatalogue newsCatalogue)
        {
            if (newsCatalogue == null)
                throw new ArgumentNullException("newsCatalogue");

            _newsCatalogueRepository.Delete(newsCatalogue);

            //cache
            _cacheManager.RemoveByPattern(Catalogue_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSCatalogue_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(newsCatalogue);
        }

        /// <summary>
        /// Gets news Catalogue mapping collection
        /// </summary>
        /// <param name="catalogueId">Catalogue identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>News a Catalogue mapping collection</returns>
        public virtual IPagedList<NewsCatalogue> GetNewsCatalogueByCatalogueId(int catalogueId, int pageIndex, int pageSize, bool showHidden = false)
        {
            if (catalogueId == 0)
                return new PagedList<NewsCatalogue>(new List<NewsCatalogue>(), pageIndex, pageSize);

            string key = string.Format(NEWSCatalogue_ALLBYCATEGORYID_KEY, showHidden, catalogueId, pageIndex, pageSize, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in _newsCatalogueRepository.Table
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

                var newsCatalogue = new PagedList<NewsCatalogue>(query, pageIndex, pageSize);
                return newsCatalogue;
            });
        }

        /// <summary>
        /// Gets a news Catalogue mapping collection
        /// </summary>
        /// <param name="newsId">News identifier</param>
        /// <param name="showHidden"> A value indicating whether to show hidden records</param>
        /// <returns> News Catalogue mapping collection</returns>
        public virtual IList<NewsCatalogue> GetNewsCatalogueByNewsId(int newsId, bool showHidden = false)
        {
            return GetNewsCatalogueByNewsId(newsId, _storeContext.CurrentStore.Id, showHidden);
        }
        /// <summary>
        /// Gets a news Catalogue mapping collection
        /// </summary>
        /// <param name="newsId">News identifier</param>
        /// <param name="storeId">Store identifier (used in multi-store environment). "showHidden" parameter should also be "true"</param>
        /// <param name="showHidden"> A value indicating whether to show hidden records</param>
        /// <returns> News Catalogue mapping collection</returns>
        public virtual IList<NewsCatalogue> GetNewsCatalogueByNewsId(int newsId, int storeId, bool showHidden = false)
        {
            if (newsId == 0)
                return new List<NewsCatalogue>();

            string key = string.Format(NEWSCatalogue_ALLBYNEWSID_KEY, showHidden, newsId, _workContext.CurrentCustomer.Id, storeId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in _newsCatalogueRepository.Table
                            join c in _CatalogueRepository.Table on pc.CatalogueId equals c.Id
                            where pc.NewsId == newsId &&
                                  !c.Deleted &&
                                  (showHidden || c.Published)
                            orderby pc.DisplayOrder
                            select pc;

                var allNewsCatalogue = query.ToList();
                var result = new List<NewsCatalogue>();
                if (!showHidden)
                {
                    foreach (var pc in allNewsCatalogue)
                    {
                        //ACL (access control list) and store mapping
                        var Catalogue = pc.Catalogue;
                        if (_aclService.Authorize(Catalogue) )
                            result.Add(pc);
                    }
                }
                else
                {
                    //no filtering
                    result.AddRange(allNewsCatalogue);
                }
                return result;
            });
        }

        /// <summary>
        /// Gets a news Catalogue mapping 
        /// </summary>
        /// <param name="newsCatalogueId">News Catalogue mapping identifier</param>
        /// <returns>News Catalogue mapping</returns>
        public virtual NewsCatalogue GetNewsCatalogueById(int newsCatalogueId)
        {
            if (newsCatalogueId == 0)
                return null;

            return _newsCatalogueRepository.GetById(newsCatalogueId);
        }

        /// <summary>
        /// Inserts a news Catalogue mapping
        /// </summary>
        /// <param name="newsCatalogue">>News Catalogue mapping</param>
        public virtual void InsertNewsCatalogue(NewsCatalogue newsCatalogue)
        {
            if (newsCatalogue == null)
                throw new ArgumentNullException("newsCatalogue");
            
            _newsCatalogueRepository.Insert(newsCatalogue);

            //cache
            _cacheManager.RemoveByPattern(Catalogue_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSCatalogue_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(newsCatalogue);
        }

        /// <summary>
        /// Updates the news Catalogue mapping 
        /// </summary>
        /// <param name="newsCatalogue">>News Catalogue mapping</param>
        public virtual void UpdateNewsCatalogue(NewsCatalogue newsCatalogue)
        {
            if (newsCatalogue == null)
                throw new ArgumentNullException("newsCatalogue");

            _newsCatalogueRepository.Update(newsCatalogue);

            //cache
            _cacheManager.RemoveByPattern(Catalogue_PATTERN_KEY);
            _cacheManager.RemoveByPattern(NEWSCatalogue_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(newsCatalogue);
        }

        #endregion

    }
}
