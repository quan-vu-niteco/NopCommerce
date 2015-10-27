using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.News;

namespace Nop.Services.News
{
    /// <summary>
    /// Catalogues service interface
    /// </summary>
    public partial interface ICataloguesService
    {
        /// <summary>
        /// Delete catalogues
        /// </summary>
        /// <param name="catalogues">Catalogues</param>
        void DeleteCatalogues(Catalogues catalogues);

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="cataloguesName">Catalogues name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Catalogues</returns>
        IPagedList<Catalogues> GetAllCatalogues(string cataloguesName = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        /// <summary>
        /// Gets all categories filtered by parent catalogues identifier
        /// </summary>
        /// <param name="ParentCatalogueId">Parent catalogues identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Catalogues collection</returns>
        IList<Catalogues> GetAllCataloguesByParentCatalogueId(int ParentCatalogueId,
            bool showHidden = false);

        /// <summary>
        /// Gets all categories displayed on the home page
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Catalogues</returns>
        IList<Catalogues> GetAllCataloguesDisplayedOnHomePage(bool showHidden = false);
                
        /// <summary>
        /// Gets a catalogues
        /// </summary>
        /// <param name="catalogueId">Catalogues identifier</param>
        /// <returns>Catalogues</returns>
        Catalogues GetCatalogueById(int catalogueId);

        /// <summary>
        /// Inserts catalogues
        /// </summary>
        /// <param name="catalogues">Catalogues</param>
        void InsertCatalogues(Catalogues catalogues);

        /// <summary>
        /// Updates the catalogues
        /// </summary>
        /// <param name="catalogues">Catalogues</param>
        void UpdateCatalogues(Catalogues catalogues);

        /// <summary>
        /// Deletes a news catalogues mapping
        /// </summary>
        /// <param name="newsCatalogues">News catalogues</param>
        void DeleteNewsCatalogues(NewsCatalogues newsCatalogues);

        /// <summary>
        /// Gets news catalogues mapping collection
        /// </summary>
        /// <param name="catalogueId">Catalogues identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>News a catalogues mapping collection</returns>
        IPagedList<NewsCatalogues> GetNewsCataloguesByCatalogueId(int catalogueId,
            int pageIndex, int pageSize, bool showHidden = false);

        /// <summary>
        /// Gets a news catalogues mapping collection
        /// </summary>
        /// <param name="newsId">News identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>News catalogues mapping collection</returns>
        IList<NewsCatalogues> GetNewsCataloguesByNewsId(int newsId, bool showHidden = false);
       
        /// <summary>
        /// Gets a news catalogues mapping 
        /// </summary>
        /// <param name="newsCatalogueId">News catalogues mapping identifier</param>
        /// <returns>News catalogues mapping</returns>
        NewsCatalogues GetNewsCataloguesById(int newsCatalogueId);

        /// <summary>
        /// Inserts a news catalogues mapping
        /// </summary>
        /// <param name="newsCatalogues">>News catalogues mapping</param>
        void InsertNewsCatalogues(NewsCatalogues newsCatalogues);

        /// <summary>
        /// Updates the news catalogues mapping 
        /// </summary>
        /// <param name="newsCatalogues">>News catalogues mapping</param>
        void UpdateNewsCatalogues(NewsCatalogues newsCatalogues);
    }
}
