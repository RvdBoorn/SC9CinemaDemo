using SC9Demo.Configuration;
using SC9Demo.Configuration.SiteUI.Base;
using SC9Demo.Configuration.SiteUI;
using SC9Demo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sitecore.Data.Items;
using Sitecore.Links;


namespace SC9Demo.Controllers
{
    public class NavigationController : Sitecore9BaseController
    {
        public ActionResult Breadcrumbs()
        {
            if (Sitecore.Context.Item.ID != SiteConfiguration.GetHomeItem().ID)
            {
                List<SimpleItem> items = new List<SimpleItem>();
                Item temp = Sitecore.Context.Item;

                while (temp.ID != SiteConfiguration.GetHomeItem().ParentID)
                {
                    items.Add(new SimpleItem(temp));
                    temp = temp.Parent;
                }

                items.Reverse();
                return View(items);
            }
            return null;
        }

        public ActionResult FooterNavigation()
        {
            Item baseItem = SiteConfiguration.GetFooterLinksItem();
            List<SimpleNavigationItem> items = new List<SimpleNavigationItem>();
            foreach (Item footerLink in baseItem.Children)
            {
                Item i = Sitecore.Context.Database.GetItem(footerLink["Top Level Item"]);
                if (i != null && SiteConfiguration.DoesItemExistInCurrentLanguage(i)) { items.Add(new SimpleNavigationItem(i)); }
            }

            return View(items);
        }

        public ActionResult NavigationBar()
        {
            var items = new MenuItemList().LoadMenuItems();
            return View(items);
        }

        public ActionResult SecondaryNavigation()
        {
            Item home = SiteConfiguration.GetHomeItem();
            Item dataSource = Sitecore.Context.Item;
            if (home.ID != dataSource.ID)  // if on the home node, just use it
            {
                while (dataSource.ParentID != home.ID)
                    dataSource = dataSource.Parent;
            }

            MenuItem ds = new MenuItem(dataSource);
            return (dataSource != null && ds.HasChildrenToShowInSecondaryMenu) ? View(ds) : ShowListIsEmptyPageEditorAlert();
        }

        public ActionResult SecondaryNavigationQuery()
        {
            if (IsDataSourceItemNull) return ShowListIsEmptyPageEditorAlert();

            IEnumerable<MenuItem> items = DataSourceItems.Select(x => new MenuItem(x)).Where(x => SiteConfiguration.DoesItemExistInCurrentLanguage(x.Item));
            return !items.IsNullOrEmpty() ? View("SecondaryNavigation", items) : ShowListIsEmptyPageEditorAlert();
        }

        public ActionResult TertiaryNavigation()
        {
            // microsites are small so we are limiting the top area.
            if (SiteConfiguration.IsMicrosite())
            {
                return View("TertiaryNavigationMicrosite");
            }
            else
            {
                if (Sitecore.Context.IsLoggedIn)
                {
                    if (Sitecore.Context.Domain.Name.ToLower() == "extranet" || Sitecore.Context.Domain.Name.ToLower() == "salesforce")
                    {
                        return View("TertiaryNavigationAuthenticated");
                    }
                    else
                    {
                        return View("TertiaryNavigationAnonymous");
                    }
                }

                return View("TertiaryNavigationAnonymous");

            }
        }


        private ActionResult ShowListIsEmptyPageEditorAlert()
        {
            return IsPageEditorEditing ? View("ShowPageEditorAlert", new PageEditorAlert(PageEditorAlert.Alerts.ListIsEmpty)) : null;
        }
    }
}