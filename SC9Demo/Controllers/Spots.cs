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
    public class Spots : Sitecore9BaseController
    {

        public ActionResult PromoSpot()
        {
            return !IsDataSourceItemNull ? View(new PromoItem(DataSourceItem)) : ShowNoDataSourcePageEditorAlert();
        }

        public ActionResult FilmSpot()
        {
            return !IsDataSourceItemNull ? View(new Movie(DataSourceItem)) : ShowNoDataSourcePageEditorAlert();
        }

        private ActionResult ShowNoDataSourcePageEditorAlert()
        {
            return IsPageEditorEditing ? View("ShowPageEditorAlert", new PageEditorAlert(PageEditorAlert.Alerts.DatasourceIsNull)) : null;
        }
    }
}