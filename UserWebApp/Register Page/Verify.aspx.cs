using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace UserWebApp
{
    public partial class Verify : Page
    {
        // Keep false in production. True shows extra debug heuristics
        private const bool showDebug = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    string token = Request.QueryString["token"];
                    token = token == null ? "" : HttpUtility.UrlDecode(token).Trim();

                    if (string.IsNullOrEmpty(token))
                    {
                        ShowError("Invalid verification link.");
                        ShowLoginButton();
                        return;
                    }

                    string cs = ConfigurationManager.ConnectionStrings["MyConn"]?.ConnectionString;
                    if (string.IsNullOrEmpty(cs))
                    {
                        ShowError("Database connection not configured. Please check web.config.");
                        return;
                    }

                    using (var con = new SqlConnection(cs))
                    {
                        con.Open();

                        int userId = 0;
                        bool alreadyVerified = false;
                        DateTime? tokenExpiry = null;

                        // 1) Try find user by token
                        using (var checkCmd = new SqlCommand(@"
                            SELECT TOP 1 UserID, Email, IsVerified, TokenExpiry, VerificationToken
                            FROM Users_Register
                            WHERE LTRIM(RTRIM(ISNULL(VerificationToken,''))) = LTRIM(RTRIM(@Token))
                        ", con))
                        {
                            checkCmd.Parameters.Add("@Token", SqlDbType.NVarChar, 200).Value = token;

                            using (var reader = checkCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    userId = reader["UserID"] != DBNull.Value ? Convert.ToInt32(reader["UserID"]) : 0;
                                    alreadyVerified = reader["IsVerified"] != DBNull.Value && Convert.ToBoolean(reader["IsVerified"]);

                                    if (reader["TokenExpiry"] != DBNull.Value)
                                    {
                                        DateTime tx;
                                        if (DateTime.TryParse(reader["TokenExpiry"].ToString(), out tx))
                                            tokenExpiry = tx;
                                    }
                                }
                                else
                                {
                                    reader.Close();

                                    // Secondary heuristic: recently verified user
                                    using (var recentCmd = new SqlCommand(@"
                                        SELECT TOP 1 UserID, Email, IsVerified, UpdatedAt
                                        FROM Users_Register
                                        WHERE IsVerified = 1
                                          AND UpdatedAt >= DATEADD(DAY, -7, GETUTCDATE())
                                        ORDER BY UpdatedAt DESC
                                    ", con))
                                    {
                                        using (var r2 = recentCmd.ExecuteReader())
                                        {
                                            if (r2.Read())
                                            {
                                                ShowSuccess("Your email is already verified. You can login now.");
                                                ShowLoginButton();
                                                return;
                                            }
                                        }
                                    }

                                    ShowError("Invalid or expired verification link.");
                                    ShowLoginButton();
                                    return;
                                }
                            }
                        }

                        // If we found a row by token:
                        if (alreadyVerified)
                        {
                            ShowSuccess("Your email is already verified. You can login now.");
                            ShowLoginButton();
                            return;
                        }

                        // Check expiry if present
                        if (tokenExpiry.HasValue && tokenExpiry.Value.ToUniversalTime() < DateTime.UtcNow)
                        {
                            ShowError("Invalid or expired verification link.");
                            ShowLoginButton();
                            return;
                        }

                        // Mark verified
                        using (var updateCmd = new SqlCommand(@"
                            UPDATE Users_Register
                            SET IsVerified = 1,
                                VerificationToken = NULL,
                                TokenExpiry = NULL,
                                UpdatedAt = GETDATE()
                            WHERE UserID = @UserID
                        ", con))
                        {
                            updateCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                            int rows = updateCmd.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                ShowSuccess("Thank you! Your email has been verified successfully. You can now login.");
                                ShowLoginButton();
                            }
                            else
                            {
                                ShowError("Failed to verify your account. The link may be invalid or expired.");
                                ShowLoginButton();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError("An error occurred: " + Server.HtmlEncode(ex.Message));
                    ShowLoginButton();
                }
            }
        }

        private void ShowError(string msg)
        {
            lblMsg.CssClass = "error";
            lblMsg.Text = Server.HtmlEncode(msg);
        }

        private void ShowSuccess(string msg)
        {
            lblMsg.CssClass = "success";
            lblMsg.Text = Server.HtmlEncode(msg);
        }

        private void ShowLoginButton()
        {
            // Use extensionless route if you prefer, or add .aspx
            hlLogin.NavigateUrl = ResolveUrl("~/Register Page/Login");
            hlLogin.Visible = true;
        }
    }
}
