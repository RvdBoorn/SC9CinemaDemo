using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace SitecoreCinema.MarketingAutomation.Services
{

    public interface ISMSService
    {
        bool SendSMS(string phoneNumber, string message, string firstName);
    }

    public class SMSService : ISMSService
    {
        public bool SendSMS(string phoneNumber, string message, string firstName)
        {
            var result= SendMessage(phoneNumber, message, firstName);
            return result.Result;
        }

        private async System.Threading.Tasks.Task<bool> SendMessage(string phoneNumber, string message, string firstName)
        {

           using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    {"email", ConfigurationManager.AppSettings["SmsGatewayEmail"]},
                    {"password", ConfigurationManager.AppSettings["SmsGatewayPassword"]},
                    {"device", ConfigurationManager.AppSettings["SmsGatewayDeviceId"]},
                    {"number", phoneNumber},
                    {"message", message }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("http://smsgateway.me/api/v3/messages/send", content);
                var responseString = await response.Content.ReadAsByteArrayAsync();

                return response.IsSuccessStatusCode;
            }
        }
    }
}