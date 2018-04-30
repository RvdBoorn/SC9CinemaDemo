using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.XConnect;

namespace SitecoreCinema.Models.Model.Collection
{
    [FacetKey(DefaultFacetKey)]
    [Serializable]
    public class CinemaVisitorInfo : Facet
    {
        public const string DefaultFacetKey = Model.Collection.FacetKeys.CinemaVisitorInfo;
        public int NumberOfReservedTickets { get; set; }
        public int NumberOfPurchasedTickets { get; set; }
        public bool FoodDiscountApplied { get; set; } = false;
    }
}