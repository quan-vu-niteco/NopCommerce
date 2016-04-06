using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.News;


namespace Nop.Core.Domain.Media
{
    /// <summary>
    /// Represents a picture
    /// </summary>
    public partial class Picture : BaseEntity
    {
        private ICollection<NewsPicture> _newsPictures;

        private ICollection<ProductPicture> _productPictures;

        /// <summary>
        /// Gets or sets the picture binary
        /// </summary>
        public byte[] PictureBinary { get; set; }

        /// <summary>
        /// Gets or sets the picture mime type
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the SEO friednly filename of the picture
        /// </summary>
        public string SeoFilename { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the picture is new
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// Gets or sets the news pictures
        /// </summary>
        public virtual ICollection<NewsPicture> NewsPictures
        {
            get { return _newsPictures ?? (_newsPictures = new List<NewsPicture>()); }
            protected set { _newsPictures = value; }
        }
        /// <summary>
        /// Gets or sets the news pictures
        /// </summary>
        public virtual ICollection<ProductPicture> ProductPictures
        {
            get { return _productPictures ?? (_productPictures = new List<ProductPicture>()); }
            protected set { _productPictures = value; }
        }
    }
}
