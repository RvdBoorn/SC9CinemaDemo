using SC9Demo.Configuration;
using Sitecore.Analytics;
using Sitecore.Analytics.Tracking;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Mvc.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.CES.DeviceDetection;

namespace SC9Demo.Models
{
    public class VisitInformation
    {
        public string PageCount
        {
            get { return Convert.ToString(Tracker.Current.Interaction.PageCount); }
        }
        public string Visits
        {
            get
            {
                return Convert.ToString(Tracker.Current.Contact.System?.VisitCount ?? 1);
            }
        }
        public string ContactID
        {
            get
            {
                return Tracker.Current.Interaction.ContactId.ToString();
            }
        }
        public string Device
        {
            get
            {
                if (!DeviceDetectionManager.IsEnabled || !DeviceDetectionManager.IsReady || string.IsNullOrEmpty(Tracker.Current.Interaction.UserAgent))
                {
                    return string.Empty;
                }

                var deviceInfo = DeviceDetectionManager.GetDeviceInformation(Tracker.Current.Interaction.UserAgent);
                return string.Join(", ", deviceInfo.DeviceOperatingSystemVendor, deviceInfo.DeviceModelName) + "(" + deviceInfo.Browser + ")";
            }
        }

        public string ContactIdentifier
        {
            get
            {
                if (Tracker.Current.Contact.Identifiers.Count() > 0)
                {
                    return Tracker.Current.Contact.Identifiers.FirstOrDefault().Identifier;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string EngagementValue
        {
            get { return Convert.ToString(Tracker.Current.Interaction.Value); }
        }

        public string Campaign
        {
            get
            {
                // the call to Tracker.CurrentVisit.CampaignId will either work or throw       
                try
                {
                    if (Tracker.Current.Interaction.CampaignId.HasValue)
                    {
                        Item campaign = Sitecore.Context.Database.GetItem(new ID(Tracker.Current.Interaction.CampaignId.Value));
                        return campaign.Name;
                    }
                }
                catch { }

                return SiteConfiguration.GetDictionaryText("Current Campaign Empty");
            }
        }

        public string City
        {
            get { return Tracker.Current.Interaction.HasGeoIpData ? Tracker.Current.Interaction.GeoData.City : SiteConfiguration.GetDictionaryText("Pending Lookup"); }
        }

        public string PostalCode
        {
            get { return Tracker.Current.Interaction.HasGeoIpData ? Tracker.Current.Interaction.GeoData.PostalCode : SiteConfiguration.GetDictionaryText("Pending Lookup"); }
        }


        public string BusinessName
        {
            get { return Tracker.Current.Interaction.HasGeoIpData ? Tracker.Current.Interaction.GeoData.BusinessName : SiteConfiguration.GetDictionaryText("Pending Lookup"); }
        }

        public List<PatternMatch> PatternMatches { get { return LoadPatterns(); } }

        public List<GenericLink> PagesViewed { get { return LoadPages(); } }

        public List<PatternMatch> LoadPatterns()
        {
            List<PatternMatch> patternMatches = new List<PatternMatch>();

            if (Tracker.IsActive)
            {
                if (SiteConfiguration.GetSiteSettingsItem() != null)
                {
                    MultilistField profiles = SiteConfiguration.GetSiteSettingsItem().Fields["Visible Profiles"];
                    foreach (Item visibleProfile in profiles.GetItems())
                    {
                        Item visibleProfileItem = Sitecore.Context.Database.GetItem(visibleProfile.ID);
                        if (visibleProfileItem != null)
                        {
                            // show the pattern match if there is one.
                            var userPattern = Tracker.Current.Interaction.Profiles[visibleProfileItem.Name];
                            if (userPattern != null)
                            {
                                // load the details about the matching pattern
                                Item matchingPattern = Sitecore.Context.Database.GetItem(userPattern.PatternId.ToId());
                                if (matchingPattern != null)
                                {
                                    Sitecore.Data.Items.MediaItem image = new Sitecore.Data.Items.MediaItem(((ImageField)matchingPattern.Fields["Image"]).MediaItem);
                                    string src = Sitecore.StringUtil.EnsurePrefix('/', Sitecore.Resources.Media.MediaManager.GetMediaUrl(image));
                                    patternMatches.Add(new PatternMatch(visibleProfileItem["Name"], matchingPattern.Name, src));
                                }
                            }
                        }
                    }
                }
            }
            return patternMatches;
        }

        public List<GenericLink> LoadPages()
        {
            List<GenericLink> pagesViewed = new List<GenericLink>();

            foreach (IPageContext page in Tracker.Current.Interaction.GetPages())
            {
                GenericLink link = new GenericLink(CleanPageName(page), page.Url.Path, false);
                pagesViewed.Add(link);
            }
            pagesViewed.Reverse();
            return pagesViewed;
        }





        public List<Goal> PageGoals()
        {
            List<Goal> goals = new List<Goal>();
            var list = Tracker.Current.Interaction.GetPages().SelectMany(page => page.PageEvents.Where(y => y.IsGoal)).ToList().Take(5);
            foreach (var a in list)
            {
                var g = new Goal()
                {
                    GoalName = Sitecore.Context.Database.GetItem(ID.Parse(a.PageEventDefinitionId)).Name
                    ,
                    DateT = a.DateTime
                    ,
                    Points = Sitecore.Context.Database.GetItem(ID.Parse(a.PageEventDefinitionId)).Fields["Points"].Value
                };

                goals.Add(g);
            }

            return goals;
        }

        public List<string> LoadGoals()
        {
            List<string> goals = new List<string>();

            var conversions = (from page in Tracker.Current.Interaction.GetPages()
                               from pageEventData in page.PageEvents
                               where pageEventData.IsGoal
                               select pageEventData).ToList();

            if (conversions.Any())
            {
                conversions.Reverse();
                foreach (var goal in conversions)
                {
                    goals.Add(String.Format("{0} ({1})", goal.Name, goal.Value));
                }
            }
            else
            {
                goals.Add(SiteConfiguration.GetDictionaryText("No Goals"));
            }

            return goals;
        }

        private string CleanPageName(IPageContext p)
        {
            string pageName = p.Url.Path.Replace("/en", "/").Replace("//", "/").Remove(0, 1).Replace(".aspx", "");
            if (pageName == String.Empty || pageName == "en") pageName = "Home";
            if (pageName.IndexOf("/") != pageName.LastIndexOf("/"))
            {
                pageName = pageName.Substring(0, pageName.IndexOf("/") + 1) + "..." + pageName.Substring(pageName.LastIndexOf("/"));
            }
            return (pageName.Length < 27) ? String.Format("{0} ({1}s)", pageName, (p.Duration / 1000.0).ToString("f2")) :
                String.Format("{0}... ({1}s)", pageName.Substring(0, 26), (p.Duration / 1000.0).ToString("f2"));
        }

        public class ProfileKeyValues
        {
            public ProfileKeyValues() { }

            public string Profile { get; set; }

            public List<KeyValuePair<string, float>> Keys
            {
                get;
                set;
            }
        }


        public class PatternMatch
        {
            public PatternMatch() { }
            public PatternMatch(string profile, string pattern, string image)
            {
                Profile = profile;
                PatternName = pattern;
                Image = image;
            }
            public string Profile { get; set; }
            public string PatternName { get; set; }
            public string Image { get; set; }
        }

        public class Goal
        {
            public Goal() { }
            public string GoalName { get; set; }
            public DateTime DateT { get; set; }
            public string Points { get; set; }
        }

    }
}