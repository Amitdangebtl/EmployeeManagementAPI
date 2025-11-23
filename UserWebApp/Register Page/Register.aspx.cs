using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;

namespace UserWebApp.RegisterPage
{
    public partial class Register : System.Web.UI.Page
    {
        protected async void Page_Load(object sender, EventArgs e)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            if (!IsPostBack)
            {
                // Ensure dropdowns enabled
                ddlCountry.Enabled = true;
                DropDownState.Enabled = true;
                DropDownCity.Enabled = true;

                if (ddlCountry.Items.Count == 0) ddlCountry.Items.Add(new ListItem("Select Country", "0"));
                if (DropDownState.Items.Count == 0) DropDownState.Items.Add(new ListItem("Select State", "0"));
                if (DropDownCity.Items.Count == 0) DropDownCity.Items.Add(new ListItem("Select City", "0"));

                lblResult.ForeColor = System.Drawing.Color.Black;
                lblResult.Text = "Loading locations...";

                // Load locations from DB and await so list is ready before render
                await LoadLocationFromUsersDbAsync();

                lblResult.Text = "";

                var max18 = DateTime.Today.AddYears(-18);
                txtDob.Attributes["max"] = max18.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            if (IsPostBack)
            {
                try
                {
                    if (!string.IsNullOrEmpty(txtPassword.Text))
                        txtPassword.Attributes["value"] = txtPassword.Text;

                    if (!string.IsNullOrEmpty(txtConfirm.Text))
                        txtConfirm.Attributes["value"] = txtConfirm.Text;
                }
                catch { }
            }
        }

