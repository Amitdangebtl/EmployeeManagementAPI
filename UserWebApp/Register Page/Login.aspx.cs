using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Threading.Tasks;

namespace UserWebApp.LoginPage
{
    public partial class Login : System.Web.UI.Page
    {
        private void DisablePageCaching()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
        }

        private bool IsLoggedIn =>
            Session["UserEmail"] != null || Session["AuthUser"] != null;

        protected void Page_Load(object sender, EventArgs e)
        {
            DisablePageCaching();

            if (IsLoggedIn)
            {
                Response.Redirect("~/Register%20Page/UsersList.aspx");
                return;
            }

            if (!IsPostBack)
            {
                if (Session["SuccessMessage"] != null)
                {
                    lblLoginMsg.ForeColor = Color.Green;
                    lblLoginMsg.Text = "✅ " + Session["SuccessMessage"].ToString();
                    Session["SuccessMessage"] = null;
                }
            }
        }

        protected async void btnLogin_Click(object sender, EventArgs e)
        {
            lblLoginMsg.Text = "";
            lblLoginMsg.ForeColor = Color.Red;

            try
            {
                if (string.IsNullOrWhiteSpace(txtEmail.Text) ||
                    string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    lblLoginMsg.Text = "Email and Password are required.";
                    return;
                }

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    lblLoginMsg.Text = "ApiBaseUrl missing in Web.config.";
                    return;
                }

                string url = baseUrl.TrimEnd('/') + "/api/usersregisters/login";

                var payload = new
                {
                    email = txtEmail.Text.Trim(),
                    password = txtPassword.Text
                };

                string json = JsonConvert.SerializeObject(payload);

                using (var client = new HttpClient())
                using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    var res = await client.PostAsync(url, content);
                    var body = await res.Content.ReadAsStringAsync();

                    // ----------------------------------------
                    // SUCCESS LOGIN RESPONSE
                    // ----------------------------------------
                    if (res.IsSuccessStatusCode)
                    {
                        JObject jo = null;
                        try { jo = string.IsNullOrWhiteSpace(body) ? null : JObject.Parse(body); }
                        catch { jo = null; }

                        // attempt to find verification flag in many shapes
                        bool? isVerified = null;
                        if (jo != null)
                        {
                            isVerified = GetBoolFromJObject(jo, "IsVerified", "isVerified", "is_verified");
                        }

                        // If NOT VERIFIED -> resend token + block login
                        if (isVerified.HasValue && isVerified.Value == false)
                        {
                            try
                            {
                                string token = await GenerateAndGetTokenFromDbAsync(txtEmail.Text.Trim());

                                string firstNameForEmail = TryGetNameFromJObject(jo).firstName ?? "User";

                                if (!string.IsNullOrWhiteSpace(token))
                                {
                                    await SendVerificationEmailAsync(txtEmail.Text.Trim(), firstNameForEmail, token);
                                }
                            }
                            catch (Exception ex)
                            {
                                lblLoginMsg.Text = "Account not verified. Could not resend verification email: " +
                                                   HttpUtility.HtmlEncode(ex.Message);
                                return;
                            }

                            lblLoginMsg.Text = "Please verify your email before logging in. A new verification link has been sent.";
                            return;
                        }

                        // -------------------------------------------------------
                        // ✔ LOGIN VERIFIED → Extract user info robustly
                        // -------------------------------------------------------
                        // defaults
                        int userId = 0;
                        string firstName = null, lastName = null, email = txtEmail.Text.Trim();

                        // Many API responses put user data directly or under "user", "data", or nested nodes.
                        if (jo != null)
                        {
                            // try direct primitive fields
                            if (jo["UserID"] != null) TryParseInt(jo["UserID"], ref userId);
                            firstName = TryGetString(jo, "FirstName", "first_name", "firstName", "name", "fullName", "username");
                            lastName = TryGetString(jo, "LastName", "last_name", "lastName", "surname", "familyName");

                            if (string.IsNullOrWhiteSpace(email)) email = TryGetString(jo, "Email", "email");

                            // common nested containers
                            if ((string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || userId == 0 || string.IsNullOrWhiteSpace(email))
                                && jo["user"] is JObject u1)
                            {
                                if (userId == 0 && u1["UserID"] != null) TryParseInt(u1["UserID"], ref userId);
                                firstName = firstName ?? TryGetString(u1, "FirstName", "first_name", "firstName", "name", "username");
                                lastName = lastName ?? TryGetString(u1, "LastName", "last_name", "lastName", "surname");
                                email = string.IsNullOrWhiteSpace(email) ? TryGetString(u1, "Email", "email") : email;
                            }

                            if ((string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || userId == 0 || string.IsNullOrWhiteSpace(email))
                                && jo["data"] is JObject u2)
                            {
                                if (userId == 0 && u2["UserID"] != null) TryParseInt(u2["UserID"], ref userId);
                                firstName = firstName ?? TryGetString(u2, "FirstName", "first_name", "firstName", "name", "username");
                                lastName = lastName ?? TryGetString(u2, "LastName", "last_name", "lastName", "surname");
                                email = string.IsNullOrWhiteSpace(email) ? TryGetString(u2, "Email", "email") : email;
                            }

                            // If jo itself is an object with nested "result" etc.
                            if ((string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || userId == 0 || string.IsNullOrWhiteSpace(email))
                                && jo.HasValues)
                            {
                                // scan properties to find an object that looks like a user
                                foreach (var prop in jo.Properties())
                                {
                                    if (prop.Value is JObject candidate)
                                    {
                                        var maybeName = TryGetString(candidate, "FirstName", "first_name", "firstName", "name", "username");
                                        if (!string.IsNullOrWhiteSpace(maybeName))
                                        {
                                            firstName = firstName ?? maybeName;
                                            lastName = lastName ?? TryGetString(candidate, "LastName", "last_name", "lastName", "surname");
                                            if (userId == 0 && candidate["UserID"] != null) TryParseInt(candidate["UserID"], ref userId);
                                            email = string.IsNullOrWhiteSpace(email) ? TryGetString(candidate, "Email", "email") : email;
                                            // break only if we've got both name and email
                                            if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(email)) break;
                                        }
                                    }
                                }
                            }
                        }

                        // If still null, leave as null (SiteMaster will not show email)
                        // Store sessions strongly
                        var authObj = new
                        {
                            UserID = userId,
                            FirstName = string.IsNullOrWhiteSpace(firstName) ? null : firstName.Trim(),
                            LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim(),
                            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim()
                        };

                        Session["AuthUser"] = authObj;
                        Session["UserEmail"] = authObj.Email;
                        if (authObj.UserID > 0) Session["UserID"] = authObj.UserID;

                        Session["SuccessBanner"] = "Welcome " + (authObj.FirstName ?? "User") + "! You have logged in successfully.";

                        Response.Redirect("~/Register%20Page/UsersList.aspx", false);
                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }

