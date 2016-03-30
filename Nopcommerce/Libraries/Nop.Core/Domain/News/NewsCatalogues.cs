namespace Nop.Core.Domain.News
{
    /// <summary>
    /// Represents a news category mapping
    /// </summary>
    public partial class NewsCatalogues : BaseEntity
    {
        /// <summary>
        /// Gets or sets the news identifier
        /// </summary>
        public int NewsId { get; set; }

        /// <summary>
        /// Gets or sets the category identifier
        /// </summary>
        public int CatalogueId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the news is featured
        /// </summary>
        public bool IsFeaturedNews { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
        
        /// <summary>
        /// Gets the category
        /// </summary>
        public virtual Catalogues Catalogues { get; set; }

        /// <summary>
        /// Gets the news
        /// </summary>
        public virtual NewsItem News { get; set; }

    }

}
