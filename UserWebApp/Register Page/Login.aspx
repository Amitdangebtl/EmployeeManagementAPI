<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="Login.aspx.cs"
    Inherits="UserWebApp.LoginPage.Login"
    MasterPageFile="~/Site.Master"
    Async="true" %>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">

    <!-- INLINE CSS - temporary to force correct look until external CSS works -->
    <style>
        /* page container */
        .form-area {
            max-width: 720px;
            margin: 0;
        }
        h3.page-title { font-size:28px; font-weight:700; margin-bottom:14px; color:#111827; }

        .form-row { display:flex; gap:14px; align-items:flex-start; margin-bottom:14px; }
        .form-label { width:140px; color:#374151; font-weight:600; margin-top:6px; }

        .form-input {
            padding:9px 12px; border-radius:6px; border:1px solid #d1d5db; background:#fff; color:#111827;
            min-width:220px; width:320px; box-sizing:border-box; transition:box-shadow .12s ease, border-color .12s ease;
        }
        .form-input:focus { border-color:#1366d6; box-shadow:0 4px 12px rgba(19,102,214,0.08); outline:none; }

        .btn { border-radius:8px; padding:8px 14px; font-weight:600; }
        .btn-primary { background:#1366d6; color:#fff; border:1px solid rgba(10,56,120,0.12); }
        .btn-secondary { background:#f3f4f6; color:#111827; border:1px solid #e6e9ee; }
    </style>

    <div class="form-area">
        <h3 class="page-title">Login</h3>

        <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="text-danger" />

        <div class="form-row">
            <label class="form-label" for="txtEmail">Email</label>
            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-input" />
        </div>

        <div class="form-row">
            <label class="form-label" for="txtPassword">Password</label>
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-input" />
        </div>

        <div class="d-flex" style="gap:10px; margin-top:12px;">
            <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn btn-primary" OnClick="btnLogin_Click" />
            <asp:Button ID="btnForgotPassword" runat="server" Text="Forgot Password?" CssClass="btn btn-secondary" OnClick="btnForgotPassword_Click" />
        </div>

        <asp:Label ID="lblLoginMsg" runat="server" CssClass="text-danger" style="display:block; margin-top:10px;" />
    </div>

</asp:Content>
