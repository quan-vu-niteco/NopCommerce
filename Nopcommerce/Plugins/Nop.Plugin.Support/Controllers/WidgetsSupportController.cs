using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Plugin.Widgets.Support.Services;
using Nop.Plugin.Widgets.Support.Models;
using Nop.Plugin.Widgets.Support.Domain;

namespace Nop.Plugin.Widgets.Support.Controllers
{
    [AdminAuthorize]
    public class WidgetsSupportController : BasePluginController
    {
        
		#region Fields
        private readonly IWorkContext _workContext;
        private readonly ISupportService _supportService;
        private readonly IPermissionService _permissionService;
		#endregion

		#region Constructors

        public WidgetsSupportController(ISupportService supportService,IPermissionService permissionService,IWorkContext workContext)
		{

            this._workContext = workContext;
            this._supportService = supportService;
            this._permissionService = permissionService;
		}

		#endregion 

        [ChildActionOnly]
        public ActionResult Configure()
        {
            return View("~/Plugins/Widgets.Support/Views/WidgetsSupport/Configure.cshtml");
        }

        [ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone, object additionalData = null)
        {
            return View("~/Plugins/Widgets.Support/Views/WidgetsSupport/PublicInfo.cshtml");
        }

        [ChildActionOnly]
        public ActionResult PublicInfoTop(string widgetZone, object additionalData = null)
        {
            return View("~/Plugins/Widgets.Support/Views/WidgetsSupport/PublicInfoTop.cshtml");
        }
        [ChildActionOnly]
        public ActionResult PublicInfoBottom(string widgetZone, object additionalData = null)
        {
            return View("~/Plugins/Widgets.Support/Views/WidgetsSupport/PublicInfoBottom.cshtml");
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return Content("Access denied");

            var records = _supportService.GetAll(_workContext.WorkingLanguage.Id, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = records.Select(x =>
                    new SupportItemModel
                    {
                        Id = x.Id,
                        YahooId = x.YahooId,
                        SkypeId = x.SkypeId,
                        Name = x.Name,
                        Phone = x.Phone,
                        HotLine = x.HotLine,
                        DisplayOrder = x.DisplayOrder,
                        Image = x.Image
                    }).ToList(),
                Total = records.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult Update(SupportItemModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return Content("Access denied");

            var support = _supportService.GetById(model.Id);
            support.Id = model.Id;
            support.YahooId = model.YahooId;
            support.SkypeId = model.SkypeId;
            support.Name = model.Name;
            support.Phone = model.Phone;
            support.HotLine = model.HotLine;
            support.DisplayOrder = model.DisplayOrder;
            support.Image = model.Image;
            _supportService.Update(support);
            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult Add(SupportItemModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return Content("Access denied");
            var support = new SupportItem
            {
                LanguageId = _workContext.WorkingLanguage.Id,
                Id = model.Id,
                YahooId = model.YahooId,
                SkypeId = model.SkypeId,
                Name = model.Name,
                Phone = model.Phone,
                HotLine = model.HotLine,
                DisplayOrder = model.DisplayOrder,
                Image = model.Image

            };
            _supportService.Insert(support);
            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return Content("Access denied");


            var support = _supportService.GetById(id);
            if (support != null)
                _supportService.Delete(support);
            return new NullJsonResult();
        }

    }
}
