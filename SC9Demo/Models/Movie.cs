using SC9Demo.Configuration;
using SC9Demo.Configuration.SiteUI;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SC9Demo.Models
{
    public class Movie : CustomItem
    {
        public Movie(Item item) : base(item)
        {
            Assert.IsNotNull(item, "item");
        }

        public string Title
        {
            get { return InnerItem[FieldId.Title]; }
        }

        public string Text
        {
            get { return InnerItem[FieldId.Text]; }
        }

        public string Poster
        {
            get { return InnerItem[FieldId.Poster]; }
        }
        public Item Item
        {
            get { return InnerItem; }
        }

        public string Url
        {
            get { return LinkManager.GetItemUrl(InnerItem); }
        }

        public IEnumerable<string> Genre
        {
            get
            {
                List<string> itemGenres = new List<string>();

                MultilistField genres = InnerItem.Fields[FieldId.Genre];
                if(genres != null)
                {
                    foreach(var item in genres.GetItems())
                    {
                        itemGenres.Add(item.Name);
                    }
                }

                return itemGenres;
            }
        }

        public IEnumerable<Movie> ChildrenInCurrentLanguage
        {
            get
            {
                return InnerItem.Children.Select(x => new Movie(x)).Where(x => SiteConfiguration.DoesItemExistInCurrentLanguage(x.Item));
            }
        }

        public IEnumerable<Movie> Children
        {
            get
            {
                return InnerItem.Children.Select(x => new Movie(x));
            }
        }

        public static class FieldId
        {
            public static readonly ID Title = new ID("{BE8278A4-8653-4F35-8A25-6E089D8E3462}");
            public static readonly ID Text = new ID("{E96DFA10-FA94-4B0F-9612-27FC15439797}");
            public static readonly ID Icon = new ID("{2B60D8C1-81DB-45A7-B1CB-654CDDA96AE3}");
            public static readonly ID Poster = new ID("{02DEF5BD-D79A-4AFC-9D2B-958713AC2C93}");
            public static readonly ID Genre = new ID("{F0BC9308-B0B8-4A76-9E03-B0D258440154}");
        }
    }
}