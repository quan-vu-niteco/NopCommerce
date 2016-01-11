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
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Controllers
{
    public partial class CataloguesController : BaseAdminController
    {
        #region Fields

        private readonly ICataloguesService _cataloguesService;      
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
        private readonly IVendorService _vendorService;
        private readonly CataloguesSettings _cataloguesSettings;

        #endregion
        
        #region Constructors

        public CataloguesController(ICataloguesService cataloguesService,
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
            IVendorService vendorService, 
            ICustomerActivityService customerActivityService,
            CataloguesSettings cataloguesSettings)
        {
            this._cataloguesService = cataloguesService; 
            this._newsService = newsService;
            this._customerService = customerService;
            this._urlRecordService = urlRecordService;
            this._pictureService = pictureService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._localizedEntityService = localizedEntityService;
            this._permissionService = permissionService;
            this._vendorService = vendorService;
            this._aclService = aclService;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
            this._exportManager = exportManager;
            this._customerActivityService = customerActivityService;
            this._cataloguesSettings = cataloguesSettings;
        }

        #endregion
        
        #region Utilities

        [NonAction]
        protected virtual void UpdateLocales(Catalogues catalogues, CataloguesModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(catalogues,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(catalogues,
                                                           x => x.Description,
                                                           localized.Description,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(catalogues,
                                                           x => x.MetaKeywords,
                                                           localized.MetaKeywords,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(catalogues,
                                                           x => x.MetaDescription,
                                                           localized.MetaDescription,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(catalogues,
                                                           x => x.MetaTitle,
                                                           localized.MetaTitle,
                                                           localized.LanguageId);

                //search engine name
                var seName = catalogues.ValidateSeName(localized.SeName, localized.Name, false);
                _urlRecordService.SaveSlug(catalogues, seName, localized.LanguageId);
            }
        }

        [NonAction]
        protected virtual void UpdatePictureSeoNames(Catalogues catalogues)
        {
            var picture = _pictureService.GetPictureById(catalogues.PictureId);
            if (picture != null)
                _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(catalogues.Name));
        }

        [NonAction]
        protected virtual void PrepareAllCataloguesModel(CataloguesModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCatalogues.Add(new SelectListItem
            {
                Text = "[None]",
                Value = "0"
            });
            var catalogues = _cataloguesService.GetAllCatalogues(showHidden: true);
            foreach (var c in catalogues)
            {
                model.AvailableCatalogues.Add(new SelectListItem
                {
                    Text = c.GetFormattedBreadCrumb(catalogues),
                    Value = c.Id.ToString()
                });
            }
        }
      

        [NonAction]
        protected virtual void PrepareAclModel(CataloguesModel model, Catalogues catalogues, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCustomerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Select(cr => cr.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (catalogues != null)
                {
                    model.SelectedCustomerRoleIds = _aclService.GetCustomerRoleIdsWithAccess(catalogues);
                }
            }
        }

        [NonAction]
        protected virtual void SaveCataloguesAcl(Catalogues catalogues, CataloguesModel model)
        {
            var existingAclRecords = _aclService.GetAclRecords(catalogues);
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds != null && model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                        _aclService.InsertAclRecord(catalogues, customerRole.Id);
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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogues))
                return AccessDeniedView();

            var model = new CataloguesListModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command, CataloguesListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogues))
                return AccessDeniedView();

            var catalogues = _cataloguesService.GetAllCatalogues(model.SearchCataloguesName, 
                command.Page - 1, command.PageSize, true);

            var catalogues1 = _cataloguesService.GetAllCatalogues("", command.Page - 1, command.PageSize, true);




            var gridModel = new DataSourceResult
            {
                Data = catalogues.Select(x =>
                {
                    var cataloguesModel = x.ToModel();
                    cataloguesModel.Breadcrumb = x.GetFormattedBreadCrumb(_cataloguesService);
                    return cataloguesModel;
                }),
                Total = catalogues.TotalCount
            };
            return Json(gridModel);
        }
        
        public ActionResult Tree()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogues))
                return AccessDeniedView();

            return View();
        }

        [HttpPost,]
        public ActionResult TreeLoadChildren(int id = 0)
        {
            var catalogues = _cataloguesService.GetAllCataloguesByParentCatalogueId(id, true)
                .Select(x => new
                             {
                                 id = x.Id,
                                 Name = x.Name,
                                 hasChildren = _cataloguesService.GetAllCataloguesByParentCatalogueId(x.Id, true).Count > 0,
                                 imageUrl = Url.Content("~/Administration/Content/images/ico-content.png")
                             });

            return Json(catalogues);
        }

        #endregion

        #region Create / Edit / Delete

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogues))
                return AccessDeniedView();

            var model = new CataloguesModel();
            //locales
            AddLocales(_languageService, model.Locales);
          
            //catalogues
            PrepareAllCataloguesModel(model);
         
            //ACL
            PrepareAclModel(model, null, false);
       
            //default values
            model.PageSize = 4;
            model.Published = true;
            model.IncludeInTopMenu = true;
            model.AllowCustomersToSelectPageSize = true;            
            model.PageSizeOptions = _cataloguesSettings.DefaultCataloguesPageSizeOptions;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(CataloguesModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogues))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var catalogues = model.ToEntity();
                catalogues.CreatedOnUtc = DateTime.UtcNow;
                catalogues.UpdatedOnUtc = DateTime.UtcNow;
                _cataloguesService.InsertCatalogues(catalogues);
                //search engine name
                model.SeName = catalogues.ValidateSeName(model.SeName, catalogues.Name, true);
                _urlRecordService.SaveSlug(catalogues, model.SeName, 0);
                //locales
                UpdateLocales(catalogues, model);
              
                //update picture seo file name
                UpdatePictureSeoNames(catalogues);
                //ACL (customer roles)
                SaveCataloguesAcl(catalogues, model);
            

                //activity log
                _customerActivityService.InsertActivity("AddNewCatalogues", _localizationService.GetResource("ActivityLog.AddNewCatalogues"), catalogues.Name);

                SuccessNotification(_localizationService.GetResource("Admin.News.Catalogues.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = catalogues.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
      
            //catalogues
            PrepareAllCataloguesModel(model);
     
            //ACL
            PrepareAclModel(model, null, true);
        
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogues))
                return AccessDeniedView();

            var catalogues = _cataloguesService.GetCatalogueById(id);
            if (catalogues == null || catalogues.Deleted) 
                //No catalogues found with the specified id
                return RedirectToAction("List");

            var model = catalogues.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = catalogues.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = catalogues.GetLocalized(x => x.Description, languageId, false, false);
                locale.MetaKeywords = catalogues.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = catalogues.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = catalogues.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = catalogues.GetSeName(languageId, false, false);
            });
         
            //catalogues
            PrepareAllCataloguesModel(model);        
            //ACL
            PrepareAclModel(model, catalogues, false);   
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(CataloguesModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogues))
                return AccessDeniedView();

            var catalogues = _cataloguesService.GetCatalogueById(model.Id);
            if (catalogues == null || catalogues.Deleted)
                //No catalogues found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                int prevPictureId = catalogues.PictureId;
                catalogues = model.ToEntity(catalogues);
                catalogues.UpdatedOnUtc = DateTime.UtcNow;
                _cataloguesService.UpdateCatalogues(catalogues);
                //search engine name
                model.SeName = catalogues.ValidateSeName(model.SeName, catalogues.Name, true);
                _urlRecordService.SaveSlug(catalogues, model.SeName, 0);
                //locales
                UpdateLocales(catalogues, model);
               
                _cataloguesService.UpdateCatalogues(catalogues);
              
                //delete an old picture (if deleted or updated)
                if (prevPictureId > 0 && prevPictureId != catalogues.PictureId)
                {
                    var prevPicture = _pictureService.GetPictureById(prevPictureId);
                    if (prevPicture != null)
                        _pictureService.DeletePicture(prevPicture);
                }
                //update picture seo file name
                UpdatePictureSeoNames(catalogues);
                //ACL
                SaveCataloguesAcl(catalogues, model);  

                //activity log
                _customerActivityService.InsertActivity("EditCatalogues", _localizationService.GetResource("ActivityLog.EditCatalogues"), catalogues.Name);

                SuccessNotification(_localizationService.GetResource("Admin.News.Catalogues.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", catalogues.Id);
                }
                return RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
          
            //catalogues
            PrepareAllCataloguesModel(model);        
            //ACL
            PrepareAclModel(model, catalogues, true);         

            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogues))
                return AccessDeniedView();

            var catalogues = _cataloguesService.GetCatalogueById(id);
            if (catalogues == null)
                //No catalogues found with the specified id
                return RedirectToAction("List");

            _cataloguesService.DeleteCatalogues(catalogues);

            //activity log
            _customerActivityService.InsertActivity("DeleteCatalogues", _localizationService.GetResource("ActivityLog.DeleteCatalogues"), catalogues.Name);

            SuccessNotification(_localizationService.GetResource("Admin.News.Catalogues.Deleted"));
            return RedirectToAction("List");
        }
        

        #endregion       

        #region News

        [HttpPost]
        public ActionResult NewsList(DataSourceRequest command, int catalogueId)
        {
            
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogues))
                return AccessDeniedView();

            var newsCatalogues = _cataloguesService.GetNewsCataloguesByCatalogueId(catalogueId,
                command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = newsCatalogues.Select(x => new CataloguesModel.CataloguesNewsModel
                {
                    Id = x.Id,
                    CatalogueId = x.CatalogueId,
                    NewsId = x.NewsId,
                    NewsTitle = _newsService.GetNewsById(x.NewsId).Title,
                    IsFeaturedNews = x.IsFeaturedNews,
                    DisplayOrder = x.DisplayOrder
                }),
                Total = newsCatalogues.TotalCount
            };

            return Json(gridModel);
        }

        public ActionResult NewsUpdate(CataloguesModel.CataloguesNewsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogues))
                return AccessDeniedView();

            var newsCatalogues = _cataloguesService.GetNewsCataloguesById(model.Id);
            if (newsCatalogues == null)
                throw new ArgumentException("No news catalogues mapping found with the specified id");

            newsCatalogues.IsFeaturedNews = model.IsFeaturedNews;
            newsCatalogues.DisplayOrder = model.DisplayOrder;
            _cataloguesService.UpdateNewsCatalogues(newsCatalogues);

            return new NullJsonResult();
        }

        public ActionResult NewsDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogues))
                return AccessDeniedView();

            var newsCatalogues = _cataloguesService.GetNewsCataloguesById(id);
            if (newsCatalogues == null)
                throw new ArgumentException("No news catalogues mapping found with the specified id");

            //var CatalogueId = newsCatalogues.CatalogueId;
            _cataloguesService.DeleteNewsCatalogues(newsCatalogues);

            return new NullJsonResult();
        }

        public ActionResult NewsAddPopup(int catalogueId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogues))
                return AccessDeniedView();
            
            var model = new CataloguesModel.AddCataloguesNewsModel();
            //catalogues
            model.AvailableCatalogues.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var catalogues = _cataloguesService.GetAllCatalogues(showHidden: true);
            foreach (var c in catalogues)
                model.AvailableCatalogues.Add(new SelectListItem { Text = c.GetFormattedBreadCrumb(catalogues), Value = c.Id.ToString() });
       

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //news types
            model.AvailableNewsTypes = NewsType.SimpleNews.ToSelectList(false).ToList();
            model.AvailableNewsTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public ActionResult NewsAddPopupList(DataSourceRequest command, CataloguesModel.AddCataloguesNewsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogues))
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
        public ActionResult NewsAddPopup(string btnId, string formId, CataloguesModel.AddCataloguesNewsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalogues))
                return AccessDeniedView();

            if (model.SelectedNewsIds != null)
            {
                foreach (int id in model.SelectedNewsIds)
                {
                    var news = _newsService.GetNewsById(id);
                    if (news != null)
                    {
                        var existingNewsCatalogues = _cataloguesService.GetNewsCataloguesByCatalogueId(model.CatalogueId, 0, int.MaxValue, true);
                        if (existingNewsCatalogues.FindNewsCatalogues(id, model.CatalogueId) == null)
                        {
                            _cataloguesService.InsertNewsCatalogues(
                                new NewsCatalogues
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
