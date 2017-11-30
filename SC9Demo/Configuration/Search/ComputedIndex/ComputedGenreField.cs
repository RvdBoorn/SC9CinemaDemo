using Sitecore.ContentSearch.ComputedFields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ContentSearch;
using Sitecore.Data;
using System.Text;

namespace SC9Demo.Configuration.Search.ComputedIndex
{
    public class ComputedGenreField : IComputedIndexField
    {
        public string FieldName { get; set; }

        public string ReturnType { get; set; }

        public object ComputeFieldValue(IIndexable indexable)
        {
            var item = indexable as SitecoreIndexableItem;

            if (item == null || item.Item == null) return string.Empty;

            if (!item.Item.TemplateID.Equals(new ID("{D645B4B7-CDBE-469F-BB17-55362FFCFFFB}"))) return string.Empty;

            StringBuilder genreList = new StringBuilder();

            IIndexableDataField genreFiled = indexable.GetFieldById(new ID("{F0BC9308-B0B8-4A76-9E03-B0D258440154}"));

            if (genreFiled != null)

            {
                var genres = genreFiled.Value.ToString().Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries).ToList();

                foreach (var genreId in genres)

                {

                    var genreItem = item.Item.Database.GetItem(new ID(genreId));
                    if (genreItem != null)
                    {
                        genreList.Append(genreItem.Name.ToString() + ";");
                    }

                }

                return genreList;

            }

            return null;
        }


        }
}








