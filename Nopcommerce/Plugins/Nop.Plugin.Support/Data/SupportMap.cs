using Nop.Data.Mapping;
using Nop.Plugin.Widgets.Support.Domain;

namespace Nop.Plugin.Widgets.Support.Data
{
    public partial class SupportMap : NopEntityTypeConfiguration<SupportItem>
    {
        public SupportMap()
        {
            this.ToTable("Support");
            this.HasKey(tr => tr.Id);
            
        }
    }
}