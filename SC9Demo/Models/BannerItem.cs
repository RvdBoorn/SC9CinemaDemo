using SC9Demo.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SC9Demo.Configuration.SiteUI;
using Sitecore.Resources.Media;

namespace SC9Demo.Models
{
    public class BannerItem : CustomItem
    {
        public BannerItem(Item item) : base(item)
        {
            Assert.IsNotNull(item, "item");
        }

        public string Title
        {
            get { return InnerItem[FieldId.Title]; }
        }

        public string Caption
        {
            get { return InnerItem[FieldId.Caption]; }
        }

        public string LinkText
        {
            get { return InnerItem[FieldId.LinkText]; }
        }

        public string LinkItem
        {
            get { return InnerItem[FieldId.LinkItem]; }
        }

        public string Image
        {
            get { return InnerItem[FieldId.Image]; }
        }

        public string ImageUrl
        {
            get
            {
                Sitecore.Data.Fields.FileField media = ((Sitecore.Data.Fields.FileField)InnerItem.Fields["Image"]);
                return MediaManager.GetMediaUrl(media.MediaItem, new MediaUrlOptions() { MaxWidth=1950});
            }
        }

        public Item Item
        {
            get { return InnerItem; }
        }

        public string LinkItemUrl
        {
            get { return InnerItem.GetLink("Link Item"); }
        }

        public IEnumerable<BannerItem> ChildrenInCurrentLanguage
        {
            get
            {
                return InnerItem.Children.Select(x => new BannerItem(x)).Where(x => SiteConfiguration.DoesItemExistInCurrentLanguage(x.Item));
            }
        }

        public IEnumerable<BannerItem> Children
        {
            get
            {
                return InnerItem.Children.Select(x => new BannerItem(x));
            }
        }

        public static class FieldId
        {
            public static readonly ID Title = new ID("{1FD34C98-05B1-4206-A48E-14D1E0C42F6F}");
            public static readonly ID Caption = new ID("{9B14A853-F008-4145-878A-DD5A2ED7EC36}");
            public static readonly ID LinkText = new ID("{48152DDC-82D7-4E74-9B1D-EC816D278AB4}");
            public static readonly ID LinkItem = new ID("{336E27E0-7F9C-437E-B179-653F427520CC}");
            public static readonly ID Image = new ID("{DD876753-9D4B-4B70-8197-41A0EB302087}");
            public static readonly ID UseDarkText = new ID("{B1F1502E-0B97-4152-92ED-6828FA25B947}");
            public static readonly ID TitleColour = new ID("{B40AC4DD-F6DD-4973-9A74-9AF327BDC1CB}");
            public static readonly ID CaptionColour = new ID("{F5DCA402-4072-4323-AEC7-E131EDFB3F3B}");
            public static readonly ID ShowTextBox = new ID("{80D5008A-B494-416A-A899-7AC365B445BB}");
            public static readonly ID TextBoxColour = new ID("{BE7EEC32-A86F-485C-AA4B-A01FFD948714}");
            public static readonly ID ShowShadow = new ID("{58A21634-0707-4A9E-923B-99F1247856B5}");
            public static readonly ID Position = new ID("{91044841-6FD9-4655-B811-5C967B0AA65D}");
        }
    }
}