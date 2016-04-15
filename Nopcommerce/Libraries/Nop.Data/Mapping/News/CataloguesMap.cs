using Nop.Core.Domain.News;

namespace Nop.Data.Mapping.News
{
    public partial class CatalogueMap : NopEntityTypeConfiguration<Catalogue>
    {
        public CatalogueMap()
        {
            this.ToTable("Catalogue");
            this.HasKey(c => c.Id);
            this.Property(c => c.Name).IsRequired().HasMaxLength(400);
            this.Property(c => c.MetaKeywords).HasMaxLength(400);
            this.Property(c => c.MetaTitle).HasMaxLength(400);        
            this.Property(c => c.PageSizeOptions).HasMaxLength(200);
        }
    }
}