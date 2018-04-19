using Sitecore.Xdb.MarketingAutomation.Core.Activity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.XConnect.Collection.Model;
using Sitecore.XConnect;
using Sitecore.Xdb.MarketingAutomation.Core.Activity;
using Sitecore.Xdb.MarketingAutomation.Core.Processing.Plan;
using SitecoreCinema.MarketingAutomation.Services;
using System.Net;


namespace SitecoreCinema.MarketingAutomation.Activity
{
    public class SendSMSActivity : IActivity
    {
        public IActivityServices Services
        {
            get; set;
        }

        public ISMSService SMSService
        {
            get; set;
        }

        public SendSMSActivity(ISMSService smsService)
        {
            SMSService = smsService;
        }

        public string smsmessagetext { get; set; }

        public ActivityResult Invoke(IContactProcessingContext context)
        {
            var contact = context.Contact;

            if (!contact.IsKnown) // if unknown we can't know their phone number
            {
                return (ActivityResult)new SuccessMove();
            }

            if (!contact.ExpandOptions.FacetKeys.Contains(PhoneNumberList.DefaultFacetKey, StringComparer.InvariantCultureIgnoreCase))
            {
                var expandOptions = new ContactExpandOptions(PhoneNumberList.DefaultFacetKey, PersonalInformation.DefaultFacetKey);
                contact = Services.Collection.GetContactAsync(contact, expandOptions).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            if(string.IsNullOrEmpty(contact.PhoneNumbers().PreferredPhoneNumber.Number)) // No phone number
            {
                return (ActivityResult)new SuccessMove();
            }



            string phoneNumber = contact.PhoneNumbers().PreferredPhoneNumber.Number;
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                if (this.smsmessagetext == string.Empty)
                {
                    this.smsmessagetext = "You have reserved a cinema ticket";
                }


                var result = SMSService.SendSMS(phoneNumber, this.smsmessagetext, contact.Personal().FirstName);

            }
            
            return (ActivityResult)new SuccessMove();
        }
    }
}