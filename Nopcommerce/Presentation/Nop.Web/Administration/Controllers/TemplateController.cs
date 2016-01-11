using System;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Extensions;


using Nop.Services.Security;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Controllers
{
    public partial class TemplateController : BaseAdminController
    {
        #region Fields
    
        private readonly IPermissionService _permissionService;

        #endregion

        #region Constructors

        public TemplateController(
           
            IPermissionService permissionService)
        {
            this._permissionService = permissionService;
        }

        #endregion
    }
}
