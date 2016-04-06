using System;
using System.Globalization;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Services.Directory;
using Nop.Services.Localization;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Price formatter
    /// </summary>
    public partial class PriceFormatter : IPriceFormatter
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly CurrencySettings _currencySettings;

        #endregion

        #region Constructors

        public PriceFormatter(IWorkContext workContext,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            CurrencySettings currencySettings)
        {
            this._workContext = workContext;
            this._currencyService = currencyService;
            this._localizationService = localizationService;
            this._currencySettings = currencySettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets currency string
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <returns>Currency string without exchange rate</returns>
        protected virtual string GetCurrencyString(decimal amount)
        {
            return GetCurrencyString(amount, true, _workContext.WorkingCurrency);
        }

        /// <summary>
        /// Gets currency string
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="showCurrency">A value indicating whether to show a currency</param>
        /// <param name="targetCurrency">Target currency</param>
        /// <returns>Currency string without exchange rate</returns>
        protected virtual string GetCurrencyString(decimal amount,
            bool showCurrency, Currency targetCurrency)
        {
            if (targetCurrency == null)
                throw new ArgumentNullException("targetCurrency");

            string result = "";
            if (!String.IsNullOrEmpty(targetCurrency.CustomFormatting))
            {
                result = amount.ToString(targetCurrency.CustomFormatting);
            }
            else
            {
                if (!String.IsNullOrEmpty(targetCurrency.DisplayLocale))
                {
                    result = amount.ToString("C", new CultureInfo(targetCurrency.DisplayLocale));
                }
                else
                {
                    result = String.Format("{0} ({1})", amount.ToString("N"), targetCurrency.CurrencyCode);
                    return result;
                }
            }

            if (showCurrency && _currencySettings.DisplayCurrencyLabel)
                result = String.Format("{0} ({1})", result, targetCurrency.CurrencyCode);
            return result;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Formats the price
        /// </summary>
        /// <param name="price">Price</param>
        /// <returns>Price</returns>
        public virtual string FormatPrice(decimal price)
        {
            return FormatPrice(price, true, _workContext.WorkingCurrency);
        }

        /// <summary>
        /// Formats the price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="showCurrency">A value indicating whether to show a currency</param>
        /// <param name="targetCurrency">Target currency</param>
        /// <returns>Price</returns>
        public virtual string FormatPrice(decimal price, bool showCurrency, Currency targetCurrency)
        {
            return FormatPrice(price, showCurrency, targetCurrency, _workContext.WorkingLanguage);
        }

        /// <summary>
        /// Formats the price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="showCurrency">A value indicating whether to show a currency</param>
        /// <param name="showTax">A value indicating whether to show tax suffix</param>
        /// <returns>Price</returns>
        public virtual string FormatPrice(decimal price, bool showCurrency)
        {
     
            return FormatPrice(price, showCurrency, _workContext.WorkingCurrency, _workContext.WorkingLanguage);
        }

        /// <summary>
        /// Formats the price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="showCurrency">A value indicating whether to show a currency</param>
        /// <param name="currencyCode">Currency code</param>
        /// <param name="language">Language</param>
        /// <returns>Price</returns>
        public virtual string FormatPrice(decimal price, bool showCurrency,
            string currencyCode,  Language language)
        {
            var currency = _currencyService.GetCurrencyByCode(currencyCode);
            if (currency == null)
            {
                currency = new Currency();
                currency.CurrencyCode = currencyCode;
            }
            return FormatPrice(price, showCurrency, currency, language);
        }
        
        /// <summary>
        /// Formats the price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="showCurrency">A value indicating whether to show a currency</param>
        /// <param name="targetCurrency">Target currency</param>
        /// <param name="language">Language</param>
        /// <returns>Price</returns>
        public virtual string FormatPrice(decimal price, bool showCurrency, 
            Currency targetCurrency, Language language)
        {
            return FormatPrice(price, showCurrency, targetCurrency, language);
        }

      

        /// <summary>
        /// Formats the price of rental product (with rental period)
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="price">Price</param>
        /// <returns>Rental product price with period</returns>
        public virtual string FormatRentalProductPeriod(Product product, string price)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (!product.IsRental)
                return price;

            if (String.IsNullOrWhiteSpace(price))
                return price;
          

            return string.Empty;
        }
        #endregion
    }
}
