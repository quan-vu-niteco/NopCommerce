using FluentValidation;
using FluentValidation.Results;
using Nop.Admin.Models.Customers;
using Nop.Core.Domain.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Admin.Validators.Customers
{
    public class CustomerValidator : BaseNopValidator<CustomerModel>
    {
        public CustomerValidator(ILocalizationService localizationService,
            CustomerSettings customerSettings)
        {
        }

    }
}