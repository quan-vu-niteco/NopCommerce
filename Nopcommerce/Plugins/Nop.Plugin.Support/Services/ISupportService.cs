using Nop.Core;
using Nop.Plugin.Widgets.Support.Domain;

namespace Nop.Plugin.Widgets.Support.Services
{
    /// <summary>
    /// Tax rate service interface
    /// </summary>
    public partial interface ISupportService
    {
        /// <summary>
        /// Deletes a tax rate
        /// </summary>
        /// <param name="support">Tax rate</param>
        void Delete(SupportItem support);

        /// <summary>
        /// Gets all tax rates
        /// </summary>
        /// <returns>Tax rates</returns>
        IPagedList<SupportItem> GetAll(int languageId=0, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a tax rate
        /// </summary>
        /// <param name="supportId">Tax rate identifier</param>
        /// <returns>Tax rate</returns>
        SupportItem GetById(int supportId);

        /// <summary>
        /// Inserts a tax rate
        /// </summary>
        /// <param name="support">Tax rate</param>
        void Insert(SupportItem support);

        /// <summary>
        /// Updates the tax rate
        /// </summary>
        /// <param name="support">Tax rate</param>
        void Update(SupportItem support);
    }
}
