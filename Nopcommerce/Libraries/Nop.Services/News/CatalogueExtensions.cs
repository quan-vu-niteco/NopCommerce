using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.News;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Services.News
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class CatalogueExtensions
    {
        /// <summary>
        /// Sort categories for tree representation
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="parentId">Parent Catalogue identifier</param>
        /// <param name="ignoreCatalogueWithoutExistingParent">A value indicating whether categories without parent Catalogue in provided Catalogue list (source) should be ignored</param>
        /// <returns>Sorted categories</returns>
        public static IList<Catalogue> SortCatalogueForTree(this IList<Catalogue> source, int parentId = 0, bool ignoreCatalogueWithoutExistingParent = false)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var result = new List<Catalogue>();

            foreach (var cat in source.Where(c => c.ParentCatalogueId == parentId).ToList())
            {
                result.Add(cat);
                result.AddRange(SortCatalogueForTree(source, cat.Id, true));
            }
            if (!ignoreCatalogueWithoutExistingParent && result.Count != source.Count)
            {
                //find categories without parent in provided Catalogue source and insert them into result
                foreach (var cat in source)
                    if (result.FirstOrDefault(x => x.Id == cat.Id) == null)
                        result.Add(cat);
            }
            return result;
        }

        /// <summary>
        /// Returns a NewsCatalogue that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="newsId">News identifier</param>
        /// <param name="CatalogueId">Catalogue identifier</param>
        /// <returns>A NewsCatalogue that has the specified values; otherwise null</returns>
        public static NewsCatalogue FindNewsCatalogue(this IList<NewsCatalogue> source,
            int newsId, int catalogueId)
        {
            foreach (var newsCatalogue in source)
                if (newsCatalogue.NewsId == newsId && newsCatalogue.CatalogueId == catalogueId)
                    return newsCatalogue;

            return null;
        }

        /// <summary>
        /// Get formatted Catalogue breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="catalogue">Catalogue</param>
        /// <param name="CatalogueService">Catalogue service</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>Formatted breadcrumb</returns>
        public static string GetFormattedBreadCrumb(this Catalogue catalogue,
            ICatalogueervice CatalogueService,
            string separator = ">>", int languageId = 0)
        {
            if (catalogue == null)
                throw new ArgumentNullException("catalogue");

            string result = string.Empty;

            //used to prevent circular references
            var alreadyProcessedCatalogueIds = new List<int>();

            while (catalogue != null &&  //not null
                !catalogue.Deleted &&  //not deleted
                !alreadyProcessedCatalogueIds.Contains(catalogue.Id)) //prevent circular references
            {
                var CatalogueName = catalogue.GetLocalized(x => x.Name, languageId);
                if (String.IsNullOrEmpty(result))
                {
                    result = CatalogueName;
                }
                else
                {
                    result = string.Format("{0} {1} {2}", CatalogueName, separator, result);
                }

                alreadyProcessedCatalogueIds.Add(catalogue.Id);

                catalogue = CatalogueService.GetCatalogueById(catalogue.ParentCatalogueId);

            }
            return result;
        }

        /// <summary>
        /// Get formatted Catalogue breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="catalogue">Catalogue</param>
        /// <param name="allCatalogue">All categories</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>Formatted breadcrumb</returns>
        public static string GetFormattedBreadCrumb(this Catalogue catalogue,
            IList<Catalogue> allCatalogue,
            string separator = ">>", int languageId = 0)
        {
            if (catalogue == null)
                throw new ArgumentNullException("catalogue");

            if (allCatalogue == null)
                throw new ArgumentNullException("catalogue");

            string result = string.Empty;

            //used to prevent circular references 
            var alreadyProcessedCatalogueIds = new List<int>();

            while (catalogue != null && //not null 
                   !catalogue.Deleted && //not deleted 
                   !alreadyProcessedCatalogueIds.Contains(catalogue.Id)) //prevent circular references 
            {
                var CatalogueName = catalogue.GetLocalized(x => x.Name, languageId);
                if (String.IsNullOrEmpty(result))
                {
                    result = CatalogueName;
                }
                else
                {
                    result = string.Format("{0} {1} {2}", CatalogueName, separator, result);
                }

                alreadyProcessedCatalogueIds.Add(catalogue.Id);

                catalogue = (from c in allCatalogue
                    where c.Id == catalogue.ParentCatalogueId
                    select c).FirstOrDefault();
            }
            return result;
        }

        /// <summary>
        /// Get Catalogue breadcrumb 
        /// </summary>
        /// <param name="catalogue">Catalogue</param>
        /// <param name="CatalogueService">Catalogue service</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>Catalogue breadcrumb </returns>
        public static IList<Catalogue> GetCatalogueBreadCrumb(this Catalogue catalogue,
            ICatalogueervice CatalogueService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            bool showHidden = false)
        {
            if (catalogue == null)
                throw new ArgumentNullException("catalogue");

            var result = new List<Catalogue>();

            //used to prevent circular references
            var alreadyProcessedCatalogueIds = new List<int>();

            while (catalogue != null && //not null
                !catalogue.Deleted && //not deleted
                (showHidden || catalogue.Published) && //published
                (showHidden || aclService.Authorize(catalogue)) && //ACL            
                !alreadyProcessedCatalogueIds.Contains(catalogue.Id)) //prevent circular references
            {
                result.Add(catalogue);

                alreadyProcessedCatalogueIds.Add(catalogue.Id);

                catalogue = CatalogueService.GetCatalogueById(catalogue.ParentCatalogueId);
            }
            result.Reverse();
            return result;
        }

    }
}
