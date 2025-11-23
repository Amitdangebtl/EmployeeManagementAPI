using System;
using System.Web.UI;

namespace UserWebApp
{
    public partial class Site_Mobile : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var isLoggedIn = Session["UserEmail"] != null || Session["AuthUser"] != null;
                var email = Session["UserEmail"] as string;
                string displayName = null;

                // Best-effort: prefer AuthUser.FirstName (if set), then email
                if (Session["AuthUser"] != null)
                {
                    try
                    {
                        // Try dynamic access (works if you stored anonymous / dynamic object)
                        dynamic auth = Session["AuthUser"];
                        // FirstName may be null or empty
                        if (auth != null)
                        {
                            try
                            {
                                var fn = (string)auth.FirstName;
                                var ln = (string)auth.LastName;
                                if (!string.IsNullOrWhiteSpace(fn))
                                    displayName = fn + (string.IsNullOrWhiteSpace(ln) ? "" : " " + ln);
                            }
                            catch { /* ignore property access issues */ }
                        }
                    }
                    catch { /* swallow */ }
                }

                // fallback to email
                if (string.IsNullOrWhiteSpace(displayName) && !string.IsNullOrWhiteSpace(email))
                    displayName = email;

                // UI visibility
                lnkRegister.Visible = !isLoggedIn;
                lnkLogin.Visible = !isLoggedIn;

                lnkLogout.Visible = isLoggedIn;
                lnkUsers.Visible = isLoggedIn;

                if (isLoggedIn && !string.IsNullOrWhiteSpace(displayName))
                {
                    // show badge with initials
                    pnlUserInfo.Visible = true;
                    lblWelcome.Text = "Hello, " + Server.HtmlEncode(displayName);

                    // set initials inside the spUserIcon element (first char of first name)
                    try
                    {
                        var firstChar = displayName.Trim()[0];
                        spUserIcon.InnerText = Server.HtmlEncode(firstChar.ToString().ToUpper());
                    }
                    catch
                    {
                        spUserIcon.InnerText = "U";
                    }
                }
                else
                {
                    pnlUserInfo.Visible = false;
                    lblWelcome.Text = string.Empty;
                }
            }
            catch
            {
                // fail-safe: don't break page on unexpected session shape
                lnkRegister.Visible = true;
                lnkLogin.Visible = true;
                lnkLogout.Visible = false;
                lnkUsers.Visible = false;
                pnlUserInfo.Visible = false;
            }
        }

        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            // Clear session and redirect to login
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login%20Page/Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}

