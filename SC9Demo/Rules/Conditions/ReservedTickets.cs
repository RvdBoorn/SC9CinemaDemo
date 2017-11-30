﻿using System;
using System.Linq.Expressions;
using Sitecore.Framework.Rules;
using Sitecore.XConnect;
using Sitecore.XConnect.Segmentation.Predicates;
using Sitecore.XConnect.Collection.Model;
using System.Linq;
using SitecoreCinema.Model.Collection;

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
            var contact = context.Fact<Contact>();
            if (contact == null) { return false; }
            var visitInfo = contact.GetFacet<CinemaVisitorInfo>(CinemaVisitorInfo.DefaultFacetKey);
            if(visitInfo==null) { return false; }
            return Comparison.Evaluate(visitInfo.NumberOfReservedTickets, NumberOfTickets);
        }
    }
}