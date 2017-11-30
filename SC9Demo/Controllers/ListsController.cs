using SC9Demo.Configuration;
using SC9Demo.Configuration.SiteUI.Base;
using SC9Demo.Configuration.SiteUI;
using SC9Demo.Models;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sitecore.Data.Fields;
using Sitecore;

using System.Configuration;
using System.Data.SqlClient;

using Sitecore.Data;


namespace SC9Demo.Controllers
{
    public class ListsController : Sitecore9BaseController
    {
        public ActionResult MoviesList()
        {
            IEnumerable<Movie> items = new Movie(DataSourceItemOrCurrentItem).ChildrenInCurrentLanguage;
            return !items.IsNullOrEmpty() ? View(UIHelpers.GetPropertyValue("view", "MoviesList"), items) : ShowListIsEmptyPageEditorAlert();
        }

        private ActionResult ShowListIsEmptyPageEditorAlert()
        {
            return IsPageEditorEditing ? View("ShowPageEditorAlert", new PageEditorAlert(PageEditorAlert.Alerts.ListIsEmpty)) : null;
        }
    }
}