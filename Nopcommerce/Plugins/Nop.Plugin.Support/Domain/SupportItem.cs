using Nop.Core;

namespace Nop.Plugin.Widgets.Support.Domain
{
    /// <summary>
    /// Represents a tax rate
    /// </summary>
    public partial class SupportItem : BaseEntity
    {
        /// <summary>
        /// Gets or sets the language identifier
        /// </summary>
        public int LanguageId { get; set; }

        /// <summary>
        /// Gets or sets the Image identifier
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the YahooId identifier
        /// </summary>
        public string YahooId { get; set; }

        /// <summary>
        /// Gets or sets the SkypeId identifier
        /// </summary>
        public string SkypeId { get; set; }

        /// <summary>
        /// Gets or sets Name identifier
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Phone identifier
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets Phone identifier
        /// </summary>
        public string HotLine { get; set; }

        /// <summary>
        /// Gets or sets Phone identifier
        /// </summary>
        public int DisplayOrder { get; set; }
      
    }
}