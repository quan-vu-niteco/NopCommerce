using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Extensions;
using Nop.Admin.Models.News;
using Nop.Core.Domain.News;
using Nop.Services.News;
using Nop.Services.Customers;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;

using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Controllers
{
    public partial class CatalogueController : BaseAdminController
    {
        #region Fields

        private readonly ICatalogueervice _CatalogueService;      
        private readonly INewsService _newsService;
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IPermissionService _permissionService;
        private readonly IAclService _aclService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IExportManager _exportManager;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly Catalogueettings _Catalogueettings;

        #endregion
        
        #region Constructors

        public CatalogueController(ICatalogueervice CatalogueService,
          INewsService newsService, 
            ICustomerService customerService,
            IUrlRecordService urlRecordService, 
            IPictureService pictureService, 
            ILanguageService languageService,
            ILocalizationService localizationService, 
            ILocalizedEntityService localizedEntityService,
            IPermissionService permissionService,
            IAclService aclService, 
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            IExportManager exportManager,
            ICustomerActivityService customerActivityService,
            Catalogueettings Catalogueettings)
        {
            this._CatalogueService = CatalogueService; 
            this._newsService = newsService;
            this._customerService = customerService;
            this._urlRecordService = urlRecordService;
            this._pictureService = pictureService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._localizedEntityService = localizedEntityService;
            this._permissionService = permissionService;
            this._aclService = aclService;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
            this._exportManager = exportManager;
            this._customerActivityService = customerActivityService;
            this._Catalogueettings = Catalogueettings;
        }

        #endregion
        
        #region Utilities

        [NonAction]
        protected virtual void UpdateLocales(Catalogue catalogue, CatalogueModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(catalogue,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(catalogue,
                                                           x => x.Description,
                                                           localized.Description,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(catalogue,
                                                           x => x.MetaKeywords,
                                                           localized.MetaKeywords,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(catalogue,
                                                           x => x.MetaDescription,
                                                           localized.MetaDescription,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(catalogue,
                                                           x => x.MetaTitle,
                                                           localized.MetaTitle,
                                                           localized.LanguageId);

                //search engine name
                var seName = catalogue.ValidateSeName(localized.SeName, localized.Name, false);
                _urlRecordService.SaveSlug(catalogue, seName, localized.LanguageId);
            }
        }

        [NonAction]
        protected virtual void UpdatePictureSeoNames(Catalogue catalogue)
        {
            var picture = _pictureService.GetPictureById(catalogue.PictureId);
            if (picture != null)
                _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(catalogue.Name));
        }

        [NonAction]
        protected virtual void PrepareAllCatalogueModel(CatalogueModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCatalogue.Add(new SelectListItem
            {
                Text = "[None]",
                Value = "0"
            });
            var Catalogue = _CatalogueService.GetAllCatalogue(showHidden: true);
            foreach (var c in Catalogue)
            {
                model.AvailableCatalogue.Add(new SelectListItem
                {
                    Text = c.GetFormattedBreadCrumb(Catalogue),
                    Value = c.Id.ToString()
                });
            }
        }
      

        [NonAction]
        protected virtual void PrepareAclModel(CatalogueModel model, Catalogue catalogue, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCustomerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Select(cr => cr.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (catalogue != null)
                {
                    model.SelectedCustomerRoleIds = _aclService.GetCustomerRoleIdsWithAccess(catalogue);
                }
            }
        }

        [NonAction]
        protected virtual void SaveCatalogueAcl(Catalogue catalogue, CatalogueModel model)
        {
            var existingAclRecords = _aclService.GetAclRecords(catalogue);
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds != null && model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                        _aclService.InsertAclRecord(catalogue, customerRole.Id);
                }
                else
                {
                    //remove role
                    var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                    if (aclRecordToDelete != null)
                        _aclService.DeleteAclRecord(aclRecordToDelete);
                }
            }
        }


        #endregion
        
        #region List / tree

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogue))
                return AccessDeniedView();

            var model = new CatalogueListModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command, CatalogueListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogue))
                return AccessDeniedView();

            var Catalogue = _CatalogueService.GetAllCatalogue(model.SearchCatalogueName, 
                command.Page - 1, command.PageSize, true);

            var Catalogue1 = _CatalogueService.GetAllCatalogue("", command.Page - 1, command.PageSize, true);




            var gridModel = new DataSourceResult
            {
                Data = Catalogue.Select(x =>
                {
                    var CatalogueModel = x.ToModel();
                    CatalogueModel.Breadcrumb = x.GetFormattedBreadCrumb(_CatalogueService);
                    return CatalogueModel;
                }),
                Total = Catalogue.TotalCount
            };
            return Json(gridModel);
        }
        
        public ActionResult Tree()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogue))
                return AccessDeniedView();

            return View();
        }

        [HttpPost,]
        public ActionResult TreeLoadChildren(int id = 0)
        {
            var Catalogue = _CatalogueService.GetAllCatalogueByParentCatalogueId(id, true)
                .Select(x => new
                             {
                                 id = x.Id,
                                 Name = x.Name,
                                 hasChildren = _CatalogueService.GetAllCatalogueByParentCatalogueId(x.Id, true).Count > 0,
                                 imageUrl = Url.Content("~/Administration/Content/images/ico-content.png")
                             });

            return Json(Catalogue);
        }

        #endregion

        #region Create / Edit / Delete

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogue))
                return AccessDeniedView();

            var model = new CatalogueModel();
            //locales
            AddLocales(_languageService, model.Locales);
          
            //Catalogue
            PrepareAllCatalogueModel(model);
         
            //ACL
            PrepareAclModel(model, null, false);
       
            //default values
            model.PageSize = 4;
            model.Published = true;
            model.IncludeInTopMenu = true;
            model.AllowCustomersToSelectPageSize = true;            
            model.PageSizeOptions = _Catalogueettings.DefaultCataloguePageSizeOptions;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(CatalogueModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogue))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var Catalogue = model.ToEntity();
                Catalogue.CreatedOnUtc = DateTime.UtcNow;
                Catalogue.UpdatedOnUtc = DateTime.UtcNow;
                _CatalogueService.InsertCatalogue(Catalogue);
                //search engine name
                model.SeName = Catalogue.ValidateSeName(model.SeName, Catalogue.Name, true);
                _urlRecordService.SaveSlug(Catalogue, model.SeName, 0);
                //locales
                UpdateLocales(Catalogue, model);
              
                //update picture seo file name
                UpdatePictureSeoNames(Catalogue);
                //ACL (customer roles)
                SaveCatalogueAcl(Catalogue, model);
            

                //activity log
                _customerActivityService.InsertActivity("AddNewCatalogue", _localizationService.GetResource("ActivityLog.AddNewCatalogue"), Catalogue.Name);

                SuccessNotification(_localizationService.GetResource("Admin.News.Catalogue.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = Catalogue.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
      
            //Catalogue
            PrepareAllCatalogueModel(model);
     
            //ACL
            PrepareAclModel(model, null, true);
        
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogue))
                return AccessDeniedView();

            var Catalogue = _CatalogueService.GetCatalogueById(id);
            if (Catalogue == null || Catalogue.Deleted) 
                //No Catalogue found with the specified id
                return RedirectToAction("List");

            var model = Catalogue.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = Catalogue.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = Catalogue.GetLocalized(x => x.Description, languageId, false, false);
                locale.MetaKeywords = Catalogue.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = Catalogue.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = Catalogue.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = Catalogue.GetSeName(languageId, false, false);
            });
         
            //Catalogue
            PrepareAllCatalogueModel(model);        
            //ACL
            PrepareAclModel(model, Catalogue, false);   
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(CatalogueModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogue))
                return AccessDeniedView();

            var Catalogue = _CatalogueService.GetCatalogueById(model.Id);
            if (Catalogue == null || Catalogue.Deleted)
                //No Catalogue found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                int prevPictureId = Catalogue.PictureId;
                Catalogue = model.ToEntity(Catalogue);
                Catalogue.UpdatedOnUtc = DateTime.UtcNow;
                _CatalogueService.UpdateCatalogue(Catalogue);
                //search engine name
                model.SeName = Catalogue.ValidateSeName(model.SeName, Catalogue.Name, true);
                _urlRecordService.SaveSlug(Catalogue, model.SeName, 0);
                //locales
                UpdateLocales(Catalogue, model);
               
                _CatalogueService.UpdateCatalogue(Catalogue);
              
                //delete an old picture (if deleted or updated)
                if (prevPictureId > 0 && prevPictureId != Catalogue.PictureId)
                {
                    var prevPicture = _pictureService.GetPictureById(prevPictureId);
                    if (prevPicture != null)
                        _pictureService.DeletePicture(prevPicture);
                }
                //update picture seo file name
                UpdatePictureSeoNames(Catalogue);
                //ACL
                SaveCatalogueAcl(Catalogue, model);  

                //activity log
                _customerActivityService.InsertActivity("EditCatalogue", _localizationService.GetResource("ActivityLog.EditCatalogue"), Catalogue.Name);

                SuccessNotification(_localizationService.GetResource("Admin.News.Catalogue.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", Catalogue.Id);
                }
                return RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
          
            //Catalogue
            PrepareAllCatalogueModel(model);        
            //ACL
            PrepareAclModel(model, Catalogue, true);         

            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogue))
                return AccessDeniedView();

            var Catalogue = _CatalogueService.GetCatalogueById(id);
            if (Catalogue == null)
                //No Catalogue found with the specified id
                return RedirectToAction("List");

            _CatalogueService.DeleteCatalogue(Catalogue);

            //activity log
            _customerActivityService.InsertActivity("DeleteCatalogue", _localizationService.GetResource("ActivityLog.DeleteCatalogue"), Catalogue.Name);

            SuccessNotification(_localizationService.GetResource("Admin.News.Catalogue.Deleted"));
            return RedirectToAction("List");
        }
        

        #endregion       

        #region News

        [HttpPost]
        public ActionResult NewsList(DataSourceRequest command, int catalogueId)
        {
            
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogue))
                return AccessDeniedView();

            var newsCatalogue = _CatalogueService.GetNewsCatalogueByCatalogueId(catalogueId,
                command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = newsCatalogue.Select(x => new CatalogueModel.CatalogueNewsModel
                {
                    Id = x.Id,
                    CatalogueId = x.CatalogueId,
                    NewsId = x.NewsId,
                    NewsTitle = _newsService.GetNewsById(x.NewsId).Title,
                    IsFeaturedNews = x.IsFeaturedNews,
                    DisplayOrder = x.DisplayOrder
                }),
                Total = newsCatalogue.TotalCount
            };

            return Json(gridModel);
        }

        public ActionResult NewsUpdate(CatalogueModel.CatalogueNewsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogue))
                return AccessDeniedView();

            var newsCatalogue = _CatalogueService.GetNewsCatalogueById(model.Id);
            if (newsCatalogue == null)
                throw new ArgumentException("No news Catalogue mapping found with the specified id");

            newsCatalogue.IsFeaturedNews = model.IsFeaturedNews;
            newsCatalogue.DisplayOrder = model.DisplayOrder;
            _CatalogueService.UpdateNewsCatalogue(newsCatalogue);

            return new NullJsonResult();
        }

        public ActionResult NewsDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogue))
                return AccessDeniedView();

            var newsCatalogue = _CatalogueService.GetNewsCatalogueById(id);
            if (newsCatalogue == null)
                throw new ArgumentException("No news Catalogue mapping found with the specified id");

            //var CatalogueId = newsCatalogue.CatalogueId;
            _CatalogueService.DeleteNewsCatalogue(newsCatalogue);

            return new NullJsonResult();
        }

        public ActionResult NewsAddPopup(int catalogueId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogue))
                return AccessDeniedView();
            
            var model = new CatalogueModel.AddCatalogueNewsModel();
            //Catalogue
            model.AvailableCatalogue.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var Catalogue = _CatalogueService.GetAllCatalogue(showHidden: true);
            foreach (var c in Catalogue)
                model.AvailableCatalogue.Add(new SelectListItem { Text = c.GetFormattedBreadCrumb(Catalogue), Value = c.Id.ToString() });

            //news types
            model.AvailableNewsTypes = NewsType.SimpleNews.ToSelectList(false).ToList();
            model.AvailableNewsTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public ActionResult NewsAddPopupList(DataSourceRequest command, CatalogueModel.AddCatalogueNewsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogue))
                return AccessDeniedView();

            var gridModel = new DataSourceResult();
            var news = _newsService.SearchNews(               
                keywords: model.SearchNewsTitle,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true
                );
            gridModel.Data = news.Select(x => x.ToModel());
            gridModel.Total = news.TotalCount;

            return Json(gridModel);
        }
        
        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult NewsAddPopup(string btnId, string formId, CatalogueModel.AddCatalogueNewsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogue))
                return AccessDeniedView();

            if (model.SelectedNewsIds != null)
            {
                foreach (int id in model.SelectedNewsIds)
                {
                    var news = _newsService.GetNewsById(id);
                    if (news != null)
                    {
                        var existingNewsCatalogue = _CatalogueService.GetNewsCatalogueByCatalogueId(model.CatalogueId, 0, int.MaxValue, true);
                        if (existingNewsCatalogue.FindNewsCatalogue(id, model.CatalogueId) == null)
                        {
                            _CatalogueService.InsertNewsCatalogue(
                                new NewsCatalogue
                                {
                                    CatalogueId = model.CatalogueId,
                                    NewsId = id,
                                    IsFeaturedNews = false,
                                    DisplayOrder = 1
                                });
                        }
                    }
                }
            }

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            return View(model);
        }

        #endregion
    }
}
