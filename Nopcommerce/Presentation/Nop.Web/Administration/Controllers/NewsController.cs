using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Extensions;
using Nop.Admin.Models.News;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.News;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.News;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Services.Media;
using Nop.Core;

namespace Nop.Admin.Controllers
{
    public partial class NewsController : BaseAdminController
	{
		#region Fields
        private readonly IWorkContext _workContext;
        private readonly INewsService _newsService;    
        private readonly ILanguageService _languageService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPictureService _pictureService;
        private readonly ICatalogueervice _Catalogueervice;
        
		#endregion

		#region Constructors

        public NewsController(INewsService newsService, 
            ILanguageService languageService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IUrlRecordService urlRecordService,
            IStoreService storeService,
            IStoreMappingService storeMappingService, IPictureService pictureService, ICatalogueervice CatalogueService,IWorkContext workContext )
        {
            this._newsService = newsService;
            this._languageService = languageService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._urlRecordService = urlRecordService;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
            this._pictureService = pictureService;
            this._Catalogueervice = CatalogueService;
            this._workContext = workContext;
		}

		#endregion 

        #region Utilities

        [NonAction]
        protected virtual void PrepareStoresMappingModel(NewsItemModel model, NewsItem newsItem, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableStores = _storeService
                .GetAllStores()
                .Select(s => s.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (newsItem != null)
                {
                    model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(newsItem);
                }
            }
        }

