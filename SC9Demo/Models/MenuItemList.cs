using SC9Demo.Configuration;
using Sitecore.Data.Items;
using Sitecore.Mvc.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SC9Demo.Models
{
    public class MenuItemList : IRenderingModel
    {
        public List<MenuItem> menuItems { get; protected set; }

        public void Initialize(Sitecore.Mvc.Presentation.Rendering rendering)
        {
            LoadMenuItems();
        }

        public List<MenuItem> LoadMenuItems()
        {
            menuItems = new List<MenuItem>();

            Item homeItem = SC9Demo.Configuration.SiteConfiguration.GetHomeItem();
            if (homeItem != null)
            {
                if (homeItem["Show Item in Menu"] == "1") menuItems.Add(new MenuItem(homeItem));
                foreach (Item item in homeItem.GetChildren().Where(x => x["Show Item in Menu"].Equals("1") && SiteConfiguration.DoesItemExistInCurrentLanguage(x)))
                {
                    menuItems.Add(new MenuItem(item));
                }
            }

            return menuItems;
        }
    }
}