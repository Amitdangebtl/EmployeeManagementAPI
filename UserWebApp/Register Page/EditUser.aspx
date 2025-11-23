<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="EditUser.aspx.cs"
    Inherits="UserWebApp.RegisterPage.EditUser"
    MasterPageFile="~/Site.Master"
    Async="true" %>

<asp:Content ID="c1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .form-area { max-width:720px; margin:0; }
        h3 { font-size:28px; margin-bottom:14px; color:#111827; }
        .form-row { display:flex; gap:14px; align-items:flex-start; margin-bottom:14px; }
        .form-label { width:140px; color:#374151; font-weight:600; margin-top:6px; }
        .form-input { padding:9px 12px; border-radius:6px; border:1px solid #d1d5db; width:100%; box-sizing:border-box; }
        .btn { border-radius:8px; padding:8px 14px; font-weight:600; }
        .btn-success { background:#198754; color:#fff; }
        .btn-secondary { background:#f3f4f6; color:#111827; }
    </style>

    <div class="form-area">
        <asp:HiddenField ID="hdnId" runat="server" />
        <asp:HiddenField ID="hdnPwdHash" runat="server" />
        <asp:HiddenField ID="hdnProfile" runat="server" />
        <asp:HiddenField ID="hdnTerms" runat="server" />
        <asp:HiddenField ID="hdnCreatedAt" runat="server" />

        <h3>Edit User</h3>
        <asp:Label ID="lblMsg" runat="server" ForeColor="Red"></asp:Label>

        <div class="form-row">
            <label class="form-label">First Name</label>
            <asp:TextBox ID="txtFirst" runat="server" CssClass="form-input" MaxLength="100" />
        </div>

        <div class="form-row">
            <label class="form-label">Last Name</label>
            <asp:TextBox ID="txtLast" runat="server" CssClass="form-input" MaxLength="100" />
        </div>

        <div class="form-row">
            <label class="form-label">Email</label>
            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-input" MaxLength="100" />
        </div>

        <div class="form-row">
            <label class="form-label">Gender</label>
            <asp:RadioButtonList ID="rblGender" runat="server" RepeatDirection="Horizontal" />
        </div>

        <div class="form-row">
            <label class="form-label">DOB (DD/MM/YYYY)</label>
            <asp:TextBox ID="txtDob" runat="server" CssClass="form-input" MaxLength="10" />
        </div>

        <div class="form-row">
            <label class="form-label">Country</label>
            <asp:TextBox ID="txtCountry" runat="server" CssClass="form-input" MaxLength="100" />
        </div>

        <div class="form-row">
            <label class="form-label">Address</label>
            <asp:TextBox ID="txtAddress" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-input" MaxLength="200" />
        </div>

        <div style="margin-top:12px;">
            <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-success" OnClick="btnSave_Click" />
            <asp:HyperLink ID="lnkBack" runat="server" Text="Back to List" CssClass="btn btn-secondary ms-2" NavigateUrl="~/Register%20Page/UsersList.aspx" />
        </div>
    </div>

</asp:Content>
