using SC9Demo.Configuration.SiteUI;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;

namespace SC9Demo.Models
{
    public class SimpleNavigationItem : CustomItem
    {
        public SimpleNavigationItem(Item item) : base(item)
        {
            Assert.IsNotNull(item, "item");
        }

        public string LinkText
        {
            get
            {
                if (InnerItem.Template.Key == "footer links section")
                {
                    Item i = Sitecore.Context.Database.GetItem(InnerItem["Top Level Item"]);
                    return i["Menu Title"];
                }

                return InnerItem["Title"];
            }
        }

        public string Url
        {
            get
            {
                if (InnerItem.Template.Key == "footer links section")
                {
                    return InnerItem.GetLink("Top Level Item");
                }

                return LinkManager.GetItemUrl(InnerItem);

            }
        }



        public Item Item
        {
            get
            {
                if (InnerItem.Template.Key == "footer links section")
                {
                    return Sitecore.Context.Database.GetItem(InnerItem["Top Level Item"]);
                }
                return InnerItem;
            }
        }
    }
}