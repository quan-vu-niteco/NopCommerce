using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators.Customers;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Customers
{
    [Validator(typeof(CustomerRoleValidator))]
    public partial class CustomerRoleModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Customers.CustomerRoles.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerRoles.Fields.FreeShipping")]
        [AllowHtml]
        public bool FreeShipping { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerRoles.Fields.TaxExempt")]
        public bool TaxExempt { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerRoles.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerRoles.Fields.IsSystemRole")]
        public bool IsSystemRole { get; set; }

        [NopResourceDisplayName("Admin.Customers.CustomerRoles.Fields.SystemName")]
        public string SystemName { get; set; }

        #region Nested classes

        public partial class AssociateProductToCustomerRoleModel : BaseNopModel
        {
            public AssociateProductToCustomerRoleModel()
            {
            }

            //vendor
            public bool IsLoggedInAsVendor { get; set; }


            public int AssociatedToProductId { get; set; }
        }
        #endregion
    }
}