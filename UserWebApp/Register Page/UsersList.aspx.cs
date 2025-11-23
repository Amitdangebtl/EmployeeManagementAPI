using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ExcelDataReader;
using System.Globalization;

namespace UserWebApp.Users
{
    public partial class UsersList : Page
    {
        private static readonly HttpClient http = new HttpClient();
        private string BaseUrl => (ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "").TrimEnd('/');

        // Auth helpers
        private bool IsLoggedIn => Session["UserEmail"] != null || Session["AuthUser"] != null;
        private bool IsAdmin => (Session["RoleName"] as string)?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;
        private int CurrentUserId => Session["UserID"] is int id ? id : 0;

        // Session cached data
        private List<UserDto> Data { get => Session["UsersData"] as List<UserDto>; set => Session["UsersData"] = value; }
        private List<UserDto> Filtered { get => Session["UsersDataFiltered"] as List<UserDto>; set => Session["UsersDataFiltered"] = value; }

        private string SortExpr { get => (ViewState["SortExpr"] as string) ?? "UserID"; set => ViewState["SortExpr"] = value; }
        private SortDirection SortDir { get => (ViewState["SortDir"] is SortDirection d) ? d : SortDirection.Ascending; set => ViewState["SortDir"] = value; }

        protected void Page_Load(object sender, EventArgs e)
        {
            // If request contains download query -> stream file and exit early
            if (!string.IsNullOrEmpty(Request.QueryString["download"]))
            {
                ServeAppDataFile(Request.QueryString["download"]);
                return;
            }

            if (!IsLoggedIn)
            {
                Response.Redirect("~/Register%20Page/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack) BindGrid(true);
        }

        // Bind grid from API
        private async void BindGrid(bool refreshFromApi)
        {
            try
            {
                if (refreshFromApi || Data == null)
                {
                    var res = await http.GetAsync(BaseUrl + "/api/usersregisters");
                    var json = await res.Content.ReadAsStringAsync();
                    Data = JsonConvert.DeserializeObject<List<UserDto>>(json) ?? new List<UserDto>();
                    Filtered = null;
                }

                var rows = (Filtered ?? Data) ?? new List<UserDto>();
                gvUsers.DataSource = Sort(rows, SortExpr, SortDir);
                gvUsers.DataBind();
                lblMsg.Text = "";
            }
            catch (Exception ex) { lblMsg.Text = "Error: " + ex.Message; }
        }

        // Search / Clear
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            ApplySearch();
            gvUsers.PageIndex = 0;
            BindGrid(false);
        }
        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            Filtered = null;
            gvUsers.PageIndex = 0;
            BindGrid(false);
        }

        private void ApplySearch()
        {
            var term = (txtSearch.Text ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(term)) { Filtered = null; return; }

            Func<string, string> norm = s => (s ?? "").Trim().ToLowerInvariant();
            Func<UserDto, string> full = u => (norm(u.FirstName) + " " + norm(u.LastName)).Trim();

            var src = Data ?? new List<UserDto>();
            Filtered = src.Where(u =>
                full(u).Contains(term) ||
                norm(u.Email).Contains(term) ||
                norm(u.Gender).Contains(term) ||
                norm(u.Country).Contains(term)
            ).ToList();
        }

