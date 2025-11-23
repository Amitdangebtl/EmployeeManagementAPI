using System;
using System.Net.Http;
using System.Text;
using System.Web.UI;
using Newtonsoft.Json;

namespace UserWebApp
{
    public partial class ForgotPassword : Page
    {
        // Change this to match the actual URL/port where your API runs
        private const string ApiBase = "https://localhost:7107";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblMsg.Text = "";
            }
        }

        protected async void btnSend_Click(object sender, EventArgs e)
        {
            lblMsg.Text = "";
            string email = txtEmail.Text?.Trim();

            if (string.IsNullOrEmpty(email))
            {
                lblMsg.Text = "Please enter email.";
                lblMsg.ForeColor = System.Drawing.Color.Red;
                return;
            }

            var data = new { email = email };

            try
            {
                using (var client = new HttpClient())
                {
                    string json = JsonConvert.SerializeObject(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(ApiBase.TrimEnd('/') + "/api/usersregisters/send-reset-link", content);
                    var msg = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        lblMsg.ForeColor = System.Drawing.Color.Green;
                        lblMsg.Text = "Reset link sent (if the email exists).";
                    }
                    else
                    {
                        lblMsg.ForeColor = System.Drawing.Color.OrangeRed;
                        lblMsg.Text = "Error: " + msg;
                    }
                }
            }
            catch (Exception ex)
            {
                lblMsg.ForeColor = System.Drawing.Color.OrangeRed;
                lblMsg.Text = "Server error: " + ex.Message;
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Register%20Page/Login.aspx");
        }
    }
}
