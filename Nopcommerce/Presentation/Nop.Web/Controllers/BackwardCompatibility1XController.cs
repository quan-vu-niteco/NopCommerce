using System;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Nop.Services.Customers;
using Nop.Services.News;
using Nop.Services.Seo;
using Nop.Services.Topics;

namespace Nop.Web.Controllers
{
    public partial class BackwardCompatibility1XController : BasePublicController
    {
		#region Fields

      
        private readonly INewsService _newsService;
        private readonly ITopicService _topicService;
        private readonly ICustomerService _customerService;
        #endregion

		#region Constructors

        public BackwardCompatibility1XController(
            INewsService newsService,
            ITopicService topicService,
            ICustomerService customerService)
        {
          
            this._newsService = newsService;
            this._topicService = topicService;
            this._customerService = customerService;
        }

		#endregion
        
        #region Methods

        public ActionResult GeneralRedirect()
        {
            
            // use Request.RawUrl, for instance to parse out what was invoked
            // this regex will extract anything between a "/" and a ".aspx"
            var regex = new Regex(@"(?<=/).+(?=\.aspx)", RegexOptions.Compiled);
            var aspxfileName = regex.Match(Request.RawUrl).Value.ToLowerInvariant();


            switch (aspxfileName)
            {
                //URL without rewriting
                case "news":
                    {
                        return RedirectNewsItem(Request.QueryString["newsid"], false);
                    }
                case "topic":
                    {
                        return RedirectTopic(Request.QueryString["topicid"], false);
                    }
                case "profile":
                    {
                        return RedirectUserProfile(Request.QueryString["UserId"]);
                    }
                case "contactus":
                    {
                        return RedirectToRoutePermanent("ContactUs");
                    }
                case "passwordrecovery":
                    {
                        return RedirectToRoutePermanent("PasswordRecovery");
                    }
                case "login":
                    {
                        return RedirectToRoutePermanent("Login");
                    }
                case "register":
                    {
                        return RedirectToRoutePermanent("Register");
                    }
                case "newsarchive":
                    {
                        return RedirectToRoutePermanent("NewsArchive");
                    }
                case "search":
                    {
                        return RedirectToRoutePermanent("ProductSearch");
                    }
                case "sitemap":
                    {
                        return RedirectToRoutePermanent("Sitemap");
                    }
                default:
                    break;
            }

            //no permanent redirect in this case
            return RedirectToRoute("HomePage");
        }

        public ActionResult RedirectNewsItem(string id, bool idIncludesSename = true)
        {
            //we can't use dash in MVC
            var newsId = idIncludesSename ? Convert.ToInt32(id.Split(new [] { '-' })[0]) : Convert.ToInt32(id);
            var newsItem = _newsService.GetNewsById(newsId);
            if (newsItem == null)
                return RedirectToRoutePermanent("HomePage");

            return RedirectToRoutePermanent("NewsItem", new { newsItemId = newsItem.Id, SeName = newsItem.GetSeName(newsItem.LanguageId, ensureTwoPublishedLanguages: false) });
        }

        public ActionResult RedirectTopic(string id, bool idIncludesSename = true)
        {
            //we can't use dash in MVC
            var topicid = idIncludesSename ? Convert.ToInt32(id.Split(new [] { '-' })[0]) : Convert.ToInt32(id);
            var topic = _topicService.GetTopicById(topicid);
            if (topic == null)
                return RedirectToRoutePermanent("HomePage");

            return RedirectToRoutePermanent("Topic", new { SeName = topic.GetSeName() });
        }

        public ActionResult RedirectUserProfile(string id)
        {
            //we can't use dash in MVC
            var userId = Convert.ToInt32(id);
            var user = _customerService.GetCustomerById(userId);
            if (user == null)
                return RedirectToRoutePermanent("HomePage");

            return RedirectToRoutePermanent("CustomerProfile", new { id = user.Id});
        }
        
        #endregion
    }
}
