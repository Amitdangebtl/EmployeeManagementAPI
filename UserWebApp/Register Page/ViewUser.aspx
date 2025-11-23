<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="ViewUser.aspx.cs"
    Inherits="UserWebApp.RegisterPage.ViewUser"
    MasterPageFile="~/Site.Master"
    Async="true" %>

<asp:Content ID="c1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .page-wrap { max-width:1100px; margin:0; }
        h3 { font-size:28px; margin-bottom:14px; color:#111827; }
        .img-col { max-width:220px; }
        .table { background:#fff; border-radius:8px; }
        .btn { border-radius:8px; padding:8px 14px; font-weight:600; }
    </style>

    <div class="page-wrap">
        <h3 class="mb-3">User Details</h3>
        <asp:Label ID="lblMsg" runat="server" ForeColor="Red"></asp:Label>

        <div class="row g-4">
            <div class="col-md-3 img-col">
                <asp:Image ID="imgProfile" runat="server" CssClass="img-thumbnail w-100" AlternateText="Profile" />
            </div>

            <div class="col-md-9">
                <table class="table table-bordered table-sm">
                    <tr><th style="width:220px">User ID</th><td><asp:Label ID="lblId" runat="server" /></td></tr>
                    <tr><th>First Name</th><td><asp:Label ID="lblFirst" runat="server" /></td></tr>
                    <tr><th>Last Name</th><td><asp:Label ID="lblLast" runat="server" /></td></tr>
                    <tr><th>Email</th><td><asp:Label ID="lblEmail" runat="server" /></td></tr>
                    <tr><th>Gender</th><td><asp:Label ID="lblGender" runat="server" /></td></tr>
                    <tr><th>DOB</th><td><asp:Label ID="lblDob" runat="server" /></td></tr>
                    <tr><th>Country</th><td><asp:Label ID="lblCountry" runat="server" /></td></tr>
                    <tr><th>State</th><td><asp:Label ID="lblState" runat="server" /></td></tr>
                    <tr><th>City</th><td><asp:Label ID="lblCity" runat="server" /></td></tr>
                    <tr><th>Address</th><td><asp:Label ID="lblAddress" runat="server" /></td></tr>
                    <tr><th>Terms Accepted</th><td><asp:Label ID="lblTerms" runat="server" /></td></tr>
                </table>

                <asp:HyperLink runat="server" ID="lnkBack" NavigateUrl="~/Register%20Page/UsersList.aspx" CssClass="btn btn-secondary">Back to List</asp:HyperLink>
                &nbsp;
                <asp:HyperLink runat="server" ID="lnkEdit" CssClass="btn btn-primary">Edit</asp:HyperLink>
            </div>
        </div>
    </div>

</asp:Content>
