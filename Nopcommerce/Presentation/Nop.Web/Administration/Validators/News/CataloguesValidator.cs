using FluentValidation;
using Nop.Admin.Models.News;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Admin.Validators.News
{
    public class CataloguesValidator : BaseNopValidator<CataloguesModel>
    {
        public CataloguesValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Categories.Fields.Name.Required"));
        }
    }
}