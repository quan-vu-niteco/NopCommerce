using Nop.Plugin.Misc.TrialTracker.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.TrialTracker.Data
{
    public class TrialTrackerRecordMap : EntityTypeConfiguration<TrialTrackerRecord>
    {
        public TrialTrackerRecordMap() {
            ToTable("TrialTracker");
            HasKey(m => m.TrialTrackerId);

            Property(m => m.CustomerEmail);
            Property(m => m.CustomerName);
            Property(m => m.ProductName);
            Property(m => m.Productid);
            Property(m => m.DownloadDate);
            Property(m => m.OnMailingList);
        }
        
    }
}
