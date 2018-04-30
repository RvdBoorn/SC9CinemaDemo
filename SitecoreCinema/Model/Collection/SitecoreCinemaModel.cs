using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.XConnect;
using Sitecore.XConnect.Schema;

namespace SitecoreCinema.Models.Model.Collection
{
    public class SitecoreCinemaModel
    {
        public static XdbModel Model { get; } = BuildModel();

        private static XdbModel BuildModel()
        {
            XdbModelBuilder builder = new XdbModelBuilder("SitecoreCinemaModel", new XdbModelVersion(1, 0));
			builder.ReferenceModel(Sitecore.XConnect.Collection.Model.CollectionModel.Model);

			builder.DefineFacet<Contact, CinemaVisitorInfo>(FacetKeys.CinemaVisitorInfo);
            builder.DefineFacet<Interaction, CinemaInfo>(FacetKeys.CinemaInfo);
            builder.DefineEventType<PurchasedCinemaTicket>(false);
            builder.DefineEventType<PurchasedFood>(false);
            builder.DefineEventType<ReservedCinemaTicket>(false);

            return builder.BuildModel();
        }
    }

    public class FacetKeys
    {
        public const string CinemaInfo = "CinemaInfo";
        public const string CinemaVisitorInfo = "CinemaVisitorInfo";
    }
}