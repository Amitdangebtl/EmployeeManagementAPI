<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReactDemo.aspx.cs" Inherits="UserWebApp.ReactDemo" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <div id="react-root"></div>

  <!-- load external file (Babel in Site.Master already present) -->
  <script type="text/babel" src="<%= ResolveUrl("~/Scripts/ReactDemo.js") %>"></script>
</asp:Content>