        // ----------------- Load locations from DB -----------------
        private async Task LoadLocationFromUsersDbAsync()
        {
            try
            {
                string conn = ConfigurationManager.ConnectionStrings["MyConn"]?.ConnectionString;
                if (string.IsNullOrWhiteSpace(conn))
                {
                    lblResult.ForeColor = System.Drawing.Color.Red;
                    lblResult.Text = "Database connection not configured. Please check web.config.";
                    return;
                }

                var countries = new List<string>();

                using (var con = new SqlConnection(conn))
                using (var cmd = new SqlCommand(@"
                    SELECT DISTINCT LTRIM(RTRIM(ISNULL(Country,''))) AS Country
                    FROM dbo.Users_Register
                    WHERE ISNULL(Country,'') <> ''
                    ORDER BY LTRIM(RTRIM(ISNULL(Country,'')))
                ", con))
                {
                    await con.OpenAsync();
                    using (var rdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await rdr.ReadAsync())
                        {
                            countries.Add(rdr.IsDBNull(0) ? "" : rdr.GetString(0));
                        }
                    }
                }

                ddlCountry.Items.Clear();
                ddlCountry.Items.Add(new ListItem("Select Country", "0"));
                foreach (var c in countries) ddlCountry.Items.Add(new ListItem(c, c));

                DropDownState.Items.Clear();
                DropDownState.Items.Add(new ListItem("Select State", "0"));

                DropDownCity.Items.Clear();
                DropDownCity.Items.Add(new ListItem("Select City", "0"));
            }
            catch (Exception ex)
            {
                lblResult.ForeColor = System.Drawing.Color.Red;
                lblResult.Text = "Error loading locations: " + ex.Message;
            }
        }

        protected async void ddlCountry_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownState.Items.Clear();
            DropDownState.Items.Add(new ListItem("Select State", "0"));

            DropDownCity.Items.Clear();
            DropDownCity.Items.Add(new ListItem("Select City", "0"));

            var selectedCountry = ddlCountry.SelectedValue?.Trim();
            if (string.IsNullOrEmpty(selectedCountry) || selectedCountry == "0") return;

            try
            {
                string conn = ConfigurationManager.ConnectionStrings["MyConn"]?.ConnectionString;
                if (string.IsNullOrWhiteSpace(conn)) return;

                var states = new List<string>();
                using (var con = new SqlConnection(conn))
                using (var cmd = new SqlCommand(@"
                    SELECT DISTINCT LTRIM(RTRIM(ISNULL(State,''))) AS State
                    FROM dbo.Users_Register
                    WHERE ISNULL(State,'') <> '' AND LTRIM(RTRIM(ISNULL(Country,''))) = @Country
                    ORDER BY LTRIM(RTRIM(ISNULL(State,'')))
                ", con))
                {
                    cmd.Parameters.AddWithValue("@Country", selectedCountry);
                    await con.OpenAsync();
                    using (var rdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await rdr.ReadAsync())
                        {
                            states.Add(rdr.IsDBNull(0) ? "" : rdr.GetString(0));
                        }
                    }
                }

                foreach (var s in states) DropDownState.Items.Add(new ListItem(s, s));
            }
            catch (Exception ex)
            {
                lblResult.ForeColor = System.Drawing.Color.Red;
                lblResult.Text = "Error loading states: " + ex.Message;
            }
        }

        protected async void DropDownState_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownCity.Items.Clear();
            DropDownCity.Items.Add(new ListItem("Select City", "0"));

            var selectedCountry = ddlCountry.SelectedValue?.Trim();
            var selectedState = DropDownState.SelectedValue?.Trim();

            if (string.IsNullOrEmpty(selectedState) || selectedState == "0") return;

            try
            {
                string conn = ConfigurationManager.ConnectionStrings["MyConn"]?.ConnectionString;
                if (string.IsNullOrWhiteSpace(conn)) return;

                var cities = new List<string>();
                using (var con = new SqlConnection(conn))
                using (var cmd = new SqlCommand(@"
                    SELECT DISTINCT LTRIM(RTRIM(ISNULL(City,''))) AS City
                    FROM dbo.Users_Register
                    WHERE ISNULL(City,'') <> ''
                      AND LTRIM(RTRIM(ISNULL(State,''))) = @State
                      AND (@Country = '0' OR LTRIM(RTRIM(ISNULL(Country,''))) = @Country)
                    ORDER BY LTRIM(RTRIM(ISNULL(City,'')))
                ", con))
                {
                    cmd.Parameters.AddWithValue("@State", selectedState);
                    cmd.Parameters.AddWithValue("@Country", string.IsNullOrEmpty(selectedCountry) ? "0" : selectedCountry);

                    await con.OpenAsync();
                    using (var rdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await rdr.ReadAsync())
                        {
                            cities.Add(rdr.IsDBNull(0) ? "" : rdr.GetString(0));
                        }
                    }
                }

                foreach (var c in cities) DropDownCity.Items.Add(new ListItem(c, c));
            }
            catch (Exception ex)
            {
                lblResult.ForeColor = System.Drawing.Color.Red;
                lblResult.Text = "Error loading cities: " + ex.Message;
            }
        }

        // AGE VALIDATION
        protected void cvDob18_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = false;
            if (string.IsNullOrWhiteSpace(txtDob.Text)) return;
            if (!DateTime.TryParse(txtDob.Text, out var dob)) return;

            args.IsValid = dob <= DateTime.Today.AddYears(-18);
        }

        protected void cvTerms_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = chkTerms.Checked;
        }

