using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.News;

namespace Nop.Services.News
{
    /// <summary>
    /// Catalogue service interface
    /// </summary>
    public partial interface ICatalogueervice
    {
        /// <summary>
        /// Delete Catalogue
        /// </summary>
        /// <param name="catalogue">Catalogue</param>
        void DeleteCatalogue(Catalogue catalogue);

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="CatalogueName">Catalogue name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Catalogue</returns>
        IPagedList<Catalogue> GetAllCatalogue(string CatalogueName = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        /// <summary>
        /// Gets all categories filtered by parent Catalogue identifier
        /// </summary>
        /// <param name="ParentCatalogueId">Parent Catalogue identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Catalogue collection</returns>
        IList<Catalogue> GetAllCatalogueByParentCatalogueId(int ParentCatalogueId,
            bool showHidden = false);

        /// <summary>
        /// Gets all categories displayed on the home page
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Catalogue</returns>
        IList<Catalogue> GetAllCatalogueDisplayedOnHomePage(bool showHidden = false);
                
        /// <summary>
        /// Gets a Catalogue
        /// </summary>
        /// <param name="catalogueId">Catalogue identifier</param>
        /// <returns>Catalogue</returns>
        Catalogue GetCatalogueById(int catalogueId);

        /// <summary>
        /// Inserts Catalogue
        /// </summary>
        /// <param name="catalogue">Catalogue</param>
        void InsertCatalogue(Catalogue catalogue);

        /// <summary>
        /// Updates the Catalogue
        /// </summary>
        /// <param name="catalogue">Catalogue</param>
        void UpdateCatalogue(Catalogue catalogue);

        /// <summary>
        /// Deletes a news Catalogue mapping
        /// </summary>
        /// <param name="newsCatalogue">News Catalogue</param>
        void DeleteNewsCatalogue(NewsCatalogue newsCatalogue);

        /// <summary>
        /// Gets news Catalogue mapping collection
        /// </summary>
        /// <param name="catalogueId">Catalogue identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>News a Catalogue mapping collection</returns>
        IPagedList<NewsCatalogue> GetNewsCatalogueByCatalogueId(int catalogueId,
            int pageIndex, int pageSize, bool showHidden = false);

        /// <summary>
        /// Gets a news Catalogue mapping collection
        /// </summary>
        /// <param name="newsId">News identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>News Catalogue mapping collection</returns>
        IList<NewsCatalogue> GetNewsCatalogueByNewsId(int newsId, bool showHidden = false);
       
        /// <summary>
        /// Gets a news Catalogue mapping 
        /// </summary>
        /// <param name="newsCatalogueId">News Catalogue mapping identifier</param>
        /// <returns>News Catalogue mapping</returns>
        NewsCatalogue GetNewsCatalogueById(int newsCatalogueId);

        /// <summary>
        /// Inserts a news Catalogue mapping
        /// </summary>
        /// <param name="newsCatalogue">>News Catalogue mapping</param>
        void InsertNewsCatalogue(NewsCatalogue newsCatalogue);

        /// <summary>
        /// Updates the news Catalogue mapping 
        /// </summary>
        /// <param name="newsCatalogue">>News Catalogue mapping</param>
        void UpdateNewsCatalogue(NewsCatalogue newsCatalogue);
    }
}
