using Nop.Core.Domain.News;
namespace Nop.Data.Mapping.News
{
    public partial class NewsCatalogueMap : NopEntityTypeConfiguration<NewsCatalogue>
    {
        public NewsCatalogueMap()
        {
            this.ToTable("News_Catalogue_Mapping");
            this.HasKey(pc => pc.Id);

            this.HasRequired(pc => pc.Catalogue)
                .WithMany()
                .HasForeignKey(pc => pc.CatalogueId);

            this.HasRequired(pc => pc.News)
                .WithMany(p => p.NewsCatalogue)
                .HasForeignKey(pc => pc.NewsId);
        }
    }
}