using FluentValidation;
using Nop.Admin.Models.News;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Admin.Validators.News
{
    public class CatalogueValidator : BaseNopValidator<CatalogueModel>
    {
        public CatalogueValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Categories.Fields.Name.Required"));
        }
    }
}