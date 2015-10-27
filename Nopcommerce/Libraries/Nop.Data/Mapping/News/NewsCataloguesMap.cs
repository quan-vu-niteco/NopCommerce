using Nop.Core.Domain.News;
namespace Nop.Data.Mapping.News
{
    public partial class NewsCataloguesMap : NopEntityTypeConfiguration<NewsCatalogues>
    {
        public NewsCataloguesMap()
        {
            this.ToTable("News_Catalogues_Mapping");
            this.HasKey(pc => pc.Id);

            this.HasRequired(pc => pc.Catalogues)
                .WithMany()
                .HasForeignKey(pc => pc.CatalogueId);

            this.HasRequired(pc => pc.News)
                .WithMany(p => p.NewsCatalogues)
                .HasForeignKey(pc => pc.NewsId);
        }
    }
}