using SC9Demo.Configuration.SiteUI.Base;
using SC9Demo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SC9Demo.Controllers
{
    public class VisitController : Sitecore9BaseController
    {
        public ActionResult VisitDetails()
        {
            return Sitecore.Context.PageMode.IsNormal ? View("VisitDetails", new VisitInformation()) : ShowEditorAlert();
        }
        private ActionResult ShowEditorAlert()
        {
            return IsPageEditorEditing ? View("ShowPageEditorAlert", new PageEditorAlert(PageEditorAlert.Alerts.VisitDetailsNotAllowedInPageEditor)) : null;
        }
    }
}