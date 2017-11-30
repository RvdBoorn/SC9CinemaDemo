using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.XConnect;

namespace SitecoreCinema.Model.Collection
{
    [FacetKey(DefaultFacetKey)]
    public class CinemaInfo : Facet
    {
        public const string DefaultFacetKey = Model.Collection.FacetKeys.CinemaInfo;
        public string Cinema { get; set; }
    }
}