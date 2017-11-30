using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.XConnect;

namespace SitecoreCinema.Model.Collection
{
    public class PurchasedCinemaTicket : Outcome
    {
        public static Guid EventDefinitionId { get; } = new Guid("4C641625-C1E7-489C-8492-346AA95C41E2");
        public PurchasedCinemaTicket(DateTime timestamp, string currencyCode, decimal monetaryValue) : base(EventDefinitionId, timestamp, currencyCode, monetaryValue)
        {
        }

        public string MovieName { get; set; }
    }
}