using System;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;

namespace SC9Demo.Configuration.SiteUI.Search.Models
{
    public class SitecoreItem : SearchResultItem
    {
        public string Title { get; set; }

        [IndexField("__smallcreateddate")]
        public DateTime PublishDate { get; set; }

        [IndexField("has_presentation")]
        public bool HasPresentation { get; set; }

        [IndexField(BuiltinFields.Semantics)]
        public string Tags { get; set; }

        [IndexField("show_in_search_results")]
        public bool ShowInSearchResults { get; set; }

        [IndexField("conditions")]
        public string Conditions { get; set; }

        [IndexField("location")]
        public string Location { get; set; }

        [IndexField("type")]
        public string Type { get; set; }

        [IndexField("jobtitle")]
        public string JobTitle { get; set; }

        [IndexField("description")]
        public string Description { get; set; }
    }
}