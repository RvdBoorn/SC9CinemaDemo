using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace SitecoreCinema.MarketingAutomation.Services
{

    public interface ISMSService
    {
        bool SendSMS(string phoneNumber, string message);
    }

    public class SMSService : ISMSService
    {
        public bool SendSMS(string phoneNumber, string message)
        {
            var result= SendMessage(phoneNumber, message);
            return result.Result;
        }

        private async System.Threading.Tasks.Task<bool> SendMessage(string phoneNumber, string message)
        {
            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    {"email", "john.penfold@gmail.com"},
                    {"password", "6pm9nT4L3nrH"},
                    {"device", "68540"},
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