                    // ----------------------------------------------
                    // FAILED LOGIN (401/400 etc)
                    // ----------------------------------------------
                    string friendly = null;
                    try
                    {
                        var err = string.IsNullOrWhiteSpace(body) ? null : JObject.Parse(body);
                        if (err != null)
                        {
                            if (err["message"] != null) friendly = err["message"].ToString();
                            if (err["Message"] != null) friendly = err["Message"].ToString();
                            if (err["error"] != null) friendly = err["error"].ToString();
                        }
                    }
                    catch { }

                    lblLoginMsg.Text = "Login failed: " + (int)res.StatusCode + " " + res.StatusCode +
                                       (friendly != null ? " — " + HttpUtility.HtmlEncode(friendly)
                                                        : "<br/>" + HttpUtility.HtmlEncode(body));
                }
            }
            catch (Exception ex)
            {
                lblLoginMsg.Text = "Error: " + ex.Message;
            }
        }

        protected void btnForgotPassword_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Register%20Page/ForgotPassword");
        }

        private async Task<string> GenerateAndGetTokenFromDbAsync(string email)
        {
            string conn = ConfigurationManager.ConnectionStrings["MyConn"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(conn))
                throw new ConfigurationErrorsException("Connection string 'MyConn' missing.");

            using (var con = new SqlConnection(conn))
            using (var cmd = new SqlCommand("sp_UsersRegister_ResendVerification", con))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", email);

                var outParam = new SqlParameter("@OutToken", System.Data.SqlDbType.NVarChar, 200)
                { Direction = System.Data.ParameterDirection.Output };
                cmd.Parameters.Add(outParam);

                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return outParam.Value as string;
            }
        }

        private async Task SendVerificationEmailAsync(string toEmail, string firstName, string token)
        {
            string emailSender = ConfigurationManager.AppSettings["EmailSender"];
            string emailSenderPassword = ConfigurationManager.AppSettings["EmailSenderPassword"];
            string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            int smtpPort = int.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out int p) ? p : 587;
            bool enableSsl = true;
            bool.TryParse(ConfigurationManager.AppSettings["EnableSsl"], out enableSsl);

            string host = Request.Url.GetLeftPart(UriPartial.Authority);
            string verifyUrl = host + "/Verify.aspx?token=" + HttpUtility.UrlEncode(token);

            var mail = new MailMessage();
            mail.From = new MailAddress(emailSender ?? "no-reply@example.com", "User Management");
            mail.To.Add(toEmail);
            mail.Subject = "Email Verification";
            mail.IsBodyHtml = true;

            mail.Body = $@"
            <h2>Hello {HttpUtility.HtmlEncode(firstName)},</h2>
            <p>Please verify your email by clicking below:</p>
            <a href='{HttpUtility.HtmlEncode(verifyUrl)}'>Verify Email</a>";

            using (var smtp = new SmtpClient(smtpHost, smtpPort))
            {
                if (!string.IsNullOrWhiteSpace(emailSender) && emailSenderPassword != null)
                    smtp.Credentials = new NetworkCredential(emailSender, emailSenderPassword);
                smtp.EnableSsl = enableSsl;
                await smtp.SendMailAsync(mail);
            }
        }

        // ---------- Helper parsing utilities ----------
        private static bool? GetBoolFromJObject(JObject jo, params string[] keys)
        {
            foreach (var k in keys)
            {
                if (jo[k] != null)
                {
                    if (jo[k].Type == JTokenType.Boolean) return jo[k].ToObject<bool>();
                    var s = jo[k].ToString().Trim().ToLower();
                    if (s == "true" || s == "1") return true;
                    if (s == "false" || s == "0") return false;
                }
            }
            return null;
        }

        private static string TryGetString(JObject jo, params string[] keys)
        {
            if (jo == null) return null;
            foreach (var k in keys)
            {
                if (jo[k] != null && jo[k].Type != JTokenType.Null)
                {
                    var v = jo[k].ToString();
                    if (!string.IsNullOrWhiteSpace(v)) return v;
                }
            }
            return null;
        }

        private static (string firstName, string lastName) TryGetNameFromJObject(JObject jo)
        {
            if (jo == null) return (null, null);
            string fn = TryGetString(jo, "FirstName", "first_name", "firstName", "name", "username", "fullName");
            string ln = TryGetString(jo, "LastName", "last_name", "lastName", "surname", "familyName");
            if (string.IsNullOrWhiteSpace(fn) && jo["user"] is JObject u) fn = TryGetString(u, "FirstName", "firstName", "name");
            if (string.IsNullOrWhiteSpace(ln) && jo["user"] is JObject u2) ln = TryGetString(u2, "LastName", "lastName");
            return (fn, ln);
        }

        private static void TryParseInt(JToken token, ref int target)
        {
            try
            {
                if (token == null) return;
                if (token.Type == JTokenType.Integer) target = token.ToObject<int>();
                else
                {
                    int.TryParse(token.ToString(), out int t); if (t > 0) target = t;
                }
            }
            catch { }
        }
    }
}
