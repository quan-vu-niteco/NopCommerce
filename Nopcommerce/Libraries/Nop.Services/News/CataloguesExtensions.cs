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
    public static class CataloguesExtensions
    {
        /// <summary>
        /// Sort categories for tree representation
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="parentId">Parent catalogues identifier</param>
        /// <param name="ignoreCataloguesWithoutExistingParent">A value indicating whether categories without parent catalogues in provided catalogues list (source) should be ignored</param>
        /// <returns>Sorted categories</returns>
        public static IList<Catalogues> SortCataloguesForTree(this IList<Catalogues> source, int parentId = 0, bool ignoreCataloguesWithoutExistingParent = false)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var result = new List<Catalogues>();

            foreach (var cat in source.Where(c => c.ParentCatalogueId == parentId).ToList())
            {
                result.Add(cat);
                result.AddRange(SortCataloguesForTree(source, cat.Id, true));
            }
            if (!ignoreCataloguesWithoutExistingParent && result.Count != source.Count)
            {
                //find categories without parent in provided catalogues source and insert them into result
                foreach (var cat in source)
                    if (result.FirstOrDefault(x => x.Id == cat.Id) == null)
                        result.Add(cat);
            }
            return result;
        }

        /// <summary>
        /// Returns a NewsCatalogues that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="newsId">News identifier</param>
        /// <param name="CatalogueId">Catalogues identifier</param>
        /// <returns>A NewsCatalogues that has the specified values; otherwise null</returns>
        public static NewsCatalogues FindNewsCatalogues(this IList<NewsCatalogues> source,
            int newsId, int catalogueId)
        {
            foreach (var newsCatalogues in source)
                if (newsCatalogues.NewsId == newsId && newsCatalogues.CatalogueId == catalogueId)
                    return newsCatalogues;

            return null;
        }

        /// <summary>
        /// Get formatted catalogues breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="catalogues">Catalogues</param>
        /// <param name="cataloguesService">Catalogues service</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>Formatted breadcrumb</returns>
        public static string GetFormattedBreadCrumb(this Catalogues catalogues,
            ICataloguesService cataloguesService,
            string separator = ">>", int languageId = 0)
        {
            if (catalogues == null)
                throw new ArgumentNullException("catalogues");

            string result = string.Empty;

            //used to prevent circular references
            var alreadyProcessedCatalogueIds = new List<int>();

            while (catalogues != null &&  //not null
                !catalogues.Deleted &&  //not deleted
                !alreadyProcessedCatalogueIds.Contains(catalogues.Id)) //prevent circular references
            {
                var cataloguesName = catalogues.GetLocalized(x => x.Name, languageId);
                if (String.IsNullOrEmpty(result))
                {
                    result = cataloguesName;
                }
                else
                {
                    result = string.Format("{0} {1} {2}", cataloguesName, separator, result);
                }

                alreadyProcessedCatalogueIds.Add(catalogues.Id);

                catalogues = cataloguesService.GetCatalogueById(catalogues.ParentCatalogueId);

            }
            return result;
        }

        /// <summary>
        /// Get formatted catalogues breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="catalogues">Catalogues</param>
        /// <param name="allCatalogues">All categories</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>Formatted breadcrumb</returns>
        public static string GetFormattedBreadCrumb(this Catalogues catalogues,
            IList<Catalogues> allCatalogues,
            string separator = ">>", int languageId = 0)
        {
            if (catalogues == null)
                throw new ArgumentNullException("catalogues");

            if (allCatalogues == null)
                throw new ArgumentNullException("catalogues");

            string result = string.Empty;

            //used to prevent circular references 
            var alreadyProcessedCatalogueIds = new List<int>();

            while (catalogues != null && //not null 
                   !catalogues.Deleted && //not deleted 
                   !alreadyProcessedCatalogueIds.Contains(catalogues.Id)) //prevent circular references 
            {
                var cataloguesName = catalogues.GetLocalized(x => x.Name, languageId);
                if (String.IsNullOrEmpty(result))
                {
                    result = cataloguesName;
                }
                else
                {
                    result = string.Format("{0} {1} {2}", cataloguesName, separator, result);
                }

                alreadyProcessedCatalogueIds.Add(catalogues.Id);

                catalogues = (from c in allCatalogues
                    where c.Id == catalogues.ParentCatalogueId
                    select c).FirstOrDefault();
            }
            return result;
        }

        /// <summary>
        /// Get catalogues breadcrumb 
        /// </summary>
        /// <param name="catalogues">Catalogues</param>
        /// <param name="cataloguesService">Catalogues service</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>Catalogues breadcrumb </returns>
        public static IList<Catalogues> GetCataloguesBreadCrumb(this Catalogues catalogues,
            ICataloguesService cataloguesService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            bool showHidden = false)
        {
            if (catalogues == null)
                throw new ArgumentNullException("catalogues");

            var result = new List<Catalogues>();

            //used to prevent circular references
            var alreadyProcessedCatalogueIds = new List<int>();

            while (catalogues != null && //not null
                !catalogues.Deleted && //not deleted
                (showHidden || catalogues.Published) && //published
                (showHidden || aclService.Authorize(catalogues)) && //ACL            
                !alreadyProcessedCatalogueIds.Contains(catalogues.Id)) //prevent circular references
            {
                result.Add(catalogues);

                alreadyProcessedCatalogueIds.Add(catalogues.Id);

                catalogues = cataloguesService.GetCatalogueById(catalogues.ParentCatalogueId);
            }
            result.Reverse();
            return result;
        }

    }
}
