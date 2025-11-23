<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Verify.aspx.cs" Inherits="UserWebApp.Verify" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Email Verification</title>
    <meta name="viewport" content="width=device-width,initial-scale=1" />
    <style>
        body { font-family: Segoe UI, Arial, sans-serif; background:#f5f7fb; margin:0; padding:0; }
        .container { max-width:980px; margin:40px auto; padding:20px; }
        .card { max-width:760px; margin:40px auto; background:#fff; padding:28px; border-radius:10px; box-shadow:0 8px 24px rgba(0,0,0,0.06); }
        .center { text-align:center; }
        .success { color: #155724; background: #d4edda; padding:14px; border-radius:8px; display:inline-block; }
        .error { color: #721c24; background: #f8d7da; padding:14px; border-radius:8px; display:inline-block; }
        .btn { display:inline-block; padding:12px 20px; background:#4CAF50; color:#fff; text-decoration:none; border-radius:8px; margin-top:18px; }
        .brand { font-weight:700; color:#0d6efd; }
        .sub { color:#666; margin-top:8px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="card">
                <div class="center">
                    <h1 style="margin:0;">Email Verification</h1>
                    <p class="sub">Verify your account to access all features.</p>
                </div>

                <div style="margin-top:20px; text-align:center;">
                    <asp:Label ID="lblMsg" runat="server" Text="" EnableViewState="false"></asp:Label>
                </div>

                <div class="center" style="margin-top:18px;">
                    <asp:HyperLink ID="hlLogin" runat="server" CssClass="btn" Visible="false">Go to Login</asp:HyperLink>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
