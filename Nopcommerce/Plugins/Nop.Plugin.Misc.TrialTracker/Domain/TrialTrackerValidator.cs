using FluentValidation;
using Nop.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Misc.TrialTracker.Domain
{
    public class TrialTrackerValidator : AbstractValidator<TrialTrackerRecord>
    {
        public TrialTrackerValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.CustomerEmail)
                .NotEmpty().WithMessage(localizationService.GetResource("Plugins.Widgets.TrialTracker.EmailRequired"))
                .EmailAddress().WithMessage(localizationService.GetResource("Plugins.Widgets.TrialTracker.EmailFormat"));

            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage(localizationService.GetResource("Plugins.Widgets.TrialTracker.NameRequired"));
        }
    }
}
