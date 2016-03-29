using Nop.Core.Caching;
using Nop.Core.Domain.Configuration;

using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Topics;

using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Events;

namespace Nop.Web.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class ModelCacheEventConsumer: 
        //languages
        IConsumer<EntityInserted<Language>>,
        IConsumer<EntityUpdated<Language>>,
        IConsumer<EntityDeleted<Language>>,
      
        //settings
        IConsumer<EntityUpdated<Setting>>,
    
        //Topics
        IConsumer<EntityInserted<Topic>>,
        IConsumer<EntityUpdated<Topic>>,
        IConsumer<EntityDeleted<Topic>>,
      
        //Picture
        IConsumer<EntityInserted<Picture>>,
        IConsumer<EntityUpdated<Picture>>,
        IConsumer<EntityDeleted<Picture>>,
      
        //polls
        IConsumer<EntityInserted<Poll>>,
        IConsumer<EntityUpdated<Poll>>,
        IConsumer<EntityDeleted<Poll>>,
        //news items
        IConsumer<EntityInserted<NewsItem>>,
        IConsumer<EntityUpdated<NewsItem>>,
        IConsumer<EntityDeleted<NewsItem>>
     
        
    {
        /// <summary>
        /// Key for TopicModel caching
        /// </summary>
        /// <remarks>
        /// {0} : topic system name
        /// {1} : language id
        /// {2} : store id
        /// </remarks>
        public const string TOPIC_MODEL_BY_SYSTEMNAME_KEY = "Nop.pres.topic.details.bysystemname-{0}-{1}-{2}";
        /// <summary>
        /// Key for TopicModel caching
        /// </summary>
        /// <remarks>
        /// {0} : topic id
        /// {1} : language id
        /// {2} : store id
        /// </remarks>
        public const string TOPIC_MODEL_BY_ID_KEY = "Nop.pres.topic.details.byid-{0}-{1}-{2}";
        /// <summary>
        /// Key for TopicModel caching
        /// </summary>
        /// <remarks>
        /// {0} : topic system name
        /// {1} : language id
        /// {2} : store id
        /// </remarks>
        public const string TOPIC_SENAME_BY_SYSTEMNAME = "Nop.pres.topic.sename.bysystemname-{0}-{1}-{2}";
        /// <summary>
        /// Key for TopMenuModel caching
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : current store ID
        /// </remarks>
        public const string TOPIC_TOP_MENU_MODEL_KEY = "Nop.pres.topic.topmenu-{0}-{1}";
        /// <summary>
        /// Key for TopMenuModel caching
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : current store ID
        /// </remarks>
        public const string TOPIC_FOOTER_MODEL_KEY = "Nop.pres.topic.footer-{0}-{1}";
        public const string TOPIC_PATTERN_KEY = "Nop.pres.topic";

        /// <summary>
        /// Key for CategoryTemplate caching
        /// </summary>
        /// <remarks>
        /// {0} : category template id
        /// </remarks>
        public const string CATEGORY_TEMPLATE_MODEL_KEY = "Nop.pres.categorytemplate-{0}";
        public const string CATEGORY_TEMPLATE_PATTERN_KEY = "Nop.pres.categorytemplate";

        /// <summary>
        /// Key for ManufacturerTemplate caching
        /// </summary>
        /// <remarks>
        /// {0} : manufacturer template id
        /// </remarks>
        public const string MANUFACTURER_TEMPLATE_MODEL_KEY = "Nop.pres.manufacturertemplate-{0}";
        public const string MANUFACTURER_TEMPLATE_PATTERN_KEY = "Nop.pres.manufacturertemplate";

        /// <summary>
        /// Key for ProductTemplate caching
        /// </summary>
        /// <remarks>
        /// {0} : product template id
        /// </remarks>
        public const string PRODUCT_TEMPLATE_MODEL_KEY = "Nop.pres.producttemplate-{0}";
        public const string PRODUCT_TEMPLATE_PATTERN_KEY = "Nop.pres.producttemplate";

        /// <summary>
        /// Key for TopicTemplate caching
        /// </summary>
        /// <remarks>
        /// {0} : topic template id
        /// </remarks>
        public const string TOPIC_TEMPLATE_MODEL_KEY = "Nop.pres.topictemplate-{0}";
        public const string TOPIC_TEMPLATE_PATTERN_KEY = "Nop.pres.topictemplate";

        /// <summary>
        /// Key for bestsellers identifiers displayed on the home page
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// </remarks>
        public const string HOMEPAGE_BESTSELLERS_IDS_KEY = "Nop.pres.bestsellers.homepage-{0}";
        public const string HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY = "Nop.pres.bestsellers.homepage";
         public const string NEWS_DEFAULTPICTURE_MODEL_KEY = "Nop.pres.product.detailspictures-{0}-{1}-{2}-{3}-{4}-{5}";
       
       
        /// <summary>
        /// Key for home page polls
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// </remarks>
        public const string HOMEPAGE_POLLS_MODEL_KEY = "Nop.pres.poll.homepage-{0}";
        /// <summary>
        /// Key for polls by system name
        /// </summary>
        /// <remarks>
        /// {0} : poll system name
        /// {1} : language ID
        /// </remarks>
        public const string POLL_BY_SYSTEMNAME__MODEL_KEY = "Nop.pres.poll.systemname-{0}-{1}";
        public const string POLLS_PATTERN_KEY = "Nop.pres.poll";

        /// <summary>
        /// Key for blog tag list model
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : current store ID
        /// </remarks>
        public const string BLOG_TAGS_MODEL_KEY = "Nop.pres.blog.tags-{0}-{1}";
        /// <summary>
        /// Key for blog archive (years, months) block model
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : current store ID
        /// </remarks>
        public const string BLOG_MONTHS_MODEL_KEY = "Nop.pres.blog.months-{0}-{1}";
        public const string BLOG_PATTERN_KEY = "Nop.pres.blog";

        /// <summary>
        /// Key for home page news
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : current store ID
        /// </remarks>
        public const string HOMEPAGE_NEWSMODEL_KEY = "Nop.pres.news.homepage-{0}-{1}";
        public const string NEWS_PATTERN_KEY = "Nop.pres.news";
        
        /// <summary>
        /// Key for states by country id
        /// </summary>
        /// <remarks>
        /// {0} : country ID
        /// {0} : "empty" or "select" item
        /// {0} : language ID
        /// </remarks>
        public const string STATEPROVINCES_BY_COUNTRY_MODEL_KEY = "Nop.pres.stateprovinces.bycountry-{0}-{1}-{2}";
        public const string STATEPROVINCES_PATTERN_KEY = "Nop.pres.stateprovinces";

        /// <summary>
        /// Key for available languages
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// </remarks>
        public const string AVAILABLE_LANGUAGES_MODEL_KEY = "Nop.pres.languages.all-{0}";
        public const string AVAILABLE_LANGUAGES_PATTERN_KEY = "Nop.pres.languages";

        /// <summary>
        /// Key for available currencies
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {0} : current store ID
        /// </remarks>
        public const string AVAILABLE_CURRENCIES_MODEL_KEY = "Nop.pres.currencies.all-{0}-{1}";
        public const string AVAILABLE_CURRENCIES_PATTERN_KEY = "Nop.pres.currencies";

        /// <summary>
        /// Key for caching of a value indicating whether we have checkout attributes
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// {1} : true - all attributes, false - only shippable attributes
        /// </remarks>
        public const string CHECKOUTATTRIBUTES_EXIST_KEY = "Nop.pres.checkoutattributes.exist-{0}-{1}";
        public const string CHECKOUTATTRIBUTES_PATTERN_KEY = "Nop.pres.checkoutattributes";

        /// <summary>
        /// Key for sitemap on the sitemap page
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string SITEMAP_PAGE_MODEL_KEY = "Nop.pres.sitemap.page-{0}-{1}-{2}";
        /// <summary>
        /// Key for sitemap on the sitemap SEO page
        /// </summary>
        /// <remarks>
        /// {0} : language id
        /// {1} : roles of the current user
        /// {2} : current store ID
        /// </remarks>
        public const string SITEMAP_SEO_MODEL_KEY = "Nop.pres.sitemap.seo-{0}-{1}-{2}";
        public const string SITEMAP_PATTERN_KEY = "Nop.pres.sitemap";

        /// <summary>
        /// Key for widget info
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// {1} : widget zone
        /// </remarks>
        public const string WIDGET_MODEL_KEY = "Nop.pres.widget-{0}-{1}";
        public const string WIDGET_PATTERN_KEY = "Nop.pres.widget";

        private readonly ICacheManager _cacheManager;
        
        public ModelCacheEventConsumer()
        {
            //TODO inject static cache manager using constructor
            this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
        }

        //languages
        public void HandleEvent(EntityInserted<Language> eventMessage)
        {
            //clear all localizable models
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_LANGUAGES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<Language> eventMessage)
        {
            //clear all localizable models
          
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_LANGUAGES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<Language> eventMessage)
        {
            //clear all localizable models
         
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_LANGUAGES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);
        }
       

        public void HandleEvent(EntityUpdated<Setting> eventMessage)
        {
            //clear models which depend on settings
           
            _cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY); //depends on CatalogSettings.NumberOfBestsellersOnHomepage
           
            _cacheManager.RemoveByPattern(BLOG_PATTERN_KEY); //depends on BlogSettings.NumberOfTags
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY); //depends on NewsSettings.MainPageNewsCount
            _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY); //depends on distinct sitemap settings
            _cacheManager.RemoveByPattern(WIDGET_PATTERN_KEY); //depends on WidgetSettings and certain settings of widgets

        }

        //Topics
        public void HandleEvent(EntityInserted<Topic> eventMessage)
        {
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<Topic> eventMessage)
        {
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<Topic> eventMessage)
        {
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
       

        //Pictures
        public void HandleEvent(EntityInserted<Picture> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdated<Picture> eventMessage)
        {
          
        }
        public void HandleEvent(EntityDeleted<Picture> eventMessage)
        {
           
        }

       

        //Polls
        public void HandleEvent(EntityInserted<Poll> eventMessage)
        {
            _cacheManager.RemoveByPattern(POLLS_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<Poll> eventMessage)
        {
            _cacheManager.RemoveByPattern(POLLS_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<Poll> eventMessage)
        {
            _cacheManager.RemoveByPattern(POLLS_PATTERN_KEY);
        }
       

        //News items
        public void HandleEvent(EntityInserted<NewsItem> eventMessage)
        {
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<NewsItem> eventMessage)
        {
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<NewsItem> eventMessage)
        {
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
        }
    }
}
