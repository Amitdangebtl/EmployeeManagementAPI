using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Globalization;
using System.Net.Http;
using System.Web;

namespace UserWebApp.RegisterPage
{
    public partial class ViewUser : System.Web.UI.Page
    {
        private static readonly HttpClient http = new HttpClient();

        private string BaseUrl => (ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "").TrimEnd('/');

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
                Response.Redirect("~/Register Page/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }


            if (!IsPostBack)
            {
                if (http.BaseAddress == null && !string.IsNullOrWhiteSpace(BaseUrl))
                    http.BaseAddress = new Uri(BaseUrl);

                var id = Request.QueryString["id"];
                if (string.IsNullOrWhiteSpace(id))
                {
                    lblMsg.Text = "Missing id.";
                    return;
                }

                lnkEdit.NavigateUrl = $"~/Register%20Page/EditUser.aspx?id={id}";

                await LoadUserAsync(id);
            }
        }

        class UserDto
        {
            public int UserID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string PasswordHash { get; set; }
            public string Gender { get; set; }
            public DateTime? DOB { get; set; }
            public string Dob { get; set; }
            public string Country { get; set; }
            public string State { get; set; }
            public string City { get; set; }
            public string Address { get; set; }
            public string ProfileImagePath { get; set; }
            public bool IsTermsAccepted { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? CreatedOn { get; set; }
            public DateTime? CreatedDate { get; set; }
        }

        private async System.Threading.Tasks.Task LoadUserAsync(string id)
        {
            try
            {
                var res = await http.GetAsync($"{BaseUrl}/api/usersregisters/{id}");
                var body = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    lblMsg.Text = $"Load failed: {(int)res.StatusCode} {res.StatusCode} — {body}";
                    return;
                }

                var u = JsonConvert.DeserializeObject<UserDto>(body);
                if (u == null) { lblMsg.Text = "No data."; return; }

                lblId.Text = u.UserID.ToString();
                lblFirst.Text = u.FirstName;
                lblLast.Text = u.LastName;
                lblEmail.Text = u.Email;
                lblGender.Text = u.Gender;

                DateTime? dobVal = u.DOB;
                if (!dobVal.HasValue && !string.IsNullOrWhiteSpace(u.Dob))
                {
                    if (DateTime.TryParse(u.Dob, out var dobParsed))
                        dobVal = dobParsed;
                }
                lblDob.Text = dobVal.HasValue ? dobVal.Value.ToString("dd/MM/yyyy") : string.Empty;

                lblCountry.Text = u.Country;
                lblState.Text = u.State;
                lblCity.Text = u.City;
                lblAddress.Text = u.Address;

                lblTerms.Text = u.IsTermsAccepted ? "Yes" : "No";

                string img = u.ProfileImagePath;
                string resolvedUrl = null;

                if (!string.IsNullOrWhiteSpace(img))
                {
                    if (img.StartsWith("~/")) resolvedUrl = ResolveUrl(img);
                    else if (img.StartsWith("http", StringComparison.OrdinalIgnoreCase)) resolvedUrl = img;
                    else resolvedUrl = ResolveUrl("~/Uploads/" + img);
                }

                if (string.IsNullOrWhiteSpace(resolvedUrl))
                    resolvedUrl = "https://via.placeholder.com/300x300?text=No+Image";

                string cacheBusted = resolvedUrl + (resolvedUrl.Contains("?") ? "&" : "?") + "v=" + DateTime.UtcNow.Ticks;
                imgProfile.ImageUrl = cacheBusted;
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Error: " + ex.Message;
            }
        }
    }
}