        // ---------------- DB SAVE HELPER ----------------
        private async Task<(int userId, string token)> SaveUserToDbAsync(
            string firstName, string lastName, string email, string passwordHash,
            string gender, DateTime? dob, string country, string state, string city,
            string address, string profileImagePath, bool isTermsAccepted)
        {
            string conn = ConfigurationManager.ConnectionStrings["MyConn"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(conn))
                throw new ConfigurationErrorsException("Connection string 'MyConn' missing in Web.config.");

            using (var con = new SqlConnection(conn))
            using (var cmd = new SqlCommand("sp_UsersRegister_Insert", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@FirstName", (object)firstName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LastName", (object)lastName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PasswordHash", (object)passwordHash ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Gender", (object)gender ?? DBNull.Value);

                if (dob.HasValue)
                    cmd.Parameters.AddWithValue("@Dob", dob.Value.Date);
                else
                    cmd.Parameters.AddWithValue("@Dob", DBNull.Value);

                cmd.Parameters.AddWithValue("@Country", (object)country ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@State", (object)state ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@City", (object)city ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProfileImagePath", (object)profileImagePath ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsTermsAccepted", isTermsAccepted);

                // Let SP generate token (pass NULL)
                cmd.Parameters.AddWithValue("@VerificationToken", DBNull.Value);
                cmd.Parameters.AddWithValue("@TokenExpiry", DBNull.Value);

                var outId = new SqlParameter("@NewUserId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var outToken = new SqlParameter("@OutToken", SqlDbType.NVarChar, 200) { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(outId);
                cmd.Parameters.Add(outToken);

                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                int newId = outId.Value != DBNull.Value ? Convert.ToInt32(outId.Value) : 0;
                string token = outToken.Value as string;

                return (newId, token);
            }
        }

        // REGISTER BUTTON CLICK
        protected async void btnRegister_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                ShowError("Please fix form validation errors.");
                return;
            }

            if (txtPassword.Text != txtConfirm.Text)
            {
                ShowError("Passwords do not match.");
                return;
            }

            string state = DropDownState.SelectedItem?.Text.Trim();
            string city = DropDownCity.SelectedItem?.Text.Trim();
            string country = ddlCountry.SelectedItem?.Text.Trim();

            if (string.IsNullOrWhiteSpace(country) || ddlCountry.SelectedValue == "0")
            {
                ShowError("Please select a Country.");
                return;
            }

            if (string.IsNullOrWhiteSpace(state) || DropDownState.SelectedValue == "0")
            {
                ShowError("Please select a State.");
                return;
            }

            if (string.IsNullOrWhiteSpace(city) || DropDownCity.SelectedValue == "0")
            {
                ShowError("Please select a City.");
                return;
            }

