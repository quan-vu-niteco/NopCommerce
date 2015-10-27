using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Plugin.Widgets.Support.Domain;
using Nop.Services.Events;


namespace Nop.Plugin.Widgets.Support.Services
{
    /// <summary>
    /// support service
    /// </summary>
    public partial class SupportService : ISupportService
    {
        #region Constants
        private const string SUPPORT_ALL_KEY = "Nop.Support.all-{0}-{1}";
        private const string SUPPORT_PATTERN_KEY = "Nop.Support.";
        #endregion

        #region Fields

        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<SupportItem> _supportRepository;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="supportRepository">support repository</param>
        public SupportService(IEventPublisher eventPublisher,
            ICacheManager cacheManager,
            IRepository<SupportItem> supportRepository)
        {
            this._eventPublisher = eventPublisher;
            this._cacheManager = cacheManager;
            this._supportRepository = supportRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a support
        /// </summary>
        /// <param name="support">support</param>
        public virtual void Delete(SupportItem support)
        {
            if (support == null)
                throw new ArgumentNullException("Support");

            _supportRepository.Delete(support);

            _cacheManager.RemoveByPattern(SUPPORT_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(support);
        }

        /// <summary>
        /// Gets all supports
        /// </summary>
        /// <returns>supports</returns>
        public virtual IPagedList<SupportItem> GetAll(int languageId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            string key = string.Format(SUPPORT_ALL_KEY, pageIndex, pageSize);
            return _cacheManager.Get(key, () =>
            {
                var query = from tr in _supportRepository.Table
                            where  tr.LanguageId.Equals(languageId)
                            orderby tr.YahooId, tr.SkypeId, tr.Name, tr.Phone
                            select tr;
                var records = new PagedList<SupportItem>(query, pageIndex, pageSize);
                return records;
            });
        }

        /// <summary>
        /// Gets a support
        /// </summary>
        /// <param name="supportId">support identifier</param>
        /// <returns>support</returns>
        public virtual SupportItem GetById(int supportId)
        {
            if (supportId == 0)
                return null;

            return _supportRepository.GetById(supportId);
        }

        /// <summary>
        /// Inserts a support
        /// </summary>
        /// <param name="support">support</param>
        public virtual void Insert(SupportItem support)
        {
            if (support == null)
                throw new ArgumentNullException("Support");

            _supportRepository.Insert(support);

            _cacheManager.RemoveByPattern(SUPPORT_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(support);
        }

        /// <summary>
        /// Updates the support
        /// </summary>
        /// <param name="support">support</param>
        public virtual void Update(SupportItem support)
        {
            if (support == null)
                throw new ArgumentNullException("Support");

            _supportRepository.Update(support);

            _cacheManager.RemoveByPattern(SUPPORT_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(support);
        }
        #endregion
    }
}
