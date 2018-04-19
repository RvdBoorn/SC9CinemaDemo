using Sitecore.Analytics.Model;
using Sitecore.Diagnostics;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Processing;
using Sitecore.ExperienceForms.Processing.Actions;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Collection.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SC9Demo.Forms.SubmitActions
{
    public class UpdateContact : SubmitActionBase<string>
    {
        public UpdateContact(ISubmitActionData submitActionData) : base(submitActionData)
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
            if (email != null)
            {
                Sitecore.Analytics.Tracker.Current.Session.IdentifyAs("sitecoreextranet", GetValue(email));
                var manager = Sitecore.Configuration.Factory.CreateObject("tracking/contactManager", true) as Sitecore.Analytics.Tracking.ContactManager;

                if (manager != null)
                {
                    Sitecore.Analytics.Tracker.Current.Contact.ContactSaveMode = ContactSaveMode.AlwaysSave;
                    manager.SaveContactToCollectionDb(Sitecore.Analytics.Tracker.Current.Contact);

                    var trackerIdentifier = new IdentifiedContactReference("sitecoreextranet", GetValue(email));

                    using (Sitecore.XConnect.Client.XConnectClient client = Sitecore.XConnect.Client.Configuration.SitecoreXConnectClientConfiguration.GetClient())
                    {
                        try
                        {
                            var contact = client.Get<Sitecore.XConnect.Contact>(trackerIdentifier, new Sitecore.XConnect.ContactExpandOptions());

                            if (contact == null)
                            {
                                contact = new Contact(new ContactIdentifier("sitecoreextranet", GetValue(email), ContactIdentifierType.Known));
                            }

                            if (contact != null)
                            {
                                PersonalInformation personalInfoFacet = new PersonalInformation();

                                var firstname = fields.Where(x => x.Name.ToLower() == "first name").FirstOrDefault();
                                if (firstname != null)
                                {
                                    personalInfoFacet.FirstName = GetValue(firstname);
                                }

                                var lastname = fields.Where(x => x.Name.ToLower() == "last name").FirstOrDefault();
                                if (lastname != null)
                                {
                                    personalInfoFacet.LastName = GetValue(lastname);
                                }

                                EmailAddressList emailFacet = new EmailAddressList(new EmailAddress(GetValue(email), true), "Work");

                                var telephone = fields.Where(x => x.Name.ToLower() == "telephone").FirstOrDefault();
                                PhoneNumberList numberFacet = new PhoneNumberList(new PhoneNumber("+44", GetValue(telephone)), "Work");

                                client.SetFacet(contact, personalInfoFacet);
                                client.SetFacet(contact, emailFacet);
                                client.SetFacet(contact, numberFacet);

                                client.Submit();
                            }
                        }
                        catch (XdbExecutionException ex)
                        {
                            // Manage exceptions
                        }
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