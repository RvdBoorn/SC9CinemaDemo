using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.XConnect;

namespace SitecoreCinema.Model.Collection
{
    public class ReservedCinemaTicket : Outcome
    {
        public static Guid EventDefinitionId { get; } = new Guid("7FC83103-8824-46AE-A7FE-914164D47D00");
        public ReservedCinemaTicket(DateTime timestamp, string currencyCode, decimal monetaryValue) : base(EventDefinitionId, timestamp, currencyCode, monetaryValue)
        {
        }

        public string MovieName { get; set; }
    }
}