        // Paging / Sorting / RowCommand / RowDataBound / RowCreated
        protected void gvUsers_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvUsers.PageIndex = e.NewPageIndex; BindGrid(false); }
        protected void gvUsers_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (string.Equals(SortExpr, e.SortExpression, StringComparison.OrdinalIgnoreCase))
                SortDir = (SortDir == SortDirection.Ascending) ? SortDirection.Descending : SortDirection.Ascending;
            else { SortExpr = e.SortExpression; SortDir = SortDirection.Ascending; }
            gvUsers.PageIndex = 0; BindGrid(false);
        }
        protected async void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "del")
            {
                try
                {
                    var id = e.CommandArgument.ToString();
                    var url = $"{BaseUrl}/api/usersregisters/{id}";
                    var res = await http.DeleteAsync(url);
                    var body = await res.Content.ReadAsStringAsync();

                    if (res.IsSuccessStatusCode)
                    {
                        if (Data != null) Data.RemoveAll(x => x.UserID.ToString() == id);
                        if (Filtered != null) Filtered.RemoveAll(x => x.UserID.ToString() == id);
                        BindGrid(false);
                        lblMsg.ForeColor = System.Drawing.Color.Green;
                        lblMsg.Text = "Deleted.";
                    }
                    else
                    {
                        lblMsg.ForeColor = System.Drawing.Color.Red;
                        lblMsg.Text = $"Delete failed: {(int)res.StatusCode} {res.StatusCode} — {body}";
                    }
                }
                catch (Exception ex) { lblMsg.Text = "Error: " + ex.Message; }
            }
        }
        protected void gvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            var hlView = (HyperLink)e.Row.FindControl("hlView");
            var hlEdit = (HyperLink)e.Row.FindControl("hlEdit");
            var btnDelete = (LinkButton)e.Row.FindControl("btnDelete");
            if (hlView != null) hlView.Visible = true;
            if (hlEdit != null) hlEdit.Visible = true;
            if (btnDelete != null) btnDelete.Visible = true;
        }
        protected void gvUsers_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                for (int i = 0; i < gvUsers.Columns.Count; i++)
                {
                    var c = gvUsers.Columns[i];
                    string expr = null;
                    if (c is BoundField b) expr = b.SortExpression;
                    else if (c is TemplateField t) expr = t.SortExpression;
                    if (string.IsNullOrEmpty(expr)) continue;
                    if (!string.Equals(expr, SortExpr, StringComparison.OrdinalIgnoreCase)) continue;

                    TableCell headerCell = e.Row.Cells[i];
                    string arrow = (SortDir == SortDirection.Ascending) ? " ▲" : " ▼";
                    if (headerCell.Controls.Count > 0)
                    {
                        var lb = headerCell.Controls.OfType<LinkButton>().FirstOrDefault();
                        if (lb != null) { lb.Text = lb.Text + arrow; continue; }
                    }
                    headerCell.Text = headerCell.Text + arrow;
                }
            }
        }

        private IEnumerable<UserDto> Sort(List<UserDto> list, string sortBy, SortDirection dir)
        {
            if (list == null || list.Count == 0) return list;
            if (string.Equals(sortBy, "FullName", StringComparison.OrdinalIgnoreCase))
            {
                return (dir == SortDirection.Ascending)
                    ? list.OrderBy(x => ((x.FirstName ?? "") + " " + (x.LastName ?? "")).Trim().ToLowerInvariant()).ToList()
                    : list.OrderByDescending(x => ((x.FirstName ?? "") + " " + (x.LastName ?? "")).Trim().ToLowerInvariant()).ToList();
            }

            var prop = typeof(UserDto).GetProperty(sortBy, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null) return (dir == SortDirection.Ascending) ? list.OrderBy(x => x.UserID).ToList() : list.OrderByDescending(x => x.UserID).ToList();

            if (dir == SortDirection.Ascending) return list.OrderBy(x => { var val = prop.GetValue(x, null); return val == null ? "" : val; }).ToList();
            else return list.OrderByDescending(x => { var val = prop.GetValue(x, null); return val == null ? "" : val; }).ToList();
        }

        // ========================
        // Excel Upload handler
        // ========================
        protected void btnUploadExcel_Click(object sender, EventArgs e)
        {
            litUploadStatus.Text = "";
            litExcelSummary.Text = "";
            // clear any previous fail alert
            if (litFailAlert != null) litFailAlert.Text = "";
            if (divFailAlert != null) divFailAlert.Style["display"] = "none";

            if (!fuExcel.HasFile)
            {
                litUploadStatus.Text = "<span class='text-danger'>Please select an Excel file (.xls or .xlsx).</span>";
                return;
            }

            string ext = Path.GetExtension(fuExcel.FileName).ToLower();
            if (ext != ".xls" && ext != ".xlsx")
            {
                litUploadStatus.Text = "<span class='text-danger'>Only .xls or .xlsx files are allowed.</span>";
                return;
            }

            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using (var stream = fuExcel.PostedFile.InputStream)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var conf = new ExcelDataSetConfiguration() { ConfigureDataTable = _ => new ExcelDataTableConfiguration() { UseHeaderRow = true } };
                    var ds = reader.AsDataSet(conf);
                    if (ds.Tables.Count == 0) { litUploadStatus.Text = "<span class='text-danger'>Excel sheet not found or empty.</span>"; return; }

                    DataTable sheet = ds.Tables[0];
                    if (sheet.Rows.Count == 0) { litUploadStatus.Text = "<span class='text-danger'>No rows found in Excel sheet.</span>"; return; }

                    // get connection string fallback
                    var csSetting = System.Configuration.ConfigurationManager.ConnectionStrings["MyConn"]
                                ?? System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"]
                                ?? System.Configuration.ConfigurationManager.ConnectionStrings["TestAPIConnection"];

                    if (csSetting == null || string.IsNullOrWhiteSpace(csSetting.ConnectionString))
                    {
                        litUploadStatus.Text = "<span class='text-danger'>Connection string not found (MyConn/DefaultConnection). Please check Web.config.</span>";
                        return;
                    }
                    string connString = csSetting.ConnectionString;

                    var existingEmails = GetExistingEmails();

                    int inserted = 0;
                    var failedRows = new List<string>();
                    var failedCsvRows = new List<string>();
                    string csvHeader = "RowNumber,FirstName,LastName,Email,Password,Gender,DOB,Country,State,City,Address,IsTermsAccepted,RoleId,Reason";

                    using (SqlConnection conn = new SqlConnection(connString))
                    {
                        conn.Open();
                        int defaultUserRoleId = GetRoleIdByName(conn, "User");
                        if (defaultUserRoleId <= 0) defaultUserRoleId = GetAnyRoleId(conn);

                        int rowIndex = 1;
                        foreach (DataRow r in sheet.Rows)
                        {
                            try
                            {
                                string firstName = GetCell(r, "FirstName", "First Name");
                                string lastName = GetCell(r, "LastName", "Last Name");
                                string email = GetCell(r, "Email");
                                string password = GetCell(r, "Password", "Pwd");
                                string gender = GetCell(r, "Gender");
                                object dobObj = null;
                                foreach (DataColumn col in r.Table.Columns)
                                {
                                    if (string.Equals(col.ColumnName.Trim(), "DOB", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(col.ColumnName.Trim(), "DateOfBirth", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(col.ColumnName.Trim(), "Date of Birth", StringComparison.OrdinalIgnoreCase))
                                    {
                                        dobObj = r[col]; break;
                                    }
                                }
                                string country = GetCell(r, "Country");
                                string state = GetCell(r, "State");
                                string city = GetCell(r, "City");
                                string address = GetCell(r, "Address");
                                string terms = GetCell(r, "IsTermsAccepted", "AcceptTerms");

                                int roleId = -1;
                                string roleIdStr = GetCell(r, "RoleId", "Role ID", "Role");
                                if (!string.IsNullOrWhiteSpace(roleIdStr) && int.TryParse(roleIdStr, out int tmp)) roleId = tmp;

                                // --- VALIDATION: mandatory fields ---
                                var rowProblems = new List<string>();
                                if (string.IsNullOrWhiteSpace(firstName)) rowProblems.Add("FirstName missing");
                                if (string.IsNullOrWhiteSpace(email)) rowProblems.Add("Email missing");
                                if (string.IsNullOrWhiteSpace(password)) rowProblems.Add("Password missing");
                                if (string.IsNullOrWhiteSpace(terms)) rowProblems.Add("IsTermsAccepted missing");
                                // If any mandatory missing => fail row
                                if (rowProblems.Count > 0)
                                {
                                    string reason = string.Join("; ", rowProblems);
                                    failedRows.Add($"{rowIndex}: {reason}");
                                    failedCsvRows.Add(CreateCsvRow(rowIndex, firstName, lastName, email, password, gender, "", country, state, city, address, terms, roleIdStr, reason));
                                    rowIndex++; continue;
                                }

                                string emailLower = email.Trim().ToLower();
                                if (existingEmails.Contains(emailLower))
                                {
                                    string reason = "Duplicate email already in DB";
                                    failedRows.Add($"{rowIndex}: {reason} ({emailLower})");
                                    failedCsvRows.Add(CreateCsvRow(rowIndex, firstName, lastName, emailLower, password, gender, "", country, state, city, address, terms, roleIdStr, reason));
                                    rowIndex++; continue;
                                }

                                DateTime? dob = null;
                                if (dobObj != null && dobObj != DBNull.Value)
                                {
                                    if (dobObj is double || dobObj is float || dobObj is decimal)
                                    {
                                        double oa = Convert.ToDouble(dobObj);
                                        try { dob = DateTime.FromOADate(oa); } catch { dob = null; }
                                    }
                                    else
                                    {
                                        string dobStr = dobObj.ToString().Trim();
                                        if (DateTime.TryParse(dobStr, out DateTime tmpdt) ||
                                            DateTime.TryParseExact(dobStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out tmpdt) ||
                                            DateTime.TryParseExact(dobStr, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out tmpdt) ||
                                            DateTime.TryParseExact(dobStr, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out tmpdt))
                                        {
                                            dob = tmpdt;
                                        }
                                    }
                                }

                                bool isTermsAccepted = false;
                                if (!string.IsNullOrEmpty(terms))
                                {
                                    bool.TryParse(terms, out isTermsAccepted);
                                    string tl = terms.Trim().ToLower();
                                    if (!isTermsAccepted && (tl == "yes" || tl == "y" || tl == "1")) isTermsAccepted = true;
                                }
                                if (!isTermsAccepted)
                                {
                                    // if terms value not recognized as accepted, mark row failed
                                    string reason = "IsTermsAccepted not true/Yes";
                                    failedRows.Add($"{rowIndex}: {reason}");
                                    failedCsvRows.Add(CreateCsvRow(rowIndex, firstName, lastName, emailLower, password, gender, "", country, state, city, address, terms, roleIdStr, reason));
                                    rowIndex++; continue;
                                }

                                string passwordHash = null;
                                if (!string.IsNullOrEmpty(password)) passwordHash = ComputeSha256Hash(password.Trim());

                                // Truncate to avoid SQL truncation issues
                                firstName = Truncate(firstName, 100); lastName = Truncate(lastName, 100);
                                emailLower = Truncate(emailLower, 200); gender = Truncate(gender, 20);
                                country = Truncate(country, 100); state = Truncate(state, 100);
                                city = Truncate(city, 100); address = Truncate(address, 500);

                                if (roleId <= 0) roleId = defaultUserRoleId;
                                else if (!RoleExists(conn, roleId)) roleId = defaultUserRoleId;

                                string sql = @"
INSERT INTO dbo.Users_Register
(FirstName, LastName, Email, PasswordHash, Gender, DOB, Country, State, City, Address, IsTermsAccepted, ProfileImagePath, CreatedAt, IsVerified, RoleId)
VALUES
(@FirstName, @LastName, @Email, @PasswordHash, @Gender, @DOB, @Country, @State, @City, @Address, @IsTermsAccepted, @ProfileImagePath, @CreatedAt, @IsVerified, @RoleId)";

                                using (SqlCommand cmd = new SqlCommand(sql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@FirstName", string.IsNullOrEmpty(firstName) ? (object)DBNull.Value : firstName);
                                    cmd.Parameters.AddWithValue("@LastName", string.IsNullOrEmpty(lastName) ? (object)DBNull.Value : lastName);
                                    cmd.Parameters.AddWithValue("@Email", emailLower);
                                    cmd.Parameters.AddWithValue("@PasswordHash", string.IsNullOrEmpty(passwordHash) ? (object)DBNull.Value : passwordHash);
                                    cmd.Parameters.AddWithValue("@Gender", string.IsNullOrEmpty(gender) ? (object)DBNull.Value : gender);
                                    cmd.Parameters.AddWithValue("@DOB", (object)(dob.HasValue ? (object)dob.Value : DBNull.Value));
                                    cmd.Parameters.AddWithValue("@Country", string.IsNullOrEmpty(country) ? (object)DBNull.Value : country);
                                    cmd.Parameters.AddWithValue("@State", string.IsNullOrEmpty(state) ? (object)DBNull.Value : state);
                                    cmd.Parameters.AddWithValue("@City", string.IsNullOrEmpty(city) ? (object)DBNull.Value : city);
                                    cmd.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(address) ? (object)DBNull.Value : address);
                                    cmd.Parameters.AddWithValue("@IsTermsAccepted", isTermsAccepted);
                                    cmd.Parameters.AddWithValue("@ProfileImagePath", DBNull.Value);
                                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@IsVerified", false);
                                    cmd.Parameters.AddWithValue("@RoleId", roleId);

                                    cmd.ExecuteNonQuery();
                                }

                                inserted++;
                                existingEmails.Add(emailLower);
                            }
                            catch (SqlException sqlex)
                            {
                                string reason = sqlex.Message.Replace("\r\n", " ");
                                failedRows.Add($"{rowIndex}: SQL Error - {reason}");
                                failedCsvRows.Add(CreateCsvRow(rowIndex, GetCell(r, "FirstName"), GetCell(r, "LastName"), GetCell(r, "Email"), GetCell(r, "Password"), GetCell(r, "Gender"), GetCell(r, "DOB"), GetCell(r, "Country"), GetCell(r, "State"), GetCell(r, "City"), GetCell(r, "Address"), GetCell(r, "IsTermsAccepted"), GetCell(r, "RoleId"), reason));
                            }
                            catch (Exception exRow)
                            {
                                string reason = exRow.Message.Replace("\r\n", " ");
                                failedRows.Add($"{rowIndex}: Error - {reason}");
                                failedCsvRows.Add(CreateCsvRow(rowIndex, GetCell(r, "FirstName"), GetCell(r, "LastName"), GetCell(r, "Email"), GetCell(r, "Password"), GetCell(r, "Gender"), GetCell(r, "DOB"), GetCell(r, "Country"), GetCell(r, "State"), GetCell(r, "City"), GetCell(r, "Address"), GetCell(r, "IsTermsAccepted"), GetCell(r, "RoleId"), reason));
                            }

                            rowIndex++;
                        } // foreach row
                    } // using conn

                    // Save failed rows CSV if any
                    string csvLink = "";
                    if (failedCsvRows.Count > 0)
                    {
                        string appData = Server.MapPath("~/App_Data");
                        string folder = Path.Combine(appData, "FailedUploads");
                        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                        string fileName = $"FailedRows_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        string fullPath = Path.Combine(folder, fileName);

                        using (var sw = new StreamWriter(fullPath, false, Encoding.UTF8))
                        {
                            sw.WriteLine(csvHeader);
                            foreach (var line in failedCsvRows) sw.WriteLine(line);
                        }

                        // create a server-side download link using query param -> ServeAppDataFile will handle streaming
                        csvLink = ResolveUrl($"~/Register%20Page/UsersList.aspx?download={HttpUtility.UrlEncode(fileName)}");
                    }

                    // Build summary HTML
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"<span class='text-success'>Inserted: {inserted}</span>&nbsp;&nbsp;");
                    if (failedRows.Count > 0)
                    {
                        sb.AppendLine($"<span class='text-danger'>Failed: {failedRows.Count}</span>");
                        sb.AppendLine("<ul>");
                        int show = 0;
                        foreach (var fr in failedRows)
                        {
                            sb.AppendLine($"<li>{Server.HtmlEncode(fr)}</li>");
                            if (++show >= 10) break;
                        }
                        sb.AppendLine("</ul>");
                        if (!string.IsNullOrEmpty(csvLink)) sb.AppendLine($"<div>Download full failed rows CSV: <a href='{csvLink}' target='_blank'>Download</a></div>");
                    }

                    // show in page summary
                    litExcelSummary.Text = sb.ToString();

                    // ---------- MINIMAL ADDED BLOCK (shows popup only if failures) ----------
                    if (failedRows.Count > 0)
                    {
                        // show fail details server-side too (so user can view under summary)
                        if (litFailAlert != null) litFailAlert.Text = sb.ToString();
                        if (divFailAlert != null) divFailAlert.Style["display"] = "block";

                        string safeHtml = HttpUtility.JavaScriptStringEncode(sb.ToString());

                        // Clean UsersList page redirect
                        string redirectUrl = ResolveUrl("~/Register%20Page/UsersList.aspx");

                        string script = $@"
    Swal.fire({{
        title: 'Upload Failed',
        html: `{safeHtml}`,
        icon: 'error',
        showCancelButton: true,
        confirmButtonText: 'OK',
        cancelButtonText: 'Cancel',
        width: '640px'
    }}).then((result) => {{
        if (result.isConfirmed) {{

            // hide fail box on client
            var el = document.getElementById('{divFailAlert.ClientID}');
            if (el) el.style.display = 'none';

            var s = document.getElementById('{litExcelSummary.ClientID}');
            if (s) s.innerHTML = '';

            // redirect to clean UsersList page (no leftover summary)
            window.location.href = '{redirectUrl}';
        }}
    }});";

                        if (ScriptManager.GetCurrent(this.Page) != null)
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "swalFail", script, true);
                        else
                            ClientScript.RegisterStartupScript(this.GetType(), "swalFail", script, true);
                    }
                    // --------------------------------------------------------------------

                    // refresh grid
                    Data = null;
                    BindGrid(true);
                } // using reader
            }
            catch (Exception ex)
            {
                litUploadStatus.Text = $"<span class='text-danger'>Error: {Server.HtmlEncode(ex.Message)}</span>";
            }
        }

        // =========================
        // Download sample button click
        // =========================
        protected void btnDownloadSample_Click(object sender, EventArgs e)
        {
            string appData = Server.MapPath("~/App_Data");
            string samplesFolder = Path.Combine(appData, "Samples");
            string sampleXlsx = Path.Combine(samplesFolder, "SampleUsers.xlsx");

            if (File.Exists(sampleXlsx))
            {
                StreamFileToClient(sampleXlsx, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SampleUsers.xlsx");
                return;
            }

            var csvLines = new List<string>();
            csvLines.Add("FirstName,LastName,Email,Password,Gender,DOB,Country,State,City,Address,IsTermsAccepted,RoleId");
            csvLines.Add("Amit,Dange,amit.unique1@example.com,pass123,Male,1999-01-01,India,Madhya Pradesh,Bhopal,Addr line,Yes,2");
            csvLines.Add("Rohit,Sharma,rohit.unique1@example.com,secret123,Male,1985-05-10,India,Maharashtra,Mumbai,Addr line,Yes,2");

            var list = new List<byte>();
            list.AddRange(new byte[] { 0xEF, 0xBB, 0xBF });
            list.AddRange(Encoding.UTF8.GetBytes(string.Join("\r\n", csvLines)));
            var bytes = list.ToArray();

            Response.ClearHeaders();
            Response.ClearContent();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment; filename=\"SampleUsers.csv\"");
            Response.AddHeader("Content-Length", bytes.Length.ToString());
            Response.BinaryWrite(bytes);
            Response.Flush();

            Response.SuppressContent = true;
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        // Helper to stream server file to client safely
        private void StreamFileToClient(string fullPath, string contentType, string downloadName)
        {
            if (!File.Exists(fullPath))
            {
                Response.ClearHeaders();
                Response.ClearContent();
                Response.StatusCode = 404;
                Response.ContentType = "text/plain";
                Response.Write("File not found");
                HttpContext.Current.ApplicationInstance.CompleteRequest();
                return;
            }

            try
            {
                byte[] fileBytes = File.ReadAllBytes(fullPath);

                Response.ClearHeaders();
                Response.ClearContent();
                Response.ContentType = contentType;
                Response.AddHeader("Content-Disposition", $"attachment; filename=\"{downloadName}\"");
                Response.AddHeader("Content-Length", fileBytes.Length.ToString());
                Response.BinaryWrite(fileBytes);
                Response.Flush();

                Response.SuppressContent = true;
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (HttpException)
            {
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                Response.ClearHeaders();
                Response.ClearContent();
                Response.ContentType = "text/plain";
                Response.Write("Error streaming file: " + ex.Message);
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }

        // Serve files from App_Data/FailedUploads folder using querystring fileName
        private void ServeAppDataFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Response.ClearHeaders();
                Response.ClearContent();
                Response.StatusCode = 400;
                Response.ContentType = "text/plain";
                Response.Write("Missing file name");
                HttpContext.Current.ApplicationInstance.CompleteRequest();
                return;
            }

            string safe = Path.GetFileName(HttpUtility.UrlDecode(fileName));
            string path = Path.Combine(Server.MapPath("~/App_Data/FailedUploads"), safe);

            if (!File.Exists(path))
            {
                Response.ClearHeaders();
                Response.ClearContent();
                Response.StatusCode = 404;
                Response.ContentType = "text/plain";
                Response.Write("File not found");
                HttpContext.Current.ApplicationInstance.CompleteRequest();
                return;
            }

            try
            {
                byte[] fileBytes = File.ReadAllBytes(path);

                Response.ClearHeaders();
                Response.ClearContent();
                Response.ContentType = "text/csv";
                Response.AddHeader("Content-Disposition", $"attachment; filename=\"{safe}\"");
                Response.AddHeader("Content-Length", fileBytes.Length.ToString());
                Response.BinaryWrite(fileBytes);
                Response.Flush();

                Response.SuppressContent = true;
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (HttpException)
            {
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                Response.ClearHeaders();
                Response.ClearContent();
                Response.ContentType = "text/plain";
                Response.Write("Error streaming file: " + ex.Message);
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }

        // Helper: read DataRow cell using multiple possible header names
        private string GetCell(DataRow row, params string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                foreach (DataColumn col in row.Table.Columns)
                {
                    if (string.Equals(col.ColumnName.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        var v = row[col];
                        if (v == null || v == DBNull.Value) return string.Empty;
                        return v.ToString().Trim();
                    }
                }
            }

            foreach (DataColumn col in row.Table.Columns)
            {
                string cn = col.ColumnName.Replace(" ", "").Replace("_", "").ToLower();
                foreach (var p in possibleNames)
                {
                    string pn = p.Replace(" ", "").Replace("_", "").ToLower();
                    if (cn == pn)
                    {
                        var v = row[col];
                        if (v == null || v == DBNull.Value) return string.Empty;
                        return v.ToString().Trim();
                    }
                }
            }

            return string.Empty;
        }

        // Load existing emails from DB to HashSet
        private HashSet<string> GetExistingEmails()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var csSetting = System.Configuration.ConfigurationManager.ConnectionStrings["MyConn"]
                            ?? System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"]
                            ?? System.Configuration.ConfigurationManager.ConnectionStrings["TestAPIConnection"];

            if (csSetting == null || string.IsNullOrWhiteSpace(csSetting.ConnectionString))
            {
                return set;
            }
            string connString = csSetting.ConnectionString;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Email FROM dbo.Users_Register WHERE Email IS NOT NULL", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var e = dr["Email"] as string;
                            if (!string.IsNullOrEmpty(e)) set.Add(e.Trim().ToLower());
                        }
                    }
                }
            }
            return set;
        }

        // Get role by name or any role fallback
        private int GetRoleIdByName(SqlConnection conn, string roleName)
        {
            if (conn == null || string.IsNullOrEmpty(roleName)) return -1;
            using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 RoleId FROM dbo.Roles WHERE RoleName = @roleName", conn))
            {
                cmd.Parameters.AddWithValue("@roleName", roleName);
                var obj = cmd.ExecuteScalar();
                if (obj != null && obj != DBNull.Value) return Convert.ToInt32(obj);
            }
            return -1;
        }
        private int GetAnyRoleId(SqlConnection conn)
        {
            if (conn == null) return -1;
            using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 RoleId FROM dbo.Roles", conn))
            {
                var obj = cmd.ExecuteScalar();
                if (obj != null && obj != DBNull.Value) return Convert.ToInt32(obj);
            }
            return -1;
        }
        private bool RoleExists(SqlConnection conn, int roleId)
        {
            if (conn == null) return false;
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM dbo.Roles WHERE RoleId = @roleId", conn))
            {
                cmd.Parameters.AddWithValue("@roleId", roleId);
                var obj = cmd.ExecuteScalar();
                return (obj != null && Convert.ToInt32(obj) > 0);
            }
        }

        // CSV helpers
        private string CreateCsvRow(int rowNumber, string firstName, string lastName, string email, string password, string gender, string dob, string country, string state, string city, string address, string terms, string roleId, string reason)
        {
            string[] fields = new string[] { rowNumber.ToString(), firstName ?? "", lastName ?? "", email ?? "", password ?? "", gender ?? "", dob ?? "", country ?? "", state ?? "", city ?? "", address ?? "", terms ?? "", roleId ?? "", reason ?? "" };
            for (int i = 0; i < fields.Length; i++) { if (fields[i] == null) fields[i] = ""; fields[i] = fields[i].Replace("\"", "\"\""); fields[i] = $"\"{fields[i]}\""; }
            return string.Join(",", fields);
        }

        // Hash + truncate helpers
        private string ComputeSha256Hash(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return null;
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(raw));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }
        private string Truncate(string s, int maxLen) { if (string.IsNullOrEmpty(s)) return s; if (s.Length <= maxLen) return s; return s.Substring(0, maxLen); }

        // DTO
        public class UserDto
        {
            public int UserID { get; set; }
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public string Email { get; set; } = "";
            public string Gender { get; set; } = "";
            public DateTime? DOB { get; set; }
            public string Country { get; set; } = "";
        }
    }
}
