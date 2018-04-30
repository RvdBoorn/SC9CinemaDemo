using System;
using System.Linq.Expressions;
using Sitecore.Framework.Rules;
using Sitecore.XConnect;
using Sitecore.XConnect.Segmentation.Predicates;
using Sitecore.XConnect.Collection.Model;
using System.Linq;
using Sitecore.XConnect.Client;
using SitecoreCinema.Models.Model.Collection;

namespace SC9Demo.Rules.Conditions
{
    public class ReservedTickets : ICondition, IMappableRuleEntity, IContactSearchQueryFactory
    {

        public ReservedTickets() { }

        public int NumberOfTickets { get; set; }

        public NumericOperationType Comparison
        {
            get;
            set;
        }

        public Expression<Func<Contact, bool>> CreateContactSearchQuery(IContactSearchQueryContext context)
        {
            return (Contact contact) => Comparison.Evaluate(contact.GetFacet<CinemaVisitorInfo>(CinemaVisitorInfo.DefaultFacetKey).NumberOfReservedTickets, NumberOfTickets);
        }

        public bool Evaluate(IRuleExecutionContext context)
        {
            NumericOperationType comparison = this.Comparison;

            var contact = RuleExecutionContextExtensions.Fact<Contact>(context);
            if (contact == null) { return false; }

            XConnectClient client = XConnectClientReference.GetClient();

            Contact existingContact = client.Get<Contact>(contact, new ExpandOptions(CinemaVisitorInfo.DefaultFacetKey));
            if (existingContact == null) { return false; }

            CinemaVisitorInfo visitInfo = existingContact.GetFacet<CinemaVisitorInfo>(CinemaVisitorInfo.DefaultFacetKey);
            
            if(visitInfo==null) { return false; }
            return Comparison.Evaluate(visitInfo.NumberOfReservedTickets, NumberOfTickets);
        }
    }
}