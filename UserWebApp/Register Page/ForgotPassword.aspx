<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="ForgotPassword.aspx.cs"
    Inherits="UserWebApp.ForgotPassword"
    MasterPageFile="~/Site.Master"
    Async="true" %>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .form-area { max-width:720px; margin:0; }
        .card { background:#fff; border-radius:10px; padding:20px; box-shadow:0 6px 18px rgba(16,24,40,0.06); }
        h3 { font-size:28px; margin-bottom:14px; color:#111827; }
        .muted { color:#6b7280; font-size:13px; }
        .form-input { padding:9px 12px; border-radius:6px; border:1px solid #d1d5db; width:100%; box-sizing:border-box; }
        .btn { border-radius:8px; padding:8px 14px; font-weight:600; }
        .btn-primary { background:#1366d6; color:#fff; border:1px solid rgba(10,56,120,0.12); }
        .btn-secondary { background:#f3f4f6; color:#111827; border:1px solid #e6e9ee; }
        .mb-2 { margin-bottom:12px; }
    </style>

    <div class="form-area">
        <div class="card">
            <h3>Forgot Password</h3>

            <asp:Label ID="lblMsg" runat="server" ForeColor="LightCoral" />

            <div class="mb-2">
                <asp:Label runat="server" AssociatedControlID="txtEmail" Text="Email" CssClass="muted" /><br />
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-input" Placeholder="Enter your registered email" />
                <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                    ErrorMessage="Email is required" ForeColor="Red" Display="Dynamic" />
            </div>

            <div style="margin-top:12px;">
                <asp:Button ID="btnSend" runat="server" Text="Send Reset Link" CssClass="btn btn-primary" OnClick="btnSend_Click" />
                &nbsp;
                <asp:Button ID="btnBack" runat="server" Text="Back to Login" CssClass="btn btn-secondary" OnClick="btnBack_Click" />
            </div>

            <div style="margin-top:14px;" class="muted">
                Enter the email address you used to register. We will send a secure password reset link.
            </div>
        </div>
    </div>

</asp:Content>
