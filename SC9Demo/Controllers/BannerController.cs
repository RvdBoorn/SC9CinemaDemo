using SC9Demo.Configuration;
using SC9Demo.Configuration.SiteUI;
using SC9Demo.Configuration.SiteUI.Base;
using SC9Demo.Models;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SC9Demo.Controllers
{
    public class BannerController : Sitecore9BaseController
    {
        public ActionResult BlurSlider()
        {
            return !IsDataSourceItemNull ? View(UIHelpers.GetPropertyValue("view", "BlurSlider"), new BannerItem(DataSourceItem)) : ShowNoDataSourcePageEditorAlert();
        }

        public ActionResult RetroTextBanner()
        {
            return !IsDataSourceItemNull ? View(UIHelpers.GetPropertyValue("view", "RetroTextBanner"), new BannerItem(DataSourceItem)) : ShowNoDataSourcePageEditorAlert();
        }

        private ActionResult ShowNoDataSourcePageEditorAlert()
        {
            return IsPageEditorEditing ? View("ShowPageEditorAlert", new PageEditorAlert(PageEditorAlert.Alerts.DatasourceIsNull)) : null;
        }
    }
}