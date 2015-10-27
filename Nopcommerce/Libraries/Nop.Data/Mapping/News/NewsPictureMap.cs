using Nop.Core.Domain.News;

namespace Nop.Data.Mapping.News
{
    public partial class NewsPictureMap : NopEntityTypeConfiguration<NewsPicture>
    {
        public NewsPictureMap()
        {
            this.ToTable("News_Picture_Mapping");
            this.HasKey(pp => pp.Id);
            
            this.HasRequired(pp => pp.Picture)
                .WithMany(p => p.NewsPictures)
                .HasForeignKey(pp => pp.PictureId);


            this.HasRequired(pp => pp.News)
                .WithMany(p => p.NewsPictures)
                .HasForeignKey(pp => pp.NewsId);
        }
    }
}