            try
            {
                DateTime? dob = null;
                if (!string.IsNullOrWhiteSpace(txtDob.Text))
                {
                    if (DateTime.TryParse(txtDob.Text, out var d))
                        dob = d.Date;
                    else
                    {
                        ShowError("Invalid date.");
                        return;
                    }
                }

                string profileImgPath = null;

                if (fuProfile != null && fuProfile.HasFile)
                {
                    string ext = Path.GetExtension(fuProfile.FileName)?.ToLowerInvariant();
                    string[] allowed = { ".jpg", ".jpeg", ".png", ".gif" };

                    if (!allowed.Contains(ext))
                    {
                        ShowError("Only JPG/PNG/GIF images allowed.");
                        return;
                    }

                    string fileName = Guid.NewGuid().ToString("N") + ext;
                    string relative = "~/Uploads/" + fileName;
                    string physical = Server.MapPath(relative);

                    Directory.CreateDirectory(Path.GetDirectoryName(physical));
                    fuProfile.SaveAs(physical);

                    profileImgPath = relative;
                }

                string passwordHash = ComputeSha256(txtPassword.Text);

                // Save to DB via stored-proc and get token back
                var (userId, token) = await SaveUserToDbAsync(
                    txtFirst.Text.Trim(),
                    txtLast.Text.Trim(),
                    txtEmail.Text.Trim(),
                    passwordHash,
                    rblGender.SelectedValue,
                    dob,
                    country,
                    state,
                    city,
                    txtAddress.Text.Trim(),
                    profileImgPath,
                    chkTerms.Checked
                );

                if (userId <= 0)
                {
                    ShowError("Registration failed. Please try again.");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(token))
                {
                    try
                    {
                        await SendWelcomeEmailAsync(txtEmail.Text.Trim(), txtFirst.Text.Trim(), token);
                        ShowSuccess("Registration successful! Please check your email to verify your account.");
                    }
                    catch (Exception ex)
                    {
                        ShowSuccess("Registered, but email sending failed: " + ex.Message);
                    }
                }
                else
                {
                    ShowSuccess("Registered. Please contact support to verify your email.");
                }

                ClearFormExceptMessage();
            }
            catch (Exception ex)
            {
                ShowError("Error: " + ex.Message);
            }
        }

        // RESET BUTTON CLICK
        protected void btnReset_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        // ----------------- Helper Methods -----------------

        private void ShowError(string msg)
        {
            lblResult.ForeColor = System.Drawing.Color.Red;
            lblResult.Text = msg;
        }

        private void ShowSuccess(string msg)
        {
            lblResult.ForeColor = System.Drawing.Color.Green;
            lblResult.Text = msg;
        }

        private static string ComputeSha256(string input)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? ""));
                return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }
        }

        private void ClearForm()
        {
            txtFirst.Text = "";
            txtLast.Text = "";
            txtEmail.Text = "";
            txtPassword.Text = "";
            txtConfirm.Text = "";
            rblGender.ClearSelection();
            txtDob.Text = "";
            ddlCountry.SelectedIndex = 0;
            DropDownState.SelectedIndex = 0;
            DropDownCity.SelectedIndex = 0;
            txtAddress.Text = "";
            chkTerms.Checked = false;
            lblResult.Text = "";
        }

        private void ClearFormExceptMessage()
        {
            txtFirst.Text = "";
            txtLast.Text = "";
            txtEmail.Text = "";
            txtPassword.Text = "";
            txtConfirm.Text = "";
            rblGender.ClearSelection();
            txtDob.Text = "";
            ddlCountry.SelectedIndex = 0;
            DropDownState.SelectedIndex = 0;
            DropDownCity.SelectedIndex = 0;
            txtAddress.Text = "";
            chkTerms.Checked = false;
        }

        // EMAIL SENDING WITH CORRECT VERIFY URL
        private async Task SendWelcomeEmailAsync(string toEmail, string firstName, string token)
        {
            string emailSender = ConfigurationManager.AppSettings["EmailSender"];
            string emailSenderPassword = ConfigurationManager.AppSettings["EmailSenderPassword"];
            string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            int smtpPort = 587;
            bool enableSsl = true;

            int.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out smtpPort);
            bool.TryParse(ConfigurationManager.AppSettings["EnableSsl"], out enableSsl);

            string host = Request?.Url?.GetLeftPart(UriPartial.Authority)
                          ?? ConfigurationManager.AppSettings["SiteRoot"] ?? "https://localhost:44330";

            // Build app-relative path safely.
            string verifyPath;
            try
            {
                verifyPath = ResolveUrl("~/Register Page/Verify.aspx");
            }
            catch
            {
                verifyPath = VirtualPathUtility.ToAbsolute("~/Register Page/Verify.aspx");
            }

            string verifyUrl = host.TrimEnd('/') + verifyPath + "?token=" + HttpUtility.UrlEncode(token);

            var mail = new MailMessage();
            mail.From = new MailAddress(emailSender, "User Management");
            mail.To.Add(toEmail);
            mail.Subject = "Please verify your email";
            mail.IsBodyHtml = true;

            mail.Body = $@"
<html>
<body>
    <h2>Welcome, {HttpUtility.HtmlEncode(firstName)}</h2>
    <p>Click below to verify your email:</p>
    <a href='{verifyUrl}' style='padding:10px 15px;background:#28a745;color:white;text-decoration:none;border-radius:5px;'>Verify Email</a>
</body>
</html>";

            using (var smtp = new SmtpClient(smtpHost, smtpPort))
            {
                smtp.Credentials = new NetworkCredential(emailSender, emailSenderPassword);
                smtp.EnableSsl = enableSsl;

                await smtp.SendMailAsync(mail);
            }
        }
    }
}

