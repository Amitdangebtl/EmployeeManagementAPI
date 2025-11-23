using System;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using Newtonsoft.Json.Linq;

namespace UserWebApp
{
    public partial class SiteMaster : MasterPage
    {
        private void DisablePageCaching()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            DisablePageCaching();

            bool isLoggedIn = Session["AuthUser"] != null || Session["UserEmail"] != null;

            // safe show/hide (controls exist in markup)
            if (lnkLogin != null) lnkLogin.Visible = !isLoggedIn;
            if (lnkRegister != null) lnkRegister.Visible = !isLoggedIn;
            if (lnkUsers != null) lnkUsers.Visible = isLoggedIn;
            if (btnLogout != null) btnLogout.Visible = isLoggedIn;

            // default hide username
            if (lblUserName != null) { lblUserName.Text = ""; lblUserName.Visible = false; }

            if (!isLoggedIn) return;

            try
            {
                object auth = Session["AuthUser"];
                string first = null, last = null;

                if (auth != null)
                {
                    // JObject (if stored)
                    if (auth is JObject jo)
                    {
                        first = TryGetString(jo, "FirstName", "firstName", "name");
                        last = TryGetString(jo, "LastName", "lastName", "surname");
                        if (string.IsNullOrWhiteSpace(first) && jo["user"] is JObject uj) first = TryGetString(uj, "FirstName", "firstName", "name");
                    }
                    else if (auth is string s)
                    {
                        try
                        {
                            var parsed = JObject.Parse(s);
                            first = TryGetString(parsed, "FirstName", "firstName", "name");
                            last = TryGetString(parsed, "LastName", "lastName", "surname");
                        }
                        catch { }
                    }
                    else if (auth is IDictionary dict)
                    {
                        first = TryGetFromDict(dict, "FirstName", "firstName", "name");
                        last = TryGetFromDict(dict, "LastName", "lastName", "surname");
                    }
                    else
                    {
                        // typed object via reflection
                        try
                        {
                            var t = auth.GetType();
                            var p1 = t.GetProperty("FirstName") ?? t.GetProperty("firstName") ?? t.GetProperty("Name");
                            var p2 = t.GetProperty("LastName") ?? t.GetProperty("lastName") ?? t.GetProperty("Surname");
                            if (p1 != null) first = ConvertToStringSafe(p1.GetValue(auth));
                            if (p2 != null) last = ConvertToStringSafe(p2.GetValue(auth));

                            var pu = t.GetProperty("user") ?? t.GetProperty("User");
                            if ((string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last)) && pu != null)
                            {
                                var userObj = pu.GetValue(auth);
                                if (userObj != null)
                                {
                                    var ut = userObj.GetType();
                                    var uf = ut.GetProperty("FirstName") ?? ut.GetProperty("firstName") ?? ut.GetProperty("Name");
                                    var ul = ut.GetProperty("LastName") ?? ut.GetProperty("lastName") ?? ut.GetProperty("Surname");
                                    if (uf != null && string.IsNullOrWhiteSpace(first)) first = ConvertToStringSafe(uf.GetValue(userObj));
                                    if (ul != null && string.IsNullOrWhiteSpace(last)) last = ConvertToStringSafe(ul.GetValue(userObj));
                                }
                            }
                        }
                        catch { }
                    }
                }

                string full = null;
                if (!string.IsNullOrWhiteSpace(first)) full = first.Trim();
                if (!string.IsNullOrWhiteSpace(last)) full = string.IsNullOrWhiteSpace(full) ? last.Trim() : (full + " " + last.Trim());

                // Do not display raw email — only names
                if (!string.IsNullOrWhiteSpace(full) && !full.Contains("@"))
                {
                    lblUserName.Text = HttpUtility.HtmlEncode(full);
                    lblUserName.Visible = true;
                }
            }
            catch
            {
                if (lblUserName != null) { lblUserName.Text = ""; lblUserName.Visible = false; }
            }
        }

        private static string TryGetString(JObject jo, params string[] keys)
        {
            if (jo == null) return null;
            foreach (var k in keys)
            {
                if (jo.TryGetValue(k, StringComparison.OrdinalIgnoreCase, out JToken t) && t != null && t.Type != JTokenType.Null)
                {
                    var v = t.ToString();
                    if (!string.IsNullOrWhiteSpace(v)) return v;
                }
            }
            return null;
        }

        private static string TryGetFromDict(IDictionary dict, params string[] keys)
        {
            if (dict == null) return null;
            foreach (var k in keys)
            {
                if (dict.Contains(k) && dict[k] != null)
                {
                    var v = dict[k].ToString();
                    if (!string.IsNullOrWhiteSpace(v)) return v;
                }
            }
            return null;
        }

        private static string ConvertToStringSafe(object o)
        {
            if (o == null) return null;
            try { return o.ToString(); } catch { return null; }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            try
            {
                Session.Remove("AuthUser");
                Session.Remove("UserEmail");
                Session.Remove("UserID");
                Session.Remove("RoleName");
                Session.Abandon();
            }
            catch { }

            try { FormsAuthentication.SignOut(); } catch { }

            Response.Redirect("~/Register%20Page/Login.aspx");
        }
    }
}
