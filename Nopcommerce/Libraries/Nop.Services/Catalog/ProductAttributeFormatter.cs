using System;
using System.Text;
using System.Web;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Html;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;


namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product attribute formatter
    /// </summary>
    public partial class ProductAttributeFormatter : IProductAttributeFormatter
    {
        private readonly IWorkContext _workContext;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IDownloadService _downloadService;
        private readonly IWebHelper _webHelper;
        private readonly IPriceCalculationService _priceCalculationService;

        public ProductAttributeFormatter(IWorkContext workContext,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            IPriceFormatter priceFormatter,
            IDownloadService downloadService,
            IWebHelper webHelper,
            IPriceCalculationService priceCalculationService
          )
        {
            this._workContext = workContext;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._currencyService = currencyService;
            this._localizationService = localizationService;
            this._priceFormatter = priceFormatter;
            this._downloadService = downloadService;
            this._webHelper = webHelper;
            this._priceCalculationService = priceCalculationService;
        }

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Attributes</returns>
        public string FormatAttributes(Product product, string attributesXml)
        {
            var customer = _workContext.CurrentCustomer;
            return FormatAttributes(product, attributesXml, customer);
        }
        
        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customer">Customer</param>
        /// <param name="serapator">Serapator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
      
        /// <param name="renderProductAttributes">A value indicating whether to render product attributes</param>
        /// <param name="allowHyperlinks">A value indicating whether to HTML hyperink tags could be rendered (if required)</param>
        /// <returns>Attributes</returns>
        public string FormatAttributes(Product product, string attributesXml,
            Customer customer, string serapator = "<br />", bool htmlEncode = true,
            bool renderProductAttributes = true,
            bool allowHyperlinks = true)
        {
            var result = new StringBuilder();

            //attributes
            if (renderProductAttributes)
            {
                var attributes = _productAttributeParser.ParseProductAttributeMappings(attributesXml);
                for (int i = 0; i < attributes.Count; i++)
                {
                    var attribute = attributes[i];
                    var valuesStr = _productAttributeParser.ParseValues(attributesXml, attribute.Id);
                    for (int j = 0; j < valuesStr.Count; j++)
                    {
                        string valueStr = valuesStr[j];
                        string formattedAttribute = string.Empty;
                        if (!attribute.ShouldHaveValues())
                        {
                            //no values
                            if (attribute.AttributeControlType == AttributeControlType.MultilineTextbox)
                            {
                                //multiline textbox
                                var attributeName = attribute.ProductAttribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id);
                                //encode (if required)
                                if (htmlEncode)
                                    attributeName = HttpUtility.HtmlEncode(attributeName);
                                formattedAttribute = string.Format("{0}: {1}", attributeName, HtmlHelper.FormatText(valueStr, false, true, false, false, false, false));
                                //we never encode multiline textbox input
                            }
                            else if (attribute.AttributeControlType == AttributeControlType.FileUpload)
                            {
                                //file upload
                                Guid downloadGuid;
                                Guid.TryParse(valueStr, out downloadGuid);
                                var download = _downloadService.GetDownloadByGuid(downloadGuid);
                                if (download != null)
                                {
                                    //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                                    string attributeText = "";
                                    var fileName = string.Format("{0}{1}", 
                                        download.Filename ?? download.DownloadGuid.ToString(),
                                        download.Extension);
                                    //encode (if required)
                                    if (htmlEncode)
                                        fileName = HttpUtility.HtmlEncode(fileName);
                                    if (allowHyperlinks)
                                    {
                                        //hyperlinks are allowed
                                        var downloadLink = string.Format("{0}download/getfileupload/?downloadId={1}", _webHelper.GetStoreLocation(false), download.DownloadGuid);
                                        attributeText = string.Format("<a href=\"{0}\" class=\"fileuploadattribute\">{1}</a>", downloadLink, fileName);
                                    }
                                    else
                                    {
                                        //hyperlinks aren't allowed
                                        attributeText = fileName;
                                    }
                                    var attributeName = attribute.ProductAttribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id);
                                    //encode (if required)
                                    if (htmlEncode)
                                        attributeName = HttpUtility.HtmlEncode(attributeName);
                                    formattedAttribute = string.Format("{0}: {1}", attributeName, attributeText);
                                }
                            }
                            else
                            {
                                //other attributes (textbox, datepicker)
                                formattedAttribute = string.Format("{0}: {1}", attribute.ProductAttribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), valueStr);
                                //encode (if required)
                                if (htmlEncode)
                                    formattedAttribute = HttpUtility.HtmlEncode(formattedAttribute);
                            }
                        }
                        else
                        {
                            //attributes with values
                            int attributeValueId;
                            if (int.TryParse(valueStr, out attributeValueId))
                            {
                                var attributeValue = _productAttributeService.GetProductAttributeValueById(attributeValueId);
                                if (attributeValue != null)
                                {
                                    formattedAttribute = string.Format("{0}: {1}", attribute.ProductAttribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), attributeValue.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id));
                                    
                                }                               
                                //encode (if required)
                                if (htmlEncode)
                                    formattedAttribute = HttpUtility.HtmlEncode(formattedAttribute);
                            }
                        }

                        if (!String.IsNullOrEmpty(formattedAttribute))
                        {
                            if (i != 0 || j != 0)
                                result.Append(serapator);
                            result.Append(formattedAttribute);
                        }
                    }
                }
            }
            return result.ToString();
        }
    }
}
