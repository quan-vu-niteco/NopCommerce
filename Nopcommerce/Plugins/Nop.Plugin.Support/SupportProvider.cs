using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Plugins;
using Nop.Plugin.Widgets.Support.Data;
using Nop.Plugin.Widgets.Support.Services;
using Nop.Services.Cms;
using Nop.Services.Localization;


namespace Nop.Plugin.Widgets.Support
{
    /// <summary>
    /// Fixed rate tax provider
    /// </summary>
    public class SupportProvider : BasePlugin, IWidgetPlugin
    {
        private readonly ISupportService _supportService;
        private readonly IStoreContext _storeContext;
        private readonly SupportObjectContext _objectContext;
        private readonly ICacheManager _cacheManager;

        public SupportProvider(ISupportService supportService,
            IStoreContext storeContext,
            SupportObjectContext objectContext,
            ICacheManager cacheManager)
        {
            this._supportService = supportService;
            this._storeContext = storeContext;
            this._objectContext = objectContext;
            this._cacheManager = cacheManager;
        }


        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //database objects
            _objectContext.Install();
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Support.Fields.Language", "Language");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Support.Fields.Image", "Image");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Support.Fields.YahooId", "YahooId");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Support.Fields.Name", "Name");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Support.Fields.Phone", "Phone");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Support.Fields.HotLine", "HotLine");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Support.Fields.DisplayOrder", "DisplayOrder");
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //database objects
            _objectContext.Uninstall();
            
            base.Uninstall();
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string> { "home_page_top", "productdetails_after_pictures" };
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "WidgetsSupport";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Widgets.Support.Controllers" }, { "area", null } };
        }

        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
          
            controllerName = "WidgetsSupport";
            actionName = "PublicInfo";

            if (widgetZone == "home_page_top")
            {
                //set action/controller/routeValues
                actionName = "PublicInfoTop";
            }
            if (widgetZone == "home_page_bottom")
            {
                //set action/controller/routeValues
                actionName = "PublicInfoBottom";
            }
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Nop.Plugin.Widgets.Support.Controllers"},
                {"area", null},
                {"widgetZone", widgetZone}
            };
        }
    }
}
