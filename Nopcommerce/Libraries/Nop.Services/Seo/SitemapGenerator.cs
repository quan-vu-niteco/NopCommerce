using System;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;


using Nop.Core.Domain.Common;

using Nop.Core.Domain.News;

using Nop.Services.Topics;

namespace Nop.Services.Seo
{
    /// <summary>
    /// Represents a sitemap generator
    /// </summary>
    public partial class SitemapGenerator : BaseSitemapGenerator, ISitemapGenerator
    {
        private readonly IStoreContext _storeContext;
        
        
       
        private readonly ITopicService _topicService;
        private readonly CommonSettings _commonSettings;
        private readonly NewsSettings _newsSettings;

        public SitemapGenerator(IStoreContext storeContext,
           
            ITopicService topicService,
            CommonSettings commonSettings,
         
            NewsSettings newsSettings
          )
        {
            this._storeContext = storeContext;
          
            this._topicService = topicService;
            this._commonSettings = commonSettings;
            this._newsSettings = newsSettings;
        }

        /// <summary>
        /// Method that is overridden, that handles creation of child urls.
        /// Use the method WriteUrlLocation() within this method.
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        protected override void GenerateUrlNodes(UrlHelper urlHelper)
        {
            //home page
            var homePageUrl = urlHelper.RouteUrl("HomePage", null, "http");
            WriteUrlLocation(homePageUrl, UpdateFrequency.Weekly, DateTime.UtcNow);
            //search products
            var productSearchUrl = urlHelper.RouteUrl("ProductSearch", null, "http");
            WriteUrlLocation(productSearchUrl, UpdateFrequency.Weekly, DateTime.UtcNow);
            //contact us
            var contactUsUrl = urlHelper.RouteUrl("ContactUs", null, "http");
            WriteUrlLocation(contactUsUrl, UpdateFrequency.Weekly, DateTime.UtcNow);
            //news
            if (_newsSettings.Enabled)
            {
                var url = urlHelper.RouteUrl("NewsArchive", null, "http");
                WriteUrlLocation(url, UpdateFrequency.Weekly, DateTime.UtcNow);
            }
            //topics
            WriteTopics(urlHelper);
        }
       

        private void WriteTopics(UrlHelper urlHelper)
        {
            var topics = _topicService.GetAllTopics(_storeContext.CurrentStore.Id)
                .Where(t => t.IncludeInSitemap)
                .ToList();
            foreach (var topic in topics)
            {
                var url = urlHelper.RouteUrl("Topic", new { SeName = topic.GetSeName() }, "http");
                WriteUrlLocation(url, UpdateFrequency.Weekly, DateTime.UtcNow);
            }
        }
    }
}
