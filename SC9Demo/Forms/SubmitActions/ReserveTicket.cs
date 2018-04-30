using Sitecore.Analytics.Model;
using Sitecore.Diagnostics;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Processing;
using Sitecore.ExperienceForms.Processing.Actions;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Collection.Model;
using SitecoreCinema.Models.Model.Collection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SC9Demo.Forms.SubmitActions
{
    public class ReserveTicket : SubmitActionBase<string>
    {
        private Guid POS = Guid.Parse("B6C1EFF9-51E3-422F-8994-E9E72C50D7A1");

        public ReserveTicket(ISubmitActionData submitActionData) : base(submitActionData)
        {
        }

        protected override bool Execute(string data, FormSubmitContext formSubmitContext)
        {
            return true;
        }

        public override void ExecuteAction(FormSubmitContext formSubmitContext, string parameters)
        {
            var fields = formSubmitContext.Fields;

            var email = fields.Where(x => x.Name.ToLower() == "e-mail").FirstOrDefault();
            var movie = fields.Where(x => x.Name.ToLower() == "movie").FirstOrDefault();
            if (email != null)
            {
                var trackerIdentifier = new IdentifiedContactReference("sitecoreextranet", GetValue(email));

                using (Sitecore.XConnect.Client.XConnectClient client = Sitecore.XConnect.Client.Configuration.SitecoreXConnectClientConfiguration.GetClient())
                {
                    try
                    {
                        var contact = client.Get<Sitecore.XConnect.Contact>(trackerIdentifier, new Sitecore.XConnect.ContactExpandOptions());
                        var contactInfo = contact.GetFacet<CinemaVisitorInfo>();

                        if (contact == null)
                        {
                            contact = new Contact(new ContactIdentifier("sitecoreextranet", GetValue(email), ContactIdentifierType.Known));
      
                        }

                        if (contact != null)
                        {
                            int tickets = 1;
                            if(contactInfo != null)
                            {
                                tickets = contactInfo.NumberOfReservedTickets + 1;
                            }
                            
                           var vistitorInfo = new CinemaVisitorInfo
                           {
                                    NumberOfReservedTickets = tickets, 
                                    FoodDiscountApplied = true
                           };

                            var interaction = new Interaction(contact, InteractionInitiator.Contact, POS, "");
                            client.SetFacet<CinemaInfo>(interaction, CinemaInfo.DefaultFacetKey, new CinemaInfo() { Cinema = "St Katharines Dock" });
                            client.SetFacet<CinemaVisitorInfo>(contact, CinemaVisitorInfo.DefaultFacetKey, vistitorInfo);
                            var moviename = Sitecore.Context.Database.GetItem(new Sitecore.Data.ID(GetValue(movie))).Name;
                            interaction.Events.Add(new ReservedCinemaTicket(DateTime.UtcNow, "GBP", 0) { MovieName = moviename });
                            client.AddInteraction(interaction);
                            client.Submit();
                        }
                    }
                    catch(Exception ex)
                    {
                        Sitecore.Diagnostics.Log.Error(ex.Message, ex, this);
                    }
                }
            }
        }

        protected static string GetValue(IViewModel field)
        {
            PropertyInfo property = field.GetType().GetProperty("Value");
            object obj;
            if ((object)property == null)
            {
                obj = (object)null;
            }
            else
            {
                IViewModel viewModel = field;
                obj = property.GetValue((object)viewModel);
            }

            object postedValue = obj;
            if (postedValue == null)
            {
                return string.Empty;
            }

            return ParseFieldValue(postedValue);
        }

        protected static string ParseFieldValue(object postedValue)
        {
            Assert.ArgumentNotNull(postedValue, "postedValue");
            List<string> stringList = new List<string>();
            IList list = postedValue as IList;
            if (list != null)
            {
                foreach (object obj in (IEnumerable)list)
                    stringList.Add(obj.ToString());
            }
            else
            {
                stringList.Add(postedValue.ToString());
            }

            return string.Join(",", (IEnumerable<string>)stringList);
        }
    }
}