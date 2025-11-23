using System;
using System.Net.Http;
using System.Text;
using System.Drawing;
using System.Web.UI;
using Newtonsoft.Json;

namespace UserWebApp
{
    public partial class ResetPassword : Page
    {
        // change if your API runs on different url/port
        private const string ApiBase = "https://localhost:7107";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblMsg.Text = "";

                // if token missing, disable reset button
                string token = Request.QueryString["token"];
                if (string.IsNullOrEmpty(token))
                {
                    lblMsg.ForeColor = Color.Red;
                    lblMsg.Text = "Invalid or missing token.";
                    btnReset.Enabled = false;
                }
            }
        }

        // async event handler is allowed because Page has Async="true"
        protected async void btnReset_Click(object sender, EventArgs e)
        {
            lblMsg.Text = "";
            string token = Request.QueryString["token"];
            if (string.IsNullOrEmpty(token))
            {
                lblMsg.ForeColor = Color.Red;
                lblMsg.Text = "Invalid token.";
                return;
            }

            string newPass = txtNewPass.Text?.Trim() ?? "";
            string confirm = txtConfirm.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(newPass))
            {
                lblMsg.ForeColor = Color.Red;
                lblMsg.Text = "Please enter new password.";
                return;
            }

            if (newPass != confirm)
            {
                lblMsg.ForeColor = Color.Red;
                lblMsg.Text = "Passwords do not match.";
                return;
            }

            var payload = new { token = token, newPassword = newPass };

            try
            {
                using (var client = new HttpClient())
                {
                    string json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(ApiBase.TrimEnd('/') + "/api/usersregisters/reset-password", content);
                    var respText = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        lblMsg.ForeColor = Color.Green;
                        lblMsg.Text = "Password reset successfully! You can now login.";
                        btnReset.Enabled = false;
                    }
                    else
                    {
                        lblMsg.ForeColor = Color.OrangeRed;
                        lblMsg.Text = "Error: " + respText;
                    }
                }
            }
            catch (Exception ex)
            {
                lblMsg.ForeColor = Color.OrangeRed;
                lblMsg.Text = "Server error: " + ex.Message;
            }
        }

        // BACK button handler
        protected void btnBack_Click(object sender, EventArgs e)
        {
            try
            {
                // ResolveUrl handles the "~/" and spaces in folder names
                string url = ResolveUrl("~/Register Page/Login.aspx");
                Response.Redirect(url, false);
                // optionally complete the request to prevent ThreadAbortException
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                // Show error so you can debug if redirect fails
                lblMsg.ForeColor = Color.Red;
                lblMsg.Text = "Redirect failed: " + ex.Message;
            }
        }
    }
}
