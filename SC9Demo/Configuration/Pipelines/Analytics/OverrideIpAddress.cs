using Sitecore.Analytics;
using Sitecore.Analytics.Pipelines.CreateVisits;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SC9Demo.Configuration.Pipelines.Analytics
{
    public class OverrideIpAddress
    {
        public void Process(CreateVisitArgs args)
        {
            if (Tracker.Current == null || Tracker.Current.Interaction.Ip == null)
                return;
            Log.Info("Checking for static IP is set.", (object)this);
            string setting1 = Settings.GetSetting("GeoIP.OverrideLookupURL");
            string ipAddress1 = new IPAddress(Tracker.Current.Interaction.Ip).ToString();
            IPAddress address;
            if (!string.IsNullOrWhiteSpace(setting1) && IPAddress.TryParse(setting1, out address) && (ipAddress1 == "0.0.0.0" || ipAddress1 == "127.0.0.1" || this.IsPrivate(ipAddress1)))
            {
                this.SetIp(address.GetAddressBytes(), args);
                Log.Info(string.Format("Set static IP to {0}", (object)setting1), (object)this);
            }
            else
            {
                Log.Info("Checking if IP override is required.", (object)this);
                string setting2 = Settings.GetSetting("GeoIP.OverrideLookupURL");
                if (string.IsNullOrWhiteSpace(setting2))
                {
                    Log.Warn("IP Override reporting null tracker, IP, or GeoIP.OverrideLookupURL.", (object)this);
                }
                else
                {
                    Log.Info(string.Format("IP is {0}", (object)ipAddress1), (object)this);
                    if (ipAddress1 != "0.0.0.0" && ipAddress1 != "127.0.0.1" && !this.IsPrivate(ipAddress1))
                    {
                        Log.Info("The IP is not 0.0.0.0 or 127.0.0.1, no need to spoof.", (object)this);
                    }
                    else
                    {
                        try
                        {
                            Log.Info(string.Format("Getting public IP for the contact using {0} page.", (object)setting2), (object)this);
                            IPAddress ipAddress2 = IPAddress.Parse(WebUtil.ExecuteWebPage(setting2));
                            Log.Info(string.Format("IP identified as {0}", (object)ipAddress2.ToString()), (object)this);
                            this.SetIp(ipAddress2.GetAddressBytes(), args);
                            Log.Info(string.Format("Spoofed the local IP with {0}", (object)string.Join<byte>(".", (IEnumerable<byte>)Tracker.Current.Interaction.Ip)), (object)this);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Unable to spoof the IP.", (object)ex);
                        }
                    }
                }
            }
        }



        private void SetIp(byte[] ip, CreateVisitArgs args)
        {
            args.Interaction.Ip = ip;
        }

        private bool IsPrivate(string ipAddress)
        {
            string str1 = ipAddress;
            string[] separator = new string[1];
            int index = 0;
            string str2 = ".";
            separator[index] = str2;
            int num = 1;
            int[] numArray = Enumerable.ToArray<int>(Enumerable.Select<string, int>((IEnumerable<string>)str1.Split(separator, (StringSplitOptions)num), (Func<string, int>)(s => int.Parse(s))));
            return numArray[0] == 10 || numArray[0] == 192 && numArray[1] == 168 || numArray[0] == 172 && (numArray[1] >= 16 && numArray[1] <= 31);
        }
    }
}