using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.XConnect;

namespace SitecoreCinema.Models.Model.Collection
{
    public class PurchasedFood : Outcome
    {
        public static Guid EventDefinitionId { get; } = new Guid("6DDAAC80-61B1-46C1-B433-70E09A56034F");
        public PurchasedFood(DateTime timestamp, string currencyCode, decimal monetaryValue) : base(EventDefinitionId, timestamp, currencyCode, monetaryValue)
        {
        }

        public string FoodType { get; set; }
    }
}