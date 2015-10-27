using Nop.Core.Data;
using Nop.Core.Domain.Messages;
using Nop.Core.Events;
using Nop.Core.Plugins;
using Nop.Plugin.Misc.TrialTracker.Data;
using Nop.Plugin.Misc.TrialTracker.Domain;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Web.Framework.Menu;
using Nop.Web.Framework.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.TrialTracker
{
    public class TrialTrackerPlugin : BasePlugin, IAdminMenuPlugin, IConsumer<EntityDeleted<NewsLetterSubscription>>
    {
        private TrialTrackerRecordObjectContext _context;
        private IRepository<TrialTrackerRecord> _trialRepo;
        private ISettingService _settings;

        public TrialTrackerPlugin(TrialTrackerRecordObjectContext context, IRepository<TrialTrackerRecord> trialRepo, ISettingService commonSettings) {
            _context = context;
            _trialRepo = trialRepo;
            _settings = commonSettings;
        }

        public bool Authenticate()
        {
            return true;
        }

        public override void Install()
        {
            _context.Install();

            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.TrialTracker.NameLabel", "Your Name");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.TrialTracker.NameLabel.Hint", "Please provide a name.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.TrialTracker.NameRequired", "Name is Required");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.TrialTracker.EmailLabel", "Your Email");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.TrialTracker.EmailLabel.Hint", "Please provide an email adddress.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.TrialTracker.EmailRequired", "Email Address is Required");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.TrialTracker.EmailFormat", "Invalid Email Address");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.TrialTracker.SubmitFormMessage", "Please provide a name and email.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.TrialTracker.ThankYouMessage", "Thanks for your interest! Download link is below.");


            _settings.SetSetting<bool>("AutoAddTrialEmail", false);

            base.Install();
        }

        public override void Uninstall()
        {
            _context.Uninstall();

            this.DeletePluginLocaleResource("Plugins.Widgets.TrialTracker.NameLabel");
            this.DeletePluginLocaleResource("Plugins.Widgets.TrialTracker.NameLabel.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.TrialTracker.NameRequired");
            this.DeletePluginLocaleResource("Plugins.Widgets.TrialTracker.EmailLabel");
            this.DeletePluginLocaleResource("Plugins.Widgets.TrialTracker.EmailLabel.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.TrialTracker.EmailRequired");
            this.DeletePluginLocaleResource("Plugins.Widgets.TrialTracker.EmailFormat");
            this.DeletePluginLocaleResource("Plugins.Widgets.TrialTracker.SubmitFormMessage");
            this.DeletePluginLocaleResource("Plugins.Widgets.TrialTracker.ThankYouMessage");


            base.Uninstall();
        }

        public Web.Framework.Menu.SiteMapNode BuildMenuItem()
        {
            SiteMapNode node = new SiteMapNode { Visible = true, Title = "Trial Tracker", Url = "/TrialTracker/Manage" };
            return node;
        }


        public void HandleEvent(EntityDeleted<NewsLetterSubscription> eventMessage)
        {
            TrialTrackerRecord entity = _trialRepo.Table.Where(x => x.CustomerEmail == eventMessage.Entity.Email).FirstOrDefault();
            entity.OnMailingList = false;
            _trialRepo.Update(entity);
        }
    }
}
