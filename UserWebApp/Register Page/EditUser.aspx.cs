using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Web;

namespace UserWebApp.RegisterPage
{
    public partial class EditUser : System.Web.UI.Page
    {
        private static readonly HttpClient http = new HttpClient();

        private string BaseUrl
        {
            get
            {
                var cfg = (ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "").TrimEnd('/');
                if (!string.IsNullOrEmpty(cfg)) return cfg;
                // fallback to current request host (so relative API calls still work)
                if (Request?.Url != null)
                    return Request.Url.GetLeftPart(UriPartial.Authority).TrimEnd('/');
                return "";
            }
        }

        private void DisablePageCaching()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
        }

        private bool IsLoggedIn =>
            Session["UserEmail"] != null || Session["AuthUser"] != null;

        protected async void Page_Load(object sender, EventArgs e)
        {
            DisablePageCaching();

            if (!IsLoggedIn)
            {
                Response.Redirect("~/Register%20Page/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                // ensure HttpClient.BaseAddress is set if not already
                if (http.BaseAddress == null && !string.IsNullOrWhiteSpace(BaseUrl))
                {
                    try
                    {
                        http.BaseAddress = new Uri(BaseUrl);
                    }
                    catch
                    {
                        // ignore, we'll use absolute URLs below
                    }
                }

                var id = Request.QueryString["id"];
                if (string.IsNullOrWhiteSpace(id))
                {
                    lblMsg.Text = "Missing id.";
                    return;
                }
                hdnId.Value = id;

                await LoadUserAsync(id);
            }
        }

        // DTO: make fields nullable to avoid deserialization issues when DB returns null
        class UserDto
        {
            public int UserID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string PasswordHash { get; set; }
            public string Gender { get; set; }

            // support both names to match different server shapes:
            public string Dob { get; set; }      // server might send "Dob" as string
            public string DOB { get; set; }      // or "DOB" as string
            public DateTime? DOBDate { get; set; } // or server might send DateTime

            public string Country { get; set; }
            public string Address { get; set; }
            public string ProfileImagePath { get; set; }
            public bool IsTermsAccepted { get; set; }
            public DateTime? CreatedAt { get; set; }
        }

        private async System.Threading.Tasks.Task LoadUserAsync(string id)
        {
            try
            {
                string url = $"{BaseUrl}/api/usersregisters/{id}";
                // Use absolute URL string to avoid issues when BaseAddress not set
                var res = await http.GetAsync(url);
                var body = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    lblMsg.Text = $"Load failed: {(int)res.StatusCode} {res.StatusCode} — {body}";
                    return;
                }

                var u = JsonConvert.DeserializeObject<UserDto>(body);
                if (u == null)
                {
                    lblMsg.Text = "Load failed: response deserialized to null.";
                    return;
                }

                txtFirst.Text = u.FirstName ?? "";
                txtLast.Text = u.LastName ?? "";
                txtEmail.Text = u.Email ?? "";
                txtCountry.Text = u.Country ?? "";
                txtAddress.Text = u.Address ?? "";

                // safe gender selection
                var genderVal = u.Gender ?? "";
                if (!string.IsNullOrWhiteSpace(genderVal))
                {
                    try
                    {
                        rblGender.SelectedValue = genderVal;
                    }
                    catch
                    {
                        // if value not present in list, ignore
                        rblGender.ClearSelection();
                    }
                }
                else
                {
                    rblGender.ClearSelection();
                }

                // DOB: try many shapes: DOBDate, DOB string, Dob string
                DateTime? dtNullable = null;
                if (u.DOBDate.HasValue)
                {
                    dtNullable = u.DOBDate.Value;
                }
                else if (!string.IsNullOrWhiteSpace(u.DOB))
                {
                    if (DateTime.TryParse(u.DOB, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt1))
                        dtNullable = dt1;
                }
                else if (!string.IsNullOrWhiteSpace(u.Dob))
                {
                    if (DateTime.TryParse(u.Dob, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt2))
                        dtNullable = dt2;
                }

                if (dtNullable.HasValue)
                    txtDob.Text = dtNullable.Value.ToString("dd/MM/yyyy");
                else
                    txtDob.Text = "";

                hdnPwdHash.Value = u.PasswordHash ?? "";
                hdnProfile.Value = u.ProfileImagePath ?? "";
                hdnTerms.Value = u.IsTermsAccepted.ToString();
                hdnCreatedAt.Value = u.CreatedAt.HasValue ? u.CreatedAt.Value.ToString("o") : "";
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Error while loading: " + ex.Message;
            }
        }

        protected async void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hdnId.Value))
                {
                    lblMsg.Text = "Missing id.";
                    return;
                }

                string dobIso = null;
                if (!string.IsNullOrWhiteSpace(txtDob.Text))
                {
                    DateTime dob;
                    // Accept dd/MM/yyyy from UI, or parse other formats
                    if (DateTime.TryParseExact(txtDob.Text.Trim(), "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out dob)
                        || DateTime.TryParse(txtDob.Text.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dob))
                    {
                        dobIso = dob.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        lblMsg.Text = "DOB format must be DD/MM/YYYY (or a valid date).";
                        return;
                    }
                }

                bool isTerms = bool.TryParse(hdnTerms.Value, out var t) ? t : false;
                DateTime? createdAt = null;
                if (!string.IsNullOrWhiteSpace(hdnCreatedAt.Value) &&
                    DateTime.TryParse(hdnCreatedAt.Value, out var ca))
                {
                    createdAt = ca;
                }

                // safe gender read
                var gender = "";
                try { gender = rblGender.SelectedValue ?? ""; } catch { gender = ""; }

                var payload = new
                {
                    UserID = int.Parse(hdnId.Value),
                    FirstName = txtFirst.Text?.Trim(),
                    LastName = txtLast.Text?.Trim(),
                    Email = txtEmail.Text?.Trim(),
                    PasswordHash = hdnPwdHash.Value,
                    Gender = string.IsNullOrWhiteSpace(gender) ? null : gender,
                    Dob = dobIso,           // match server UpdateUserDto.Dob (string)
                    Country = txtCountry.Text?.Trim(),
                    Address = txtAddress.Text?.Trim(),
                    ProfileImagePath = hdnProfile.Value,
                    IsTermsAccepted = isTerms,
                    CreatedAt = createdAt
                };

                var json = JsonConvert.SerializeObject(payload);
                string url = $"{BaseUrl}/api/usersregisters/{hdnId.Value}";
                var res = await http.PutAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
                var body = await res.Content.ReadAsStringAsync();

                if (res.IsSuccessStatusCode)
                {
                    lblMsg.ForeColor = System.Drawing.Color.Green;
                    lblMsg.Text = "Saved successfully.";
                }
                else
                {
                    lblMsg.ForeColor = System.Drawing.Color.Red;
                    // show full body to help debugging
                    lblMsg.Text = $"Save failed: {(int)res.StatusCode} {res.StatusCode} — {body}";
                }
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Error while saving: " + ex.Message;
            }
        }
    }
}

