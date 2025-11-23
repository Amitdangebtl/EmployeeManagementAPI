<%@ Page Title="Home" Language="C#" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs"
    Inherits="UserWebApp._Default"
    MasterPageFile="~/Site.Master" %>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <!-- SIMPLE HERO -->
    <section style="padding:40px 0; text-align:center;">
        <div style="max-width:900px; margin:0 auto; background:#fff; border-radius:12px; padding:36px; box-shadow:0 8px 30px rgba(2,6,23,0.06);">
            <h1 style="margin:0 0 12px; font-size:34px; color:#0b1220;">User Management System</h1>
            <p style="margin:0 0 18px; color:#475569; font-size:16px;">
                Simple, secure and fast web app to register and manage users.
            </p>

            <div style="margin-top:18px;">
               <a href='<%= ResolveUrl("~/Register Page/Register.aspx") %>' 
   class="btn btn-primary btn-lg" 
   style="padding:10px 18px; border-radius:8px;">
    Register
</a>
            </div>
        </div>
    </section>

    <!-- SIMPLE FEATURES (very short) -->
    <section style="max-width:900px; margin:18px auto; color:#374151;">
        <ul style="padding-left:18px; line-height:1.8; font-size:15px;">
            <li>Register new users with profile and location.</li>
            <li>Search, view and edit user records.</li>
            <li>Secure login, logout and password reset.</li>
        </ul>
    </section>

    <!-- SIMPLE FOOTER CTA -->
    <section style="max-width:900px; margin:22px auto 40px; text-align:center; color:#6b7280;">
        <small>Built with ASP.NET WebForms • Designed to be simple and usable</small>
    </section>

</asp:Content>
