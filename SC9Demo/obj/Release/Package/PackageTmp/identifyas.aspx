<% Sitecore.Analytics.Tracker.Current.Session.IdentifyAs("sitecoreextranet", "john@smith.com"); %>
<% Response.Redirect(Request.UrlReferrer.AbsoluteUri); %>