        [NonAction]
        protected virtual void SaveStoreMappings(NewsItem newsItem, NewsItemModel model)
        {
            var existingStoreMappings = _storeMappingService.GetStoreMappings(newsItem);
            var allStores = _storeService.GetAllStores();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds != null && model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        _storeMappingService.InsertStoreMapping(newsItem, store.Id);
                }
                else
                {
                    //remove store
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                        _storeMappingService.DeleteStoreMapping(storeMappingToDelete);
                }
            }
        }



        [NonAction]
        protected virtual void PrepareNewsModel(NewsItemModel model, NewsItem newsItem)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (newsItem != null)
            {
                var allCategories = _Catalogueervice.GetAllCatalogue(showHidden: true);
                foreach (var category in allCategories)
                {
                    model.AvailableCatalogue.Add(new SelectListItem
                    {
                        Text = category.Name,
                        Value = category.Id.ToString()
                    });
                }
            }
        }

        #endregion

        #region News items

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var model = new NewsItemListModel();
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            return View(model);
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command, NewsItemListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var news = _newsService.GetAllNews(_workContext.WorkingLanguage.Id, model.SearchStoreId, command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = news.Select(x =>
                {
                    var m = x.ToModel();
                    if (x.StartDateUtc.HasValue)
                        m.StartDate = _dateTimeHelper.ConvertToUserTime(x.StartDateUtc.Value, DateTimeKind.Utc);
                    if (x.EndDateUtc.HasValue)
                        m.EndDate = _dateTimeHelper.ConvertToUserTime(x.EndDateUtc.Value, DateTimeKind.Utc);
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    m.LanguageName = x.Language.Name;
                    m.Comments = x.CommentCount;
                    return m;
                }),
                Total = news.TotalCount
            };

            return Json(gridModel);
        }

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = new NewsItemModel();
            //Stores
            PrepareStoresMappingModel(model, null, false);
            //default values
            model.Published = true;
            model.AllowComments = true;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(NewsItemModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var newsItem = model.ToEntity();
                newsItem.StartDateUtc = model.StartDate;
                newsItem.EndDateUtc = model.EndDate;
                newsItem.CreatedOnUtc = DateTime.UtcNow;
                _newsService.InsertNews(newsItem);
                
                //search engine name
                var seName = newsItem.ValidateSeName(model.SeName, model.Title, true);
                _urlRecordService.SaveSlug(newsItem, seName, newsItem.LanguageId);

                //Stores
                SaveStoreMappings(newsItem, model);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.News.NewsItems.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = newsItem.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            //Stores
            PrepareStoresMappingModel(model, null, true);
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsItem = _newsService.GetNewsById(id);
            if (newsItem == null)
                //No news item found with the specified id
                return RedirectToAction("List");

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = newsItem.ToModel();
            PrepareNewsModel(model, newsItem);

            model.StartDate = newsItem.StartDateUtc;
            model.EndDate = newsItem.EndDateUtc;
            //Store
            PrepareStoresMappingModel(model, newsItem, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(NewsItemModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsItem = _newsService.GetNewsById(model.Id);
            if (newsItem == null)
                //No news item found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                newsItem = model.ToEntity(newsItem);
                newsItem.StartDateUtc = model.StartDate;
                newsItem.EndDateUtc = model.EndDate;
                _newsService.UpdateNews(newsItem);

                //search engine name
                var seName = newsItem.ValidateSeName(model.SeName, model.Title, true);
                _urlRecordService.SaveSlug(newsItem, seName, newsItem.LanguageId);

                //Stores
                SaveStoreMappings(newsItem, model);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.News.NewsItems.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new {id = newsItem.Id});
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            //Store
            PrepareStoresMappingModel(model, newsItem, true);
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsItem = _newsService.GetNewsById(id);
            if (newsItem == null)
                //No news item found with the specified id
                return RedirectToAction("List");

            _newsService.DeleteNews(newsItem);

            SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.News.NewsItems.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region News categories

        [HttpPost]
        public ActionResult NewsCatalogueList(DataSourceRequest command, int newsId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsCatalogue = _Catalogueervice.GetNewsCatalogueByNewsId(newsId, true);
            var newsCatalogueModel = newsCatalogue
                .Select(x => new NewsItemModel.NewsCatalogueModel
                {
                    Id = x.Id,
                    Catalogue = _Catalogueervice.GetCatalogueById(x.CatalogueId).GetFormattedBreadCrumb(_Catalogueervice),
                    NewsId = x.NewsId,
                    CatalogueId = x.CatalogueId,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = newsCatalogueModel,
                Total = newsCatalogueModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult NewsCatalogueInsert(NewsItemModel.NewsCatalogueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsId = model.NewsId;
            var catalogueId = model.CatalogueId;

            var existingNewsCatalogue = _Catalogueervice.GetNewsCatalogueByCatalogueId(catalogueId, 0, int.MaxValue, true);
            if (existingNewsCatalogue.FindNewsCatalogue(newsId, catalogueId) == null)
            {
                var newsCatalogue = new NewsCatalogue
                {
                    NewsId = newsId,
                    CatalogueId = catalogueId,
                    DisplayOrder = model.DisplayOrder
                };

                _Catalogueervice.InsertNewsCatalogue(newsCatalogue);
            }

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult NewsCatalogueUpdate(NewsItemModel.NewsCatalogueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsCatalogue = _Catalogueervice.GetNewsCatalogueById(model.Id);
            if (newsCatalogue == null)
                throw new ArgumentException("No news catalogue mapping found with the specified id");


            newsCatalogue.CatalogueId = model.CatalogueId;
            newsCatalogue.DisplayOrder = model.DisplayOrder;
           
            _Catalogueervice.UpdateNewsCatalogue(newsCatalogue);
            
            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult NewsCatalogueDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsCatalogue = _Catalogueervice.GetNewsCatalogueById(id);
            if (newsCatalogue == null)
                throw new ArgumentException("No news catalogue mapping found with the specified id");

            _Catalogueervice.DeleteNewsCatalogue(newsCatalogue);

            return new NullJsonResult();
        }

        #endregion

        #region News pictures

        public ActionResult NewsPictureAdd(int pictureId, int displayOrder, int newsId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            if (pictureId == 0)
                throw new ArgumentException();

            var news = _newsService.GetNewsById(newsId);
            if (news == null)
                throw new ArgumentException("No news found with the specified id");

            _newsService.InsertNewsPicture(new NewsPicture
            {
                PictureId = pictureId,
                NewsId = newsId,
                DisplayOrder = displayOrder,
            });

            _pictureService.SetSeoFilename(pictureId, _pictureService.GetPictureSeName(news.Title));

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult NewsPictureList(DataSourceRequest command, int newsId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();
          

            var newsPictures = _newsService.GetNewsPicturesByNewsId(newsId);
            var newsPicturesModel = newsPictures
                .Select(x => new NewsItemModel.NewsPictureModel
                {
                    Id = x.Id,
                    NewsId = x.NewsId,
                    PictureId = x.PictureId,
                    PictureUrl = _pictureService.GetPictureUrl(x.PictureId),
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = newsPicturesModel,
                Total = newsPicturesModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult NewsPictureUpdate(NewsItemModel.NewsPictureModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsPicture = _newsService.GetNewsPictureById(model.Id);
            if (newsPicture == null)
                throw new ArgumentException("No news picture found with the specified id");
         

            newsPicture.DisplayOrder = model.DisplayOrder;
            _newsService.UpdateNewsPicture(newsPicture);

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult NewsPictureDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsPicture = _newsService.GetNewsPictureById(id);
            if (newsPicture == null)
                throw new ArgumentException("No news picture found with the specified id");

            var newsId = newsPicture.NewsId;

            var pictureId = newsPicture.PictureId;
            _newsService.DeleteNewsPicture(newsPicture);
            var picture = _pictureService.GetPictureById(pictureId);
            _pictureService.DeletePicture(picture);

            return new NullJsonResult();
        }

        #endregion
        
        #region Comments

        public ActionResult Comments(int? filterByNewsItemId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            ViewBag.FilterByNewsItemId = filterByNewsItemId;
            return View();
        }

        [HttpPost]
        public ActionResult Comments(int? filterByNewsItemId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            IList<NewsComment> comments;
            if (filterByNewsItemId.HasValue)
            {
                //filter comments by news item
                var newsItem = _newsService.GetNewsById(filterByNewsItemId.Value);
                comments = newsItem.NewsComments.OrderBy(bc => bc.CreatedOnUtc).ToList();
            }
            else
            {
                //load all news comments
                comments = _newsService.GetAllComments(0);
            }

            var gridModel = new DataSourceResult
            {
                Data = comments.PagedForCommand(command).Select(newsComment =>
                {
                    var commentModel = new NewsCommentModel();
                    commentModel.Id = newsComment.Id;
                    commentModel.NewsItemId = newsComment.NewsItemId;
                    commentModel.NewsItemTitle = newsComment.NewsItem.Title;
                    commentModel.CustomerId = newsComment.CustomerId;
                    var customer = newsComment.Customer;
                    commentModel.CustomerInfo = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
                    commentModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(newsComment.CreatedOnUtc, DateTimeKind.Utc);
                    commentModel.CommentTitle = newsComment.CommentTitle;
                    commentModel.CommentText = Core.Html.HtmlHelper.FormatText(newsComment.CommentText, false, true, false, false, false, false);
                    return commentModel;
                }),
                Total = comments.Count,
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult CommentDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var comment = _newsService.GetNewsCommentById(id);
            if (comment == null)
                throw new ArgumentException("No comment found with the specified id");

            var newsItem = comment.NewsItem;
            _newsService.DeleteNewsComment(comment);
            //update totals
            newsItem.CommentCount = newsItem.NewsComments.Count;
            _newsService.UpdateNews(newsItem);

            return new NullJsonResult();
        }


        #endregion
    }
}
