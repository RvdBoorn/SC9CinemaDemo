using System;
using System.Linq.Expressions;
using Sitecore.Framework.Rules;
using Sitecore.XConnect;
using Sitecore.XConnect.Segmentation.Predicates;
using Sitecore.XConnect.Collection.Model;
using System.Linq;
using SitecoreCinema.Model.Collection;
using Sitecore.Rules;
using Sitecore.Rules.Conditions;
using Sitecore.Analytics;

namespace SC9Demo.Rules.Conditions
{
    public class PurchasedTicketsRule<T> : OperatorCondition<T> where T : RuleContext
    {

        public PurchasedTicketsRule() { }

        public int NumberOfTickets { get; set; }

        protected override bool Execute(T ruleContext)
        {
            if (Tracker.Current == null || Tracker.Current.Contact == null)
            {
                return false;
            }

            var contact = ContactHelper.GetXConnectContact(Tracker.Current.Contact);
            var cinemaVisitInfo = contact?.GetFacet<CinemaVisitorInfo>(CinemaVisitorInfo.DefaultFacetKey);
            if(cinemaVisitInfo == null)
            {
                return false;
            }


            switch (this.GetOperator())
            {
                case ConditionOperator.Equal:
                    return cinemaVisitInfo.NumberOfPurchasedTickets.Equals(this.NumberOfTickets);
                case ConditionOperator.GreaterThanOrEqual:
                    return cinemaVisitInfo.NumberOfPurchasedTickets >= this.NumberOfTickets;
                case ConditionOperator.GreaterThan:
                    return cinemaVisitInfo.NumberOfPurchasedTickets > this.NumberOfTickets;
                case ConditionOperator.LessThanOrEqual:
                    return cinemaVisitInfo.NumberOfPurchasedTickets <= this.NumberOfTickets;
                case ConditionOperator.LessThan:
                    return cinemaVisitInfo.NumberOfPurchasedTickets < this.NumberOfTickets;
                case ConditionOperator.NotEqual:
                    return cinemaVisitInfo.NumberOfPurchasedTickets.Equals(this.NumberOfTickets) == false;
                default:
                    return false;
            }

        }
    }
}