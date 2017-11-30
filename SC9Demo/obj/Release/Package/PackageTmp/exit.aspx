<%@ Import Namespace="System" %>
<%@ Page Language="c#"%>

<script runat="server">
void Page_Load(object sender, System.EventArgs e)  
{      

   HttpContext.Current.Session.Abandon();
   Response.Redirect("/");
} 